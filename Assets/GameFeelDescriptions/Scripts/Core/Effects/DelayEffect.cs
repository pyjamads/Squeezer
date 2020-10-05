using UnityEngine;

namespace GameFeelDescriptions
{
    public class DelayEffect : GameFeelEffect
    {
        public DelayEffect()
        {
            Description = "Delay any subsequent effect.";
            Delay = Random.Range(1, 100) / 100f;
            
            //10% chance of random
            RandomizeDelay = RandomExtensions.Boolean(0.1f);
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new DelayEffect();
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            //We're done
            return true;
        }
    }
}