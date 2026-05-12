namespace TinyTweaks
{
    public class CapFeelsLike : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }

        private static bool IsNearCampFire()
        {
            return GameManager.GetFireManagerComponent().GetDistanceToClosestFire(GameManager.GetPlayerTransform().position) < GameManager.GetBodyHarvestManagerComponent().m_RadiusToThawFromFire;
        }
        

        [HarmonyPatch(typeof(Freezing), "CalculateBodyTemperature")]
        private class FeelsLikeCap
        {
            
            public static void Postfix(ref float __result)
            {
                if ((GameManager.GetWeatherComponent() && GameManager.GetWeatherComponent().IsIndoorEnvironment()) || IsNearCampFire()) return;
                if (Settings.options.capHigh != 0 && __result > Settings.options.capHigh) __result = Settings.options.capHigh;
                if (Settings.options.capLow != 0 && __result < Settings.options.capLow) __result = Settings.options.capLow;
            }
        }

    }
}


// ===== Settings.cs =====

﻿using ModSettings;

namespace TinyTweaks
{
    internal static class Settings
    {
        public static TTSettings options;

        public static void OnLoad()
        {
            options = new TTSettings();
            options.AddToModSettings("[Tiny Tweaks]");
        }
    }

    internal class TTSettings : JsonModSettings
    {
        [Section("Cap Feels Like Temperature")]

        [Name("Cap high")]
        [Description("Does what it says (in Celsius). \n\n0 = no cap")]
        [Slider(-10, 50)]
        public int capHigh = 0;

        [Name("Cap low")]
        [Description("Does what it says (in Celsius). \n\n0 = no cap")]
        [Slider(-50, 10)] 
        public int capLow = 0;

        protected override void OnConfirm()
        {
            base.OnConfirm();
        }
    }

}
