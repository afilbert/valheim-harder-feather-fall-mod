using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace ValheimMovementMods
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("valheim.exe")]
    public class HarderFeatherFallMod : BaseUnityPlugin
    {
        const string pluginGUID = "afilbert.ValheimHarderFeatherFallMod";
        const string pluginName = "Valheim - Harder Feather Fall Mod";
        const string pluginVersion = "0.2.0";
        public static ManualLogSource logger;
        public static HarderFeatherFallMod plugin;

        private readonly Harmony _harmony = new Harmony(pluginGUID);

        public static bool Started = false;

        private static string FeatherFallName = "$se_slowfall_name";
        private static string WispLightName = "$item_demister";
        private static bool FeatherFallEnabled = false;
        private static bool WispLightEnabled = false;
        private static bool PlayerFalling = false;

        private static float StaminaPercentage = 0;
        private static float InitialMaxFallSpeed = 0;
        private static float StamDrainConversion = 0;

        public static ConfigEntry<bool> EnableToggle;
        public static ConfigEntry<bool> EnableEverywhere;
        public static ConfigEntry<bool> AltMode;
        public static ConfigEntry<int> AltStamDrain;
        public static ConfigEntry<bool> HardcoreMode;

        void Awake()
        {
            plugin = this;
            logger = Logger;


            _harmony.PatchAll();
            EnableToggle = Config.Bind<bool>("Mod", "Enable Mod", true, "Enable this mod");
            EnableEverywhere = Config.Bind<bool>("Override", "Override: Enable even when Wisplight equipped", false, "Enable even when Wisplight equipped");
            AltMode = Config.Bind<bool>("Alternative Mode", "Alternative: Uses stamina to stay aloft", false, "Uses stamina to stay aloft");
            AltStamDrain = Config.Bind<int>("Alternative Mode", "Alternative: Stamina drain unit (larger drains faster)", 10, "Stamina drain unit");
            HardcoreMode = Config.Bind<bool>("Hardcore Mode", "Hardcore: Wisplight bound and uses stamina", false, "Requires Wisplight for normal use and uses stamina to stay aloft");
            StamDrainConversion = AltStamDrain.Value / 100f;
        }

        [HarmonyPatch(typeof(Player), "SetControls")]
        private class SetControlsPatch
        {
            private static void Prefix(ref Player __instance, ref Vector3 movedir, ref bool ___m_groundContact)
            {
                if (!EnableToggle.Value)
                {
                    return;
                }

                StatusEffect slowFallEffect = __instance.GetSEMan().GetStatusEffects().Find(effect => effect.m_name == FeatherFallName);
                StatusEffect whispLightEffect = __instance.GetSEMan().GetStatusEffects().Find(effect => effect.m_name == WispLightName);
                FeatherFallEnabled = slowFallEffect != null;
                WispLightEnabled = whispLightEffect != null;
                StaminaPercentage = __instance.GetStaminaPercentage();

                PlayerFalling = __instance.GetVelocity().y < 0 && !___m_groundContact;

                if (AltMode.Value && FeatherFallEnabled && PlayerFalling && (!WispLightEnabled || HardcoreMode.Value))
                {
                    __instance.UseStamina(StamDrainConversion);
                }
            }
        }

        [HarmonyPatch(typeof(SE_Stats), "ModifyWalkVelocity")]
        private class ModifyWalkVelocityPatch
        {
            private static void Prefix(ref Vector3 vel, ref float ___m_maxMaxFallSpeed)
            {
                if (!EnableToggle.Value)
                {
                    return;
                }

                if (InitialMaxFallSpeed == 0)
                {
                    InitialMaxFallSpeed = ___m_maxMaxFallSpeed;
                }

                if (AltMode.Value && !HardcoreMode.Value)
                {
                    if (StaminaPercentage == 0)
                    {
                        ___m_maxMaxFallSpeed = 0;
                    }
                    else
                    {
                        ___m_maxMaxFallSpeed = 5;
                    }
                    return;
                }

                if (!WispLightEnabled || EnableEverywhere.Value)
                {
                    if (StaminaPercentage != 0)
                    {
                        ___m_maxMaxFallSpeed = InitialMaxFallSpeed / StaminaPercentage;
                    }
                } else
                {
                    ___m_maxMaxFallSpeed = InitialMaxFallSpeed;
                }

                if (AltMode.Value && StaminaPercentage == 0)
                {
                    ___m_maxMaxFallSpeed = 0;
                }
            }
        }

        [HarmonyPatch(typeof(SE_Stats), "ModifyFallDamage")]
        private class ModifyFallDamagePatch
        {
            private static void Prefix(float baseDamage, ref float damage, ref float ___m_fallDamageModifier, ref float ___m_maxMaxFallSpeed)
            {
                if (!EnableToggle.Value)
                {
                    return;
                }

                if (AltMode.Value && !HardcoreMode.Value)
                {
                    if (StaminaPercentage == 0)
                    {
                        ___m_fallDamageModifier = 0;
                    } else
                    {
                        ___m_fallDamageModifier = -1;
                    }
                    return;
                }

                bool ApplyFeatherFall = (FeatherFallEnabled && (AltMode.Value && StaminaPercentage != 0)) || (FeatherFallEnabled && !AltMode.Value);

                if (ApplyFeatherFall)
                {
                    ___m_fallDamageModifier = -1;
                }
                else
                {
                    ___m_fallDamageModifier = 0;
                }

                if (!WispLightEnabled || EnableEverywhere.Value)
                {
                    if (ApplyFeatherFall)
                    {
                        float stamDiff = 1 - StaminaPercentage;
                        damage = 0;
                        ___m_fallDamageModifier = stamDiff;
                    }
                }
            }
        }
    }
}
