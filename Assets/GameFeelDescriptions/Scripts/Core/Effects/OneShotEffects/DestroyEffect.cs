using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class DestroyEffect : GameFeelEffect
    {
        public DestroyEffect()
        {
            Description = "Simple destruction effect.";
        }
      
        //TODO: Remove this effect, replace all logic with copies of the thing that the effect happened to. 11/02/2020

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new DestroyEffect();
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            if (target == null) return true;

            if (triggerData.InCollisionUpdate)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }

            //We're done
            return true;
        }
    }
}