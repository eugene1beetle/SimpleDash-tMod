using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace SimpleDash.Common.Configs
{
    public class SimpleDashConfig : ModConfig
    {
        // Dash should be server side synced feature
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [DefaultValue("4.5")]
        public string DashVelocity;

        [DefaultValue("90")]
        public string DashCooldown;

        [DefaultValue(true)]
        public bool ResetDashWhenVelocity0;

        [DefaultValue(false)]
        //[ReloadRequired]
        public bool AllowVerticalDash;
    }
}
