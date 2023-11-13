using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        public static int FeatherFallStatusIndex = -1;
        public static int AppliedDamageIndex = -1;

        private static string FeatherFallName = "$se_slowfall_name";
        private static string WispLightName = "$item_demister";
        private static bool FeatherFallEnabled = false;
        private static bool WispLightEnabled = false;
        private static bool PlayerFalling = false;

        private static float StaminaPercentage = 0;
        private static float InitialMaxFallSpeed = 5f;
        public static float ElapsedFreeFallTime = 0f;
        public static float FullFallVelocity = 0f;
        public static float CapturedFallVel = 0f;

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
            EnableEverywhere = Config.Bind<bool>("Mod", "Hard mode: Enabled even when Wisplight equipped", false, "Stamina affects fall speed/damage even when Wisplight equipped");
            AltMode = Config.Bind<bool>("Stamina Mode Override", "Drains stamina while aloft when Wisplight unequipped", false, "Uses stamina to stay aloft without Wisplight");
            AltStamDrain = Config.Bind<int>("Stamina Mode Override", "Stamina drain unit (larger drains faster)", 10, "Stamina drain unit");
            HardcoreMode = Config.Bind<bool>("Stamina Mode Override", "Hardcore mode: Drains stamina & affects fall speed/damage (only works if Stamina Mode Override enabled)", false, "Uses stamina while aloft, regardless of Wisplight, & affects fall speed/damage");
        }

        [HarmonyPatch(typeof(SEMan), "ModifyFallDamage")]
        private class ModifyFallDamageSEManPatch
        {
            private static void Prefix(ref List<StatusEffect> ___m_statusEffects)
            {
                FeatherFallStatusIndex = ___m_statusEffects.FindIndex(se => se.m_name == FeatherFallName);
                AppliedDamageIndex = 0;
            }
        }

        [HarmonyPatch(typeof(SE_Stats), "ModifyFallDamage")]
        private class ModifyFallDamagePatch
        {
            private static void Prefix(ref float baseDamage, ref float damage, ref float ___m_fallDamageModifier)
            {
                //logger.LogInfo($"featherfall index {FeatherFallStatusIndex} applied index {AppliedDamageIndex}");

                // This bit of fun code is to track whether the caller was actually the Feather Fall Status effect
                if (FeatherFallStatusIndex != AppliedDamageIndex)
                {
                    //logger.LogInfo($"rejecting AppliedDamageIndex {AppliedDamageIndex}");
                    AppliedDamageIndex++;
                    return;
                }

                FeatherFallStatusIndex = -1;
                AppliedDamageIndex = 0;

                float vel = FullFallVelocity;

                //float ld = damage;
                //float lbd = baseDamage;
                // Don't need this, it's for performance tuning and logging
                //float finalValue = ld += lbd * ___m_fallDamageModifier;

                //logger.LogInfo($"final damage before {finalValue}");
                //logger.LogInfo($"feather fall enabled? {FeatherFallEnabled} instantaneous vel {FullFallVelocity}");
                if (!EnableToggle.Value || !FeatherFallEnabled)
                {
                    //logger.LogInfo("feather fall disabled (damage)");
                    ___m_fallDamageModifier = FeatherFallEnabled ? -1 : 0;
                    return;
                }

                //logger.LogInfo($"original damage modifier {___m_fallDamageModifier} original basedamage {baseDamage} damage {damage}");

                if (AltMode.Value && !HardcoreMode.Value)
                {
                    setDamageModifier(ref damage, ref ___m_fallDamageModifier, vel);

                    //ld = damage;
                    //lbd = baseDamage;

                    //float fv = ld += lbd * ___m_fallDamageModifier;

                    //logger.LogInfo($"alt mode final damage {fv}");
                    return;
                }

                ___m_fallDamageModifier = -1;
                if (!WispLightEnabled || EnableEverywhere.Value || HardcoreMode.Value)
                {
                    //logger.LogInfo($"setting stam-derived damage {damage} baseDamage {baseDamage} modifier {___m_fallDamageModifier}");
                    setStamDerivedDamage(ref damage, ref ___m_fallDamageModifier);
                }

                //ld = damage;
                //lbd = baseDamage;

                //finalValue = ld += lbd * ___m_fallDamageModifier;

                //logger.LogInfo($"final damage after {finalValue}");
            }

            private static void setDamageModifier(ref float damage, ref float damageModifier, float vel)
            {
                //logger.LogInfo($"initial vel {vel}");
                if (StaminaPercentage == 0)
                {
                    if (FullFallVelocity > 5.4f)
                    {
                        CapturedFallVel = Mathf.Clamp((vel * 2.9f), 0, 100);
                        //logger.LogInfo($"captured fall vel {CapturedFallVel}");
                        damage = CapturedFallVel;
                        damageModifier = 0;
                        CapturedFallVel = 0;
                        FullFallVelocity = 0;
                    }
                    else
                    {
                        setFinalDamageModifier(ref damage, ref damageModifier);
                    }
                }
                else
                {
                    setFinalDamageModifier(ref damage, ref damageModifier);
                }
            }

            private static void setStamDerivedDamage(ref float damage, ref float damageModifier)
            {
                float stamDiff = 1 - StaminaPercentage;
                damage = 0;
                damageModifier = stamDiff;
            }

            private static void setFinalDamageModifier(ref float damage, ref float damageModifier)
            {
                if (HardcoreMode.Value)
                {
                    setStamDerivedDamage(ref damage, ref damageModifier);
                }
                else
                {
                    damageModifier = -1;
                }
            }
        }

        [HarmonyPatch(typeof(Player), "SetControls")]
        private class SetControlsPatch
        {
            private static void Prefix(ref Player __instance, ref bool ___m_groundContact, ref float ___m_fallTimer)
            {
                Vector3 vel = __instance.GetVelocity();
                StatusEffect slowFallEffect = __instance.GetSEMan().GetStatusEffects().Find(effect => effect.m_name == FeatherFallName);
                StatusEffect whispLightEffect = __instance.GetSEMan().GetStatusEffects().Find(effect => effect.m_name == WispLightName);
                FeatherFallEnabled = slowFallEffect != null;
                WispLightEnabled = whispLightEffect != null;

                if (!EnableToggle.Value)
                {
                    return;
                }

                PlayerFalling = __instance.GetVelocity().y < 0 && !___m_groundContact;

                if (AltMode.Value && FeatherFallEnabled && PlayerFalling && (!WispLightEnabled || HardcoreMode.Value))
                {
                    //logger.LogInfo($"using stam as falling with unit {AltStamDrain.Value / 100f}");
                    __instance.UseStamina(AltStamDrain.Value / 100f);
                }

                StaminaPercentage = __instance.GetStaminaPercentage();

                // Calculate fall time at 0 percent stam
                if (AltMode.Value && PlayerFalling && FeatherFallEnabled && !__instance.IsFlying() && StaminaPercentage == 0)
                {
                    ElapsedFreeFallTime += Time.deltaTime;
                }
                else
                {
                    ElapsedFreeFallTime = 0f;
                }

                // Calculate fall velocity while in air
                if (AltMode.Value && FeatherFallEnabled && PlayerFalling && ElapsedFreeFallTime > 0 && ___m_fallTimer > 0)
                {
                    int delay = Mathf.Abs(vel.y) > 50f ? 200 : 100;
                    Task.Delay(delay).ContinueWith((task) => {
                        setVelocity(vel);
                    });
                }

                //if (vel.y < 0 && PlayerFalling)
                //{
                //    logger.LogInfo($"falling velocity {vel.y}");
                //}
            }

            private static void setVelocity(Vector3 vel)
            {
                FullFallVelocity = Mathf.Abs(vel.y);
                //logger.LogInfo($"setting delayed velocity at {FullFallVelocity}");
            }
        }

        [HarmonyPatch(typeof(SE_Stats), "ModifyWalkVelocity")]
        private class ModifyWalkVelocityPatch
        {
            private static void Prefix(ref Vector3 vel, ref float ___m_maxMaxFallSpeed)
            {
                if (!EnableToggle.Value || !FeatherFallEnabled)
                {
                    ___m_maxMaxFallSpeed = FeatherFallEnabled ? 5 : 0;
                    return;
                }

                if (WispLightEnabled)
                {
                    ___m_maxMaxFallSpeed = 5;
                }

                if (AltMode.Value && !HardcoreMode.Value)
                {
                    if (StaminaPercentage <= 0)
                    {
                        ___m_maxMaxFallSpeed = 0;
                    }
                    else
                    {
                        ___m_maxMaxFallSpeed = 5;
                    }
                    return;
                }

                if (!WispLightEnabled || EnableEverywhere.Value || (AltMode.Value && HardcoreMode.Value))
                {
                    if (StaminaPercentage != 0)
                    {
                        ___m_maxMaxFallSpeed = InitialMaxFallSpeed / StaminaPercentage;
                    }
                }

                if (AltMode.Value && StaminaPercentage <= 0)
                {
                    ___m_maxMaxFallSpeed = 0;
                }
            }
        }
    }
}
