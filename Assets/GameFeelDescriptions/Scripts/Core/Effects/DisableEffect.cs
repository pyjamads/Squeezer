using System;
using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class DisableEffect : GameFeelEffect
    {
        public DisableEffect()
        {
            Description = "Disable game object, unless onlyRenderer and/or onlyCollider is set, in which case it'll disable collisions and/or rendering.";
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            var cp = new DisableEffect();
            cp.Init(origin, target, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {       
            if (target == null) return true;
            
             target.SetActive(false);

             //We're done
             return true;
        }
    }
}