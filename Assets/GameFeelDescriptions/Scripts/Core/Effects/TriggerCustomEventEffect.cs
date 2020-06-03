using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class TriggerCustomEventEffect : GameFeelEffect
    {
        public string EventName;
        
        [Min(0)]
        public float Cooldown;
        
        private static Dictionary<int, float> lastEvent = new Dictionary<int, float>();
        
        public TriggerCustomEventEffect()
        {
            Description = "Simple event triggering effect.";
        }
      
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var hash = EventName.GetHashCode();
            var now = unscaledTime ? Time.unscaledTime : Time.time;
            
            if (lastEvent.ContainsKey(hash) && lastEvent[hash] + Cooldown > now ) return null;

            lastEvent[hash] = now;
            
            var cp = new TriggerCustomEventEffect();
            cp.EventName = EventName;
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            GameFeelEffectExecutor.Instance.TriggerCustomEvent(target == null ? origin : target, EventName, interactionDirection);

            return false;
        }
    }
}