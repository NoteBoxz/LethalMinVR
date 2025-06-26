using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LCVR.Managers;
using LCVR.Player;
using LethalMin;
using LethalMin.HUD;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(OnionHUDManager))]
    public class OnionHUDManagerPatch
    {
        static VRPlayer LocalPlayer => VRSession.Instance.LocalPlayer;
        [HarmonyPatch(nameof(OnionHUDManager.OpenMenu))]
        [HarmonyPrefix]
        private static void OpenMenuPostfix(OnionHUDManager __instance)
        {
            if (LethalMinVR.InVRMode && !__instance.IsMenuOpen)
            {
                LocalPlayer.EnableUIInteractors();
                LocalPlayer.PrimaryController.ShowDebugInteractorVisual(false);
            }
        }

        [HarmonyPatch(nameof(OnionHUDManager.CloseMenu))]
        [HarmonyPrefix]
        private static void CloseMenuPostfix(OnionHUDManager __instance)
        {
            if (!__instance.IsMenuOpen)
            {
                return;
            }

            if (LethalMinVR.InVRMode)
            {
                LocalPlayer.EnableUIInteractors(false);
                LocalPlayer.PrimaryController.ShowDebugInteractorVisual();
            }
        }
    }
}
