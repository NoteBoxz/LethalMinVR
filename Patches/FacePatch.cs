using HarmonyLib;
using LCVR;
using LCVR.Physics.Interactions;
using LCVR.Player;
using LCVR.UI;
using LethalMin;
using LethalMin.HUD;
using LethalMin.Utils;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(Face))]
    public static class FacePatch
    {
        [HarmonyPatch(nameof(Face.Create))]
        [HarmonyPostfix]
        public static void CreatePostfix(VRPlayer __instance)
        {
            if (LethalMinVR.InVRMode)
            {
                Face face = Object.FindObjectOfType<Face>();
                face.ALLOWED_ITEMS.Add("Whistle");
            }
        }
    }
}