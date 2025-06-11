using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LethalMin;
using LethalMin.HUD;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(PikminHUDManager))]
    public class PikminHUDManagerPatch
    {
        [HarmonyPatch(nameof(PikminHUDManager.Start))]
        [HarmonyPostfix]
        private static void StartPostFix(PikminHUDManager __instance)
        {
            __instance.element.targetAlpha = 1;
            if (LethalMinVR.InVRMode)
            {
                AutoSetHudVRPreset(__instance);
            }
        }

        private static void AutoSetHudVRPreset(PikminHUDManager __instance)
        {
            // Skip if auto-preset is disabled
            if (!LethalMin.LethalMin.AutoSetHudVRPreset.InternalValue)
                return;

            // Skip if the correct preset is already set
            bool armHUDDisabled = LCVR.Plugin.Config.DisableArmHUD.Value;
            bool isCorrectPresetAlreadySet =
                (armHUDDisabled && LethalMin.LethalMin.HUDPreset.InternalValue == PikminHUDManager.HUDLayoutPresets.VRFace) ||
                (!armHUDDisabled && LethalMin.LethalMin.HUDPreset.InternalValue == PikminHUDManager.HUDLayoutPresets.VRHands);

            if (isCorrectPresetAlreadySet)
                return;

            // Set the appropriate preset based on whether arm HUD is disabled
            var targetPreset = armHUDDisabled
                ? PikminHUDManager.HUDLayoutPresets.VRFace
                : PikminHUDManager.HUDLayoutPresets.VRHands;

            LethalMin.LethalMin.HUDPreset.Entry.BoxedValue = targetPreset;
            __instance.SetLayout(targetPreset);
        }
    }
}
