using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class RotateEffect : TweenEffect<Vector3>
    {
        public RotateEffect()
        {
            Description = "Rotate Effect allows you to rotate an object using easing.";
            relative = false;
        }
        
        [Tooltip("Use global rotation instead of local rotation.")]
        public bool useGlobalRotation;

        protected override void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;
            
            if (useGlobalRotation)
            {
                target.transform.eulerAngles = value;  
            }
            else
            {
                target.transform.localEulerAngles = value;
            }
        }

        protected override Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            if (useGlobalRotation)
            {
                return target.transform.eulerAngles;    
            } 
            
            return target.transform.localEulerAngles;
        }

        protected override Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return fromValue + addValue;
        }

        protected override Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
        {
            return toValue - fromValue;
        }
        
        protected override bool TickTween()
        {
            if (target == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            var easeFunc = GetEaseFunc();
            if (relative)
            {
                var progress = elapsed / Duration;
                var prevProgress = oldElapsed / Duration;

                if (reverse)
                {
                    progress = 1 - progress;
                    prevProgress = 1 - prevProgress;
                }
                
                var prev = diffAmount * easeFunc.Invoke(prevProgress);
                var current = diffAmount * easeFunc.Invoke(progress);
                
                //amount = end - start;
                //current + (amount * easing(t1)) - (amount * - easing(t0));
                SetValue(target, GetValue(target) + (reverse ? -1 : 1) * (current - prev));
            }
            else
            {
                //@from  + (to - @from) * easing(t);
                SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, easeFunc));    
            }
            //SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));

            return false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new RotateEffect{useGlobalRotation = useGlobalRotation};
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean(amount))
            {
                setFromValue = !setFromValue;
            }
            
            if(RandomExtensions.Boolean(amount))
            {
                useGlobalRotation = RandomExtensions.Boolean(0.25f);
            }

            if (RandomExtensions.Boolean())
            {
                //Make a random color, and add/subtract a proportional amount here.
                var rndAmount = Random.value * amount * 2 - amount;
                @from += Random.onUnitSphere * rndAmount;    
            }


            if (RandomExtensions.Boolean())
            {
                //Make a random color, and add/subtract a proportional amount here.
                var rndAmount = Random.value * amount * 2 - amount;
                to += Random.onUnitSphere * rndAmount;    
            }

            if (RandomExtensions.Boolean(amount))
            {
                easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});
            }

            if (RandomExtensions.Boolean(amount))
            {
                loopType = EnumExtensions.GetRandomValue<LoopType>();
            }

            if (RandomExtensions.Boolean(amount))
            {
                repeat = Random.Range(-1, 3);
            }

            base.Mutate(amount);
        }
    }
}