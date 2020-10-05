using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelCustomEventScript : GameFeelBehaviorBase
    {
        public string EventName; 
        
        public OnCustomEventTrigger.EventTriggerSources AllowFrom;
 
        [HideFieldIf("AllowFrom", OnCustomEventTrigger.EventTriggerSources.Sources, negate: true)]
        public string[] Sources;


        public List<bool> isTag;
        private void Start()
        {
            SetupInitialTargets();
            CheckSources();
            //TODO: figure out if there are side effects on enter/exit playmode. 7/4/2020
            GameFeelEffectExecutor.Instance.OnCustomEventTriggered += QueueExecution;
        }
        
        private void CheckSources()
        {
            isTag = new List<bool>();
            foreach (var str in Sources)
            {
                isTag.Add(Helpers.DoesTagExist(str));
            }
        }

        private void OnDestroy()
        {
            if (GameFeelEffectExecutor.Instance)
            {
                // ReSharper disable once DelegateSubtraction (ignored here because we're doing a very simple subtraction)
                GameFeelEffectExecutor.Instance.OnCustomEventTriggered -= QueueExecution;   
            }
        }

        private void QueueExecution(GameObject origin, string eventName, GameFeelTriggerData triggerData)
        {
            if (Disabled) return;
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }

            if (AllowFrom == OnCustomEventTrigger.EventTriggerSources.Sources && Sources.Length != isTag.Count)
            {
                CheckSources();
            }
      
            switch (AllowFrom)
            {
                case OnCustomEventTrigger.EventTriggerSources.Anywhere:
                    //Just allow it through.
                    break;
                case OnCustomEventTrigger.EventTriggerSources.Self:
                    if (origin != gameObject) return; 
                    break;
                case OnCustomEventTrigger.EventTriggerSources.Sources:
                    if (Sources.Any(origin.CompareTag) == false && Sources.Any(origin.name.Equals) == false) return;
                    break;
            }
            
            //Check to see if we are supposed to React to this event.
            if (!EventName.Contains("*") &&
                string.Compare(eventName, EventName, StringComparison.OrdinalIgnoreCase) != 0) return;
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode(eventName, origin, triggerData);
            }
#endif
            
            //When a custom event triggers, Other is the origin of the event.
            for (var i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i], eventName, origin, triggerData);
#endif
                
                //NOTE: we pass here the origin of the event, instead of this detector as the origin.
                EffectGroups[i].InitializeAndQueueEffects(origin, targets[i], triggerData);
            }
        }
    }
}