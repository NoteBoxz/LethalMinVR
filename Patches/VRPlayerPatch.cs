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
    [HarmonyPatch(typeof(VRPlayer))]
    public static class VRPlayerPatch
    {
        [HarmonyPatch(nameof(VRPlayer.Awake))]
        [HarmonyPostfix]
        public static void AwakePostfix(VRPlayer __instance)
        {
            if (LethalMinVR.InVRMode)
            {
                Leader LocalLeader = PikminManager.instance.LocalLeader;
                LeaderVR lvr = __instance.gameObject.AddComponent<LeaderVR>();
                LocalLeader.holdPosition.transform.SetParent(__instance.Bones.LocalRightHand);
                LocalLeader.holdPosition.transform.localPosition = new Vector3(0f, 0f, 0f);
                LocalLeader.ThrowOrigin.transform.SetParent(__instance.Bones.LocalRightHand);
                LocalLeader.ThrowOrigin.transform.localPosition = new Vector3(0f, 0f, 0f);
                lvr.LeaderScript = LocalLeader;
                GameObject root = new GameObject("GlowMobHoldPosForwardOffset");
                root.transform.SetParent(__instance.Bones.LocalLeftHand);
                root.transform.localPosition = new Vector3(0f, 0f, 0f);
                root.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                lvr.GlowMobHoldPosition = new GameObject("GlowMobHoldPosition").transform;
                lvr.GlowMobHoldPosition.SetParent(root.transform);
                lvr.GlowMobHoldPosition.localPosition = new Vector3(0f, 0f, 0f);
                lvr.GlowMobHoldPosition.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
        }
    }
}