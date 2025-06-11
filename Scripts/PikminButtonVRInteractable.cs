using LCVR.Physics;
using LCVR.Player;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LethalMin;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

namespace LethalMinVR
{
    public class PikminButtonVRInteractable : PikminVRInteractable
    {
        public Button buttonScript = null!;
        public override bool OnButtonPress(VRInteractor interactor)
        {
            buttonScript.onClick.Invoke();
            return true;
        }

        public override void OnButtonRelease(VRInteractor interactor)
        {
        }

        public override void OnColliderEnter(VRInteractor interactor)
        {
        }

        public override void OnColliderExit(VRInteractor interactor)
        {
        }
    }
}