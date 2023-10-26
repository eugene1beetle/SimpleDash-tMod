using Microsoft.Xna.Framework;
using SimpleDash.Common.Configs;
using System;
using Terraria;
using Terraria.ModLoader;

namespace SimpleDash.Core.System
{
    public class DashPlayer : ModPlayer
    {
        // These indicate what direction is what in the timer arrays used
        public const int DashDown = 0;
        public const int DashUp = 1;
        public const int DashRight = 2;
        public const int DashLeft = 3;

        // Time (frames) that needs to trigger double tap, 15 ticks (1/4 second)
        public const int DashDoubleTapTime = 15;

        // Time (frames) between starting dashes. If this is shorter than DashDuration you can start a new dash before an old one has finished
        public int DashCooldown = int.Parse(ModContent.GetInstance<SimpleDashConfig>().DashCooldown);

        // The initial velocity. got from configs
        public float DashVelocity = float.Parse(ModContent.GetInstance<SimpleDashConfig>().DashVelocity.Replace(',', '.'));

        // Allowing vertical dash
        public bool AllowVerticalDash = ModContent.GetInstance<SimpleDashConfig>().AllowVerticalDash;

        // Resetting dash
        public bool ResetDashWhenVelocity0 = ModContent.GetInstance<SimpleDashConfig>().ResetDashWhenVelocity0;

        // The direction the player has double tapped. Defaults to -1 for no dash double tap
        public int DashDir = -1;

        public int DashDelay = 0; // frames remaining till we can dash again
        public int DashTimer = 0; // frames remaining in the dash

        public override void ResetEffects()
        {
            // ResetEffects is called not long after player.doubleTapCardinalTimer's values have been set
            // When a directional key is pressed and released, timer starts with DashDoubleTapTime during which a second press activates a dash
            // If the timers are set to 15, then this is the first press just processed by the vanilla logic. Otherwise, it's a double-tap
            if (AllowVerticalDash && Player.controlDown && Player.releaseDown && Player.doubleTapCardinalTimer[DashDown] < DashDoubleTapTime)
            {
                DashDir = DashDown;
            }
            else if (AllowVerticalDash && Player.controlUp && Player.releaseUp && Player.doubleTapCardinalTimer[DashUp] < DashDoubleTapTime)
            {
                DashDir = DashUp;
            }
            else if (Player.controlRight && Player.releaseRight && Player.doubleTapCardinalTimer[DashRight] < DashDoubleTapTime)
            {
                DashDir = DashRight;
            }
            else if (Player.controlLeft && Player.releaseLeft && Player.doubleTapCardinalTimer[DashLeft] < DashDoubleTapTime)
            {
                DashDir = DashLeft;
            }
            else
            {
                DashDir = -1;
            }
        }

        // This is the perfect place to apply dash movement, it's after the vanilla movement code, and before the player's position is modified based on velocity.
        // If they double tapped this frame, they'll move fast this frame
        public override void PreUpdateMovement()
        {
            // if the player can use our dash, has double tapped in a direction, and our dash isn't currently on cooldown
            if (CanUseDash() && DashDir != -1 && DashDelay == 0)
            {
                Vector2 newVelocity = Player.velocity;

                switch (DashDir)
                {
                    // Only apply the dash velocity if our current speed in the wanted direction is less than DashVelocity
                    case DashUp when Player.velocity.Y > -DashVelocity:
                    case DashDown when Player.velocity.Y < DashVelocity:
                        {
                            // Y-velocity is set here
                            // If the direction requested was DashUp, then we adjust the velocity to make the dash appear "faster" due to gravity being immediately in effect
                            // This adjustment is roughly 1.3x the intended dash velocity
                            float dashDirection = DashDir == DashDown ? 1 : -1.3f;
                            newVelocity.Y = dashDirection * DashVelocity;
                            break;
                        }
                    case DashLeft when Player.velocity.X > -DashVelocity:
                    case DashRight when Player.velocity.X < DashVelocity:
                        {
                            // X-velocity is set here
                            float dashDirection = DashDir == DashRight ? 1 : -1;
                            newVelocity.X = dashDirection * DashVelocity;
                            break;
                        }
                    default:
                        return; // Not moving fast enough, so don't start our dash
                }

                // Start our dash
                DashDelay = DashCooldown;
                DashTimer = (int)(DashVelocity * 8);
                Player.velocity = newVelocity;
            }

            if (DashDelay > 0)
            {
                DashDelay--;
                
                // Setting Dash delay to 0 if player has no velocity
                if (!AllowVerticalDash && ResetDashWhenVelocity0 && Math.Abs(Player.velocity.X) < 0.33)
                {
                    DashDelay = 0;
                }
            }    

            if (DashTimer > 0)
            { 
                // Dash is active
                // This is where we set the afterimage effect.  You can replace these two lines with whatever you want to happen during the dash
                // Some examples include:  spawning dust where the player is, adding buffs, making the player immune, etc.
                // Here we take advantage of "player.eocDash" and "player.armorEffectDrawShadowEOCShield" to get the Shield of Cthulhu's afterimage effect
                Player.eocDash = DashTimer;
                Player.armorEffectDrawShadowEOCShield = true;

                // Count down frames remaining
                DashTimer--;

                // Setting Dash delay to 0 if player has no velocity
                if (!AllowVerticalDash && ResetDashWhenVelocity0 && Math.Abs(Player.velocity.X) < 0.33)
                {
                    DashTimer = 0;
                    Player.eocDash = 0;
                    Player.armorEffectDrawShadowEOCShield = false;
                }
            }
        }

        private bool CanUseDash()
        {
            return Player.dashType == 0 // Player doesn't have Tabi or EoCShield equipped (give priority to those dashes)
                && !Player.setSolar // Player isn't wearing solar armor
                && !Player.mount.Active // Player isn't mounted, since dashes on a mount look weird
                && Player.grappling[0] < 0; // Player isn't using grappling hook
        }
    }
}
