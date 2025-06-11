using GameNetcodeStuff;
using HarmonyLib;
using LCVR.Physics.Interactions;
using LethalMin;
using LethalMin.Utils;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(WhistleItem))]
    public class WhistleItemPatch
    {
        [HarmonyPatch(nameof(WhistleItem.OnDismiss))]
        [HarmonyPrefix]
        public static bool OnDismissPrefix(WhistleItem __instance)
        {
            if (!LethalMinVR.InVRMode || !LethalMin.LethalMin.UseMouthTriggerForWhistle)
            {
                return true; // Allow the original method to execute if not using mouth trigger
            }
            Face face = Object.FindObjectOfType<Face>();
            return face.heldItem == __instance;
        }
    }
}
