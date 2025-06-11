using HarmonyLib;
using LCVR;
using LCVR.Player;
using LCVR.UI;
using LethalMin;
using LethalMin.HUD;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(Glowmob))]
    public static class GlowmobrPatch
    {
        [HarmonyPatch(nameof(Glowmob.SetLeader))]
        [HarmonyPostfix]
        public static void AwakePostfix(Glowmob __instance, Leader leader)
        {
            if (LethalMinVR.InVRMode)
            {
                __instance.holdPosition = leader.GetComponent<LeaderVR>().GlowMobHoldPosition;
            }
        }
    }
}