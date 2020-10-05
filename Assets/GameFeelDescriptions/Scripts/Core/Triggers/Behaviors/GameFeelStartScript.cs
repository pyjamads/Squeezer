
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelStartScript : GameFeelBehaviorBase
    {
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
                
#if UNITY_EDITOR
                if (EditorApplication.isPlaying)
#endif
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], null);
            }
        }
    }
}