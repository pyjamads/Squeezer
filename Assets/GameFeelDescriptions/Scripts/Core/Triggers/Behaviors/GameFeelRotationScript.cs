using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelRotationScript : GameFeelBehaviorBase<OnRotateTrigger>
    {
        private Quaternion lastRotation;

        private void Start()
        {
            lastRotation = transform.rotation;
            
            SetupInitialTargets();
        }

        private void Update()
        {
            if (Disabled) return;
            
            if (transform.rotation == lastRotation) return;

            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }
            
            var rotation = transform.rotation * Quaternion.Inverse(lastRotation);
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode(rotation.eulerAngles);
            }
#endif
            
            for (var i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i], rotation.eulerAngles);
#endif
                
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], new RotationalData(rotation.eulerAngles){Origin = gameObject});
            }
            
            lastRotation = transform.rotation;
        }
    }
}