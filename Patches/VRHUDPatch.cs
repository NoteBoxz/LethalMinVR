using HarmonyLib;
using LCVR;
using LCVR.Managers;
using LCVR.UI;
using LethalMin;
using LethalMin.HUD;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(VRHUD))]
    public static class VRHUDPatch
    {
        public static Canvas? OnionCanvas;
        public static GameObject? OnionHUD;
        [HarmonyPatch(nameof(VRHUD.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(VRHUD __instance)
        {
            var xOffset = Plugin.Config.HUDOffsetX.Value;
            var yOffset = Plugin.Config.HUDOffsetY.Value;

            GameObject PikminHUD = Object.FindObjectOfType<PikminHUDManager>().gameObject;
            if (Plugin.Config.DisableArmHUD.Value)
            {
                PikminHUD.transform.SetParent(__instance.FaceCanvas.transform, false);
                PikminHUD.transform.localPosition = new Vector3(91 + xOffset, -185 + yOffset, 0);
                PikminHUD.transform.localRotation = Quaternion.identity;
            }
            else
            {
                PikminHUD.transform.SetParent(__instance.RightHandCanvas.transform, false);
                PikminHUD.transform.localPosition = new Vector3(-28, 120, 40);
                PikminHUD.transform.localRotation = Quaternion.Euler(0, 195, 0);
                PikminHUD.transform.localScale = Vector3.one * 0.8f;
            }

            OnionCanvas = new GameObject("Onion VR Canvas")
            {
                transform =
                {
                    parent = __instance.transform,
                    localScale = Vector3.one * 0.0007f
                }
            }.AddComponent<Canvas>();
            OnionCanvas.worldCamera = VRSession.Instance.MainCamera;
            OnionCanvas.renderMode = RenderMode.WorldSpace;
            OnionCanvas.sortingOrder = 1;
            OnionCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            OnionHUD = Object.FindObjectOfType<OnionHUDManager>().gameObject;
            OnionHUD.transform.SetParent(OnionCanvas.transform, false);
            OnionHUD.transform.localPosition = new Vector3(0 + xOffset, 0 + yOffset, LethalMin.LethalMin.OnionHUDZDistance.InternalValue);
            OnionHUD.transform.localRotation = Quaternion.identity;
            GameObject darkenOBJ = OnionHUD.transform.Find("AcutalUI/Darken").gameObject;
            if (darkenOBJ != null)
            {
                darkenOBJ.SetActive(false);
            }
            else
            {
                LethalMinVR.Logger.LogError("Darken object not found in OnionHUDManager");
            }
            VRHUD.MoveToFront(OnionCanvas);
        }

        [HarmonyPatch(nameof(VRHUD.LateUpdate))]
        [HarmonyPostfix]
        public static void LateUpdatePostfix(VRHUD __instance)
        {
            if (OnionCanvas == null || OnionHUD == null)
            {
                return;
            }

            var camTransform = VRSession.Instance.MainCamera.transform;
            var xOffset = Plugin.Config.HUDOffsetX.Value;
            var yOffset = Plugin.Config.HUDOffsetY.Value;
        
            OnionCanvas.transform.localPosition =
                Vector3.Lerp(OnionCanvas.transform.localPosition, camTransform.forward * 0.5f, 0.4f);
            OnionCanvas.transform.rotation = Quaternion.Slerp(OnionCanvas.transform.rotation, camTransform.rotation, 0.4f);
            OnionHUD.transform.localPosition = new Vector3(0 + xOffset, 0 + yOffset, LethalMin.LethalMin.OnionHUDZDistance.InternalValue);
        }
    }
}