using LCVR.Physics;
using LCVR.Player;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LethalMin;
using UnityEngine.Animations.Rigging;

namespace LethalMinVR
{
    public class PikminVRInteractableProvider : MonoBehaviour, VRInteractable
    {
        public InteractableFlags Flags => InteractableFlags.BothHands;
        public PikminVRInteractable mainScript = null!;

        public bool OnButtonPress(VRInteractor interactor)
        {
            return mainScript.OnButtonPress(interactor);
        }

        public void OnButtonRelease(VRInteractor interactor)
        {
            mainScript.OnButtonRelease(interactor);
        }

        public void OnColliderEnter(VRInteractor interactor)
        {
            mainScript.OnColliderEnter(interactor);
        }

        public void OnColliderExit(VRInteractor interactor)
        {
            mainScript.OnColliderExit(interactor);
        }
    }
}