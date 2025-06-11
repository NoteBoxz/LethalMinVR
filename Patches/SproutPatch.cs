using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using LethalMin;
using LethalMin.Pikmin;
using LethalMin.Utils;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace LethalMinVR.Patches
{
    [HarmonyPatch(typeof(Sprout))]
    public class SproutPatch
    {
        [HarmonyPatch(nameof(Sprout.Start))]
        [HarmonyPostfix]
        private static void StartPostFix(Sprout __instance)
        {
            if (LethalMinVR.InVRMode && !LethalMin.LethalMin.DisableSproutInteraction.InternalValue)
            {
                ReplaceInteractable(__instance);
            }
        }

        public static void ReplaceInteractable(Sprout sprout)
        {
            // Get required components
            InteractTrigger trigger = sprout.GetComponentInChildren<InteractTrigger>();
            if (trigger == null) return;

            SproutModelRefences refs = sprout.GetComponentInChildren<SproutModelRefences>();
            if (refs == null) return;

            GameObject triggerGameObject = trigger.gameObject;
            Object.Destroy(trigger);

            // Collect VR interactable objects
            HashSet<GameObject> vrInteractables = new HashSet<GameObject>();
            if (refs.SproutVRInteractableObject != null)
            {
                bool isDuplicate = false;
                foreach (SproutModelGeneration gen in refs.Generations)
                {
                    if (refs.SproutVRInteractableObject == gen.SproutVRInteractableObject)
                    {
                        isDuplicate = true;
                        break;
                    }
                }

                if (!isDuplicate)
                {
                    vrInteractables.Add(refs.SproutVRInteractableObject);
                }
            }

            // Add generation interactables
            foreach (SproutModelGeneration gen in refs.Generations)
            {
                if (gen.SproutVRInteractableObject != null)
                {
                    vrInteractables.Add(gen.SproutVRInteractableObject);
                }
            }

            // Create main VR interactable component
            SproutVRInteractable vrInteractable = triggerGameObject.AddComponent<SproutVRInteractable>();
            vrInteractable.sproutScript = sprout;

            // Set up main interactable physics properties
            if (vrInteractables.Count > 0)
            {
                Object.Destroy(vrInteractable.GetComponent<Collider>());
                Object.Destroy(vrInteractable.GetComponent<Rigidbody>());
            }
            else
            {
                triggerGameObject.layer = LayerMask.NameToLayer("Colliders");
                triggerGameObject.tag = "InteractTrigger";
            }

            // Create interactive elements for each VR interactable object
            foreach (GameObject go in vrInteractables)
            {
                CreateInteractableCapsule(go, vrInteractable);
            }

            // Create the IK rig system
            SetupIKSystem(refs, vrInteractable);
        }

        private static void CreateInteractableCapsule(GameObject parent, SproutVRInteractable mainInteractable)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsule.transform.SetParent(parent.transform);

            // Configure collider
            Collider collider = capsule.GetComponent<Collider>();
            collider.isTrigger = true;

            // Configure rigidbody
            Rigidbody rb = capsule.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            // Set layer and tag
            capsule.layer = LayerMask.NameToLayer("Colliders");
            capsule.tag = "Untagged";

            // Add provider component
            PikminVRInteractableProvider provider = capsule.AddComponent<PikminVRInteractableProvider>();
            provider.mainScript = mainInteractable;

            // Position and scale
            capsule.transform.localPosition = Vector3.zero;
            capsule.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            capsule.transform.localRotation = Quaternion.Euler(0, 0, 90f);
        }

        private static void SetupIKSystem(SproutModelRefences refs, SproutVRInteractable vrInteractable)
        {
            Dictionary<GameObject, List<Transform>> sproutBones = new Dictionary<GameObject, List<Transform>>();

            // Check if the main animator is shared with any generations
            bool animatorIsShared = refs.Generations.Any(gen => refs.Animator == gen.Animator);

            // Add main references if its animator isn't shared
            if (!animatorIsShared && refs.SproutBones != null && refs.SproutBones.Count > 0)
            {
                sproutBones[refs.gameObject] = SortBonesFromTipToRoot(refs.SproutBones);
            }

            // Add generation bones
            foreach (SproutModelGeneration gen in refs.Generations)
            {
                if (gen.SproutBones != null && gen.SproutBones.Count > 0)
                {
                    sproutBones[gen.gameObject] = SortBonesFromTipToRoot(gen.SproutBones);
                }
            }

            // Create IK constraints for each bone chain
            foreach (var kvp in sproutBones)
            {
                List<Transform> bones = kvp.Value;
                GameObject gameObject = kvp.Key;

                if (bones.Count == 0)
                {
                    LethalMinVR.Logger.LogWarning("No bones found in sprout hierarchy for " + gameObject.name);
                    continue;
                }

                // Add rig components
                RigBuilder builder = gameObject.AddComponent<RigBuilder>();
                Rig rig = gameObject.AddComponent<Rig>();
                builder.layers.Add(new RigLayer(rig));

                // Configure IK constraint
                ChainIKConstraint chainIKConstraint = gameObject.AddComponent<ChainIKConstraint>();
                chainIKConstraint.data.root = bones[bones.Count - 1];
                chainIKConstraint.data.tip = bones[0];
                chainIKConstraint.data.target = null;
                chainIKConstraint.data.chainRotationWeight = 1f;
                chainIKConstraint.data.tipRotationWeight = 0.5f;
                chainIKConstraint.data.maxIterations = 15;
                chainIKConstraint.data.tolerance = 0.0001f;

                // Store and build
                vrInteractable.chainIKConstraints.Add(builder, chainIKConstraint);
                builder.Build();
            }
        }

        private static List<Transform> SortBonesFromTipToRoot(List<Transform> bones)
        {
            if (bones == null || bones.Count == 0)
                return new List<Transform>();

            // Find the tip bone (bone with no children or with children that aren't in our bone list)
            Transform tipBone = bones.FirstOrDefault(b =>
                !bones.Any(otherBone => otherBone != b && otherBone.parent == b));

            if (tipBone == null)
            {
                LethalMinVR.Logger.LogWarning("Could not identify tip bone in sprout hierarchy");
                return bones; // Return unsorted as fallback
            }

            // Build sorted list from tip to root
            List<Transform> sorted = new List<Transform>();
            Transform current = tipBone;

            while (current != null && bones.Contains(current))
            {
                sorted.Add(current);
                current = current.parent;
            }

            return sorted;
        }
    }
}
