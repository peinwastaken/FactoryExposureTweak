using BepInEx;
using BepInEx.Configuration;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

namespace FactoryExposureTweak
{
    [BepInPlugin("com.pein.factoryexposuretweak", "FactoryExposureTweak", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> Enabled { get; private set; }
        public static ConfigEntry<float> ExposureUpperLimit { get; private set; }

        public readonly string generalCategory = "1. General";

        private static Camera _camera;
        private static PrismEffects _prismEffects;

        private void Awake()
        {
            Enabled = Config.Bind(generalCategory, "Enabled", true, "Enable or disable the plugin.");
            ExposureUpperLimit = Config.Bind(generalCategory, "Exposure Upper Limit", 1.2f, "The upper exposure value on Factory. Pretty self-explanatory.");

            Enabled.SettingChanged += (sender, e) => UpdateExposureUpper(ExposureUpperLimit.Value);
            ExposureUpperLimit.SettingChanged += (sender, e) => UpdateExposureUpper(ExposureUpperLimit.Value);

            new OnGameStartedPatch().Enable();
        }

        public static void UpdateExposureUpper(float value)
        {
            if (_camera == null)
            {
                _camera = CameraClass.Instance.Camera;
            }

            if (_prismEffects == null)
            {
                _prismEffects = _camera.GetComponent<PrismEffects>();
            }

            if (Enabled.Value == true)
            {
                _prismEffects.exposureUpperLimit = ExposureUpperLimit.Value;
            }
            else
            {
                _prismEffects.exposureUpperLimit = 0.9f;
            }
            
        }
    }

    public class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            string locationId = __instance.LocationId;
            if (locationId == "factory4_day")
            {
                Plugin.UpdateExposureUpper(Plugin.ExposureUpperLimit.Value);
            }
        }
    }
}
