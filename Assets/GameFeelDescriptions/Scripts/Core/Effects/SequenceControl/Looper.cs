using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class Looper : DurationalGameFeelEffect
    {
        public Looper()
        {
            Description = "Loop any subsequent effect(s).";
            loopType = EnumExtensions.GetRandomValue(new List<LoopType>{LoopType.None});
            repeat = Random.Range(1, 4);
            DelayBetweenLoops = Random.Range(0f, 1f);
            ExecuteEffectsOnLooping = true;
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new Looper();
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }
        
        protected override bool ExecuteTick()
        {
            //DO NOTHING!
            return false;
        }
    }
}