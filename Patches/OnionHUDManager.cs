using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LCVR.Player;
using LethalMin;
using LethalMin.HUD;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(OnionHUDManager))]
    public class OnionHUDManagerPatch
    {
        [HarmonyPatch(nameof(OnionHUDManager.OpenMenu))]
        [HarmonyPrefix]
        private static void OpenMenuPostfix(OnionHUDManager __instance)
        {
            if (LethalMinVR.InVRMode && !__instance.IsMenuOpen)
            {
                VRSession.Instance.LocalPlayer.leftHandInteractor.enabled = true;
                VRSession.Instance.LocalPlayer.rightHandInteractor.enabled = true;
                VRSession.Instance.LocalPlayer.EnableInteractorVisuals();
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
                VRSession.Instance.LocalPlayer.leftHandInteractor.enabled = !VRSession.Instance.LocalPlayer.PlayerController.isPlayerDead;
                VRSession.Instance.LocalPlayer.rightHandInteractor.enabled = !VRSession.Instance.LocalPlayer.PlayerController.isPlayerDead;
                VRSession.Instance.LocalPlayer.EnableInteractorVisuals(false);
            }
        }
    }
}
