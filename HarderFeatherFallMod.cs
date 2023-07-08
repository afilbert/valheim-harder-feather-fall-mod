using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace ValheimMovementMods
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    [BepInProcess("valheim.exe")]
    public class HarderFeatherFallMod : BaseUnityPlugin
    {
        const string pluginGUID = "afilbert.ValheimHarderFeatherFallMod";
        const string pluginName = "Valheim - Harder Feather Fall Mod";
        const string pluginVersion = "0.1.0";
        public static ManualLogSource logger;

        private readonly Harmony _harmony = new Harmony(pluginGUID);
        public static HarderFeatherFallMod _plugin;

        public static bool Started = false;

        private static string FeatherFallName = "$se_slowfall_name";
        private static string WispLightName = "$item_demister";
        private static bool FeatherFallEnabled = false;
        private static bool WispLightEnabled = false;

        private static float StaminaPercentage = 0;
        private static float InitialMaxFallSpeed = 0;

        public static ConfigEntry<bool> EnableToggle;
        public static ConfigEntry<bool> EnableEverywhere;

        void Awake()
        {
            _plugin = this;
            logger = Logger;


            _harmony.PatchAll();
            EnableToggle = Config.Bind<bool>("Mod", "Enable Mod", true, "Enable this mod");
            EnableEverywhere = Config.Bind<bool>("Mod", "Override: Enable even when Wisplight equipped", false, "Enable even when Wisplight equipped");
        }

        [HarmonyPatch(typeof(Player), "SetControls")]
        private class ToggleMovement
        {
            private static void Prefix(ref Player __instance, ref Vector3 movedir, ref bool run, ref bool autoRun, ref bool crouch, ref Vector3 ___m_lookDir, ref Vector3 ___m_moveDir, ref bool ___m_autoRun, ref bool ___m_crouchToggled, ref string ___m_actionAnimation, ref List<Player.MinorActionData> ___m_actionQueue)
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

                if (FeatherFallEnabled)
                {
                    ___m_fallDamageModifier = -1;
                }
                else
                {
                    ___m_fallDamageModifier = 0;
                }

                if (!WispLightEnabled || EnableEverywhere.Value)
                {
                    if (FeatherFallEnabled)
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
