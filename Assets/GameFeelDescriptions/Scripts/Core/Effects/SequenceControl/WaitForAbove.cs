using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class WaitForAbove : GameFeelEffect
    {
        public WaitForAbove()
        {
            Description = "Delay any subsequent effect by an amount, or until all effects above it are complete. NB does not include nested effects";
        }

        public bool WaitForAllTargets;
        
        private List<GameFeelEffect> waitingFor = new List<GameFeelEffect>();

        public void WaitFor(GameFeelEffect effect)
        {
            waitingFor.Add(effect);
        }

        public void WaitFor(IEnumerable<GameFeelEffect> effects)
        {
            waitingFor.AddRange(effects);
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new WaitForAbove();
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            //TODO: might need a version that waits for every child effect as well. 2020-09-05
            
            //While any of the effects in this list is still running, returns false, unless the effect is looping infinitely.
            return waitingFor.All(item => item.isComplete || float.IsInfinity(item.GetRemainingTime()));
        }

        public override void ExecuteCleanUp()
        {
            //Clear our waiting for list, for Garbage Collection purposes.
            waitingFor.Clear();
            base.ExecuteCleanUp();
        }
    }
}