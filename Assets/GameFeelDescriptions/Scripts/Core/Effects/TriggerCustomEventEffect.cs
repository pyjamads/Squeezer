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
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new TriggerCustomEventEffect();
            cp.EventName = EventName;
            
            //This effect does nothing, when no event name is specified, make it trigger anything!
            if (string.IsNullOrEmpty(EventName))
            {
                Debug.LogWarning("No EventName specified on TriggerCustomEventEffect! Description: "+origin+" target: "+target);
                //return null;
                cp.EventName = "*";
            }
            
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }

        protected override bool ExecuteTick()
        {
            GameFeelEffectExecutor.Instance.TriggerCustomEvent(target == null ? origin : target, EventName, triggerData);

            //We're done
            return true;
        }
    }
}