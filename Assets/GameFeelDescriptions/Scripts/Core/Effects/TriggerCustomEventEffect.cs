using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class TriggerCustomEventEffect : GameFeelEffect
    {
        public string EventName;
        
        public TriggerCustomEventEffect()
        {
            Description = "Simple event triggering effect.";
        }
      
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            //This effect does nothing, when no event name is specified!
            if (string.IsNullOrEmpty(EventName))
            {
                Debug.LogWarning("No EventName specified on TriggerCustomEventEffect! Description: "+origin+" target: "+target);
                return null;
            }

            var cp = new TriggerCustomEventEffect();
            cp.EventName = EventName;
            cp.Init(origin, target, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            GameFeelEffectExecutor.Instance.TriggerCustomEvent(target == null ? origin : target, EventName, interactionDirection);

            //We're done
            return true;
        }
    }
}