
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelStartScript : GameFeelBehaviorBase<OnStartTrigger>
    {
        [Header("Position offset, from transform.position")]
        public Vector3 localPositionOffset;
        
        [Header("Is position offset relative to transform.forward")]
        public bool useForwardForPositionOffset;
        
        [Header("Rotation offset, from transform.forward")]
        public Vector3 forwardRotationOffset;

        private void Start()
        {
            if (Disabled) return;
            
            SetupInitialTargets();
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode();
            }
#endif
            
            for (var i = 0; i < EffectGroups.Count; i++)
            {   
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i]);
#endif
                
                var positionWithOffset = transform.position + localPositionOffset;

                if (useForwardForPositionOffset)
                {
                    var qrt = Quaternion.LookRotation(transform.forward, transform.up);
                    positionWithOffset = transform.position + qrt * localPositionOffset;
                }

                var positionDelta = new PositionalData(positionWithOffset, Quaternion.Euler(forwardRotationOffset) * transform.forward) { Origin = gameObject };

#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
#endif
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], positionDelta);
            }
        }
    }
}