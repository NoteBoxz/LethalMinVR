using LCVR.Physics;
using LCVR.Player;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LethalMin;
using UnityEngine.Animations.Rigging;

namespace LethalMinVR
{
    public class SproutVRInteractable : PikminVRInteractable
    {
        public Sprout sproutScript = null!;
        private VRInteractor? currentInteractor = null;

        [Header("Plucking Settings")]
        [Tooltip("Distance needed to pluck the sprout")]
        public float pluckThreshold = 1.5f;
        [Tooltip("Speed at which the sprout returns to its original position")]
        public float returnSpeed = 5f;
        [Tooltip("How strongly bones follow the pull direction")]
        public float boneFollowStrength = 0.8f;

        private Vector3 initialPosition;
        private bool isPlucked = false;
        private bool isReturning = false;
        private float TargetWeight = 0f;
        public Dictionary<RigBuilder, ChainIKConstraint> chainIKConstraints = new Dictionary<RigBuilder, ChainIKConstraint>();

        private void Start()
        {
            initialPosition = sproutScript.transform.position;
        }

        public override bool OnButtonPress(VRInteractor interactor)
        {
            if (isPlucked)
                return false;

            LethalMinVR.Logger.LogInfo($"SproutVRInteractable: {interactor.name} pressed the button on {gameObject.name}");
            currentInteractor = interactor;
            interactor.FingerCurler.ForceFist(true);
            isReturning = false;
            sproutScript.PlayPullSoundServerRpc();
            sproutScript.sproutAudio.PlayOneShot(sproutScript.PullSFX);
            Leader leaderScript = interactor.GetComponentInParent<Leader>();
            if (leaderScript == null)
            {
                LethalMinVR.Logger.LogError($"SproutVRInteractable: {interactor.name} does not have a Leader component in parent!");
                return false;
            }
            sproutScript.PlayerPluckingID = leaderScript.Controller.OwnerClientId;

            return true;
        }

        public override void OnButtonRelease(VRInteractor interactor)
        {
            if (isPlucked)
                return;

            LethalMinVR.Logger.LogInfo($"SproutVRInteractable: {interactor.name} released the button on {gameObject.name}");
            currentInteractor = null;
            interactor.FingerCurler.ForceFist(false);
            isReturning = true;
        }

        public void OnDestroy()
        {
            // Clean up any references to the interactor
            if (currentInteractor != null)
            {
                LethalMinVR.Logger.LogWarning($"SproutVRInteractable: {currentInteractor.name} was destroyed without releasing the button on {gameObject.name}");
                currentInteractor.FingerCurler.ForceFist(false);
                currentInteractor = null;
            }
        }

        public void Update()
        {
            // Handle plucking logic
            if (currentInteractor != null && !isPlucked)
            {
                // Move the sprout with the interactor
                Vector3 pullDirection = currentInteractor.transform.position - initialPosition;
                float pullDistance = pullDirection.magnitude;

                // Check if we've pulled far enough to pluck
                if (pullDistance >= pluckThreshold)
                {
                    isPlucked = true;
                    sproutScript.sproutAudio.PlayOneShot(sproutScript.PluckSFX);
                    sproutScript.PluckAndDespawnServerRpc(sproutScript.PlayerPluckingID);
                }
                else
                {
                    // Move sprout slightly in pull direction (limited by distance)
                    float moveRatio = Mathf.Clamp01(pullDistance / 5f);
                    sproutScript.transform.position = initialPosition + pullDirection.normalized * (pullDistance * moveRatio);
                    TargetWeight = 1;
                }
            }
            // Return to original position when released
            else if (isReturning && !isPlucked)
            {
                sproutScript.transform.position = Vector3.Lerp(
                    sproutScript.transform.position,
                    initialPosition,
                    returnSpeed * Time.deltaTime
                );

                // Check if we've approximately reached the initial position
                if (Vector3.Distance(sproutScript.transform.position, initialPosition) < 0.01f)
                {
                    sproutScript.transform.position = initialPosition;
                    isReturning = false;
                    foreach (var kpv in chainIKConstraints)
                    {
                        ChainIKConstraint chainIKConstraint = kpv.Value;
                        chainIKConstraint.data.target = null;
                        chainIKConstraint.weight = 0;
                    }
                }
                else
                {
                    TargetWeight = 0;
                }
            }


            if (currentInteractor != null || isReturning)
            {
                foreach (var kpv in chainIKConstraints)
                {
                    RigBuilder rigBuilder = kpv.Key;
                    ChainIKConstraint chainIKConstraint = kpv.Value;

                    // Store the previous target
                    Transform previousTarget = chainIKConstraint.data.target;

                    // Set the new target
                    Transform? newTarget = currentInteractor?.transform;
                    chainIKConstraint.data.target = newTarget;

                    // Only call Build() if the target has changed
                    if (previousTarget != newTarget)
                    {
                        rigBuilder.Build();
                    }

                    // Update the weight (no need to rebuild for weight changes)
                    chainIKConstraint.weight = Mathf.Lerp(chainIKConstraint.weight, TargetWeight, Time.deltaTime * boneFollowStrength);
                }
            }
        }

        public override void OnColliderEnter(VRInteractor interactor)
        {
        }

        public override void OnColliderExit(VRInteractor interactor)
        {
        }
    }
}