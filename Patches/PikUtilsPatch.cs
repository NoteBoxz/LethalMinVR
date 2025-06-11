using GameNetcodeStuff;
using HarmonyLib;
using LethalMin.Utils;
using UnityEngine;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(PikUtils))]
    public class PikUtilsPatch
    {
        [HarmonyPatch(nameof(PikUtils.RevivePlayer))]
        [HarmonyPrefix]
        public static void RevivePlayerPrefix(PlayerControllerB player, Vector3 RevivePos)
        {
            if (player.IsOwner && LethalMinVR.InVRMode)
            {
                CallOnPlayerRevive();
            }
        }

        public static void CallOnPlayerRevive()
        {
            LCVR.Patches.Spectating.SpectatorPlayerPatches.OnPlayerRevived();
        }
    }
}
