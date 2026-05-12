using src;

namespace TinyTweaks
{
    class FallDeathGoat : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }

        [HarmonyPatch(typeof(FallDeathTrigger), nameof(FallDeathTrigger.OnTriggerEnter))]
        private static class FallTriggerDisable
        {
            internal static void Postfix(ref Collider c)
            {
                if (c.gameObject.CompareTag("Player"))
                {
                    if (Settings.options.notify) HUDMessage.AddMessage("Fall death trigger passed");
                    GameManager.GetFallDamageComponent().m_DieOnNextFall = false;
                }
            }
        }
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        private static class ApplyFallDamageMultipler
        {
            internal static void Postfix()
            {
                GameManager.GetFallDamageComponent().m_DamagePerMeter = Settings.options.fallDamageMult;
            }
        }
    }
}


// ===== Settings.cs =====

﻿using ModSettings;

namespace src
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
        [Section("Fall Death Goat")]

        [Name("Fall Damage")]
        [Description("Fall damage multiplier, applied per meter of free fall.\n\n Vanilla: 3")]
        [Slider(1, 12)]
        public int fallDamageMult = 6;

        [Name("Notify")]
        [Description("Show notification when death wall is entered")]
        public bool notify = false;

        protected override void OnConfirm()
        {
            GameManager.GetFallDamageComponent().m_DamagePerMeter = fallDamageMult;
            base.OnConfirm();
        }
    }

}
