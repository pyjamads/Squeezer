using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelDisableScript : GameFeelBehaviorBase
    {
        [HideInInspector]
        public bool shouldReact = true;
        
        private void Start()
        {
            SetupInitialTargets(false);
            
            foreach (var item in EffectGroups)
            {
                if (item.AppliesTo == GameFeelTarget.Self && !item.ExecuteOnTargetCopy)
                {
                    Debug.LogWarning("Effect group: "+item.GroupName +" applies to Self which is being disabled, did you want to execute it on a copy?");
                }
            }
        }

        private void OnEnable()
        {
            //If someone re-enables the object, we should react again next time a disable happens.
            shouldReact = true;
        }
        
        private void OnDisable()
        {
            //This is to make sure we don't react to the destruction of this script.
            if (!shouldReact)
            {
                return;
            }
            shouldReact = false;
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets(false);
            }
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode();
            }
#endif
            
            for (int i = 0; i < EffectGroups.Count; i++)
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