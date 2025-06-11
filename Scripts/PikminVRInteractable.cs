using LCVR.Physics;
using LCVR.Player;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LethalMin;
using UnityEngine.Animations.Rigging;

namespace LethalMinVR
{
    public abstract class PikminVRInteractable : MonoBehaviour
    {
        public abstract bool OnButtonPress(VRInteractor interactor);
        public abstract void OnButtonRelease(VRInteractor interactor);
        public abstract void OnColliderEnter(VRInteractor interactor);
        public abstract void OnColliderExit(VRInteractor interactor);
    }
}