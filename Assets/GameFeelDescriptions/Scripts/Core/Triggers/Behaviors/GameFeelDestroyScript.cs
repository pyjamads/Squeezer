using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelDestroyScript : GameFeelBehaviorBase
    {
        [HideInInspector]
        public bool shouldReact = true;
        
        private void Start()
        {
            SetupInitialTargets();
        }

        private void OnDestroy()
        {
            if (Disabled) return;
            
            //This is to make sure we don't react to the destruction of this script.
            if (!shouldReact)
            {
                return;
            }
            shouldReact = false;
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }
            
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
                
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i]);
            }
        }
    }
}