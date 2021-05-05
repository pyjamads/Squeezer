using UnityEngine;

namespace GameFeelDescriptions
{
    public class Selector : GameFeelEffect
    {
        public Selector()
        {
            Description = "Select and run only one of the subsequent effect(s).";
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new Selector();
            cp.Init(origin, target, triggerData);
            cp = DeepCopy(cp, ignoreCooldown);

            if (cp.ExecuteAfterCompletion.Count > 1)
            {
                var next = cp.ExecuteAfterCompletion.GetRandomElement();
                cp.ExecuteAfterCompletion.Clear();
                cp.ExecuteAfterCompletion.Add(next);    
            }
            
            return cp;
        }

        protected override bool ExecuteTick()
        {
            //We're done
            return true;
        }
    }
}