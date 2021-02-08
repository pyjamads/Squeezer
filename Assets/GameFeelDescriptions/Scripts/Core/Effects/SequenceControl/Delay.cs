using UnityEngine;

namespace GameFeelDescriptions
{
    public class Delay : GameFeelEffect
    {
        public Delay()
        {
            Description = "Delay any subsequent effect(s).";
            Delay = Random.Range(1, 100) / 100f;
            
            //10% chance of random
            RandomizeDelay = RandomExtensions.Boolean(0.1f);
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new Delay();
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