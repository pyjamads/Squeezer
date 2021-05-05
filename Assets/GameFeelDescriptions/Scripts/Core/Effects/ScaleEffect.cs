using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class ScaleEffect : TweenEffect<Vector3>
    {   
        public ScaleEffect()
        {
            Description = "Scale Effect allows you to scale an object using easing.";
            relative = false;
            to = Vector3.one * Random.Range(0, 101) / 20f;
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean(amount))
            {
                setFromValue = !setFromValue;
            }

            if (RandomExtensions.Boolean())
            {
                //Make a random scale, and add/subtract a proportional amount here.
                @from += Vector3.one * RandomExtensions.MutationAmount(amount);
                if (@from.x < 0)
                {
                    @from = Vector3.zero;
                }    
            }

            if (RandomExtensions.Boolean())
            {
                //Make a random scale, and add/subtract a proportional amount here.
                to += Vector3.one * RandomExtensions.MutationAmount(amount); 
                if (to.x < 0)
                {
                    to = Vector3.zero;
                }    
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

        protected override void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;

            //Clamp the scale value min to 0.
            if (value.x < 0) value.x = 0;
            if (value.y < 0) value.y = 0;
            if (value.z < 0) value.z = 0;

            target.transform.localScale = value;
        }

        protected override Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            return target.transform.localScale;
        }

        protected override Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return new Vector3(fromValue.x * addValue.x,fromValue.y * addValue.y,fromValue.z * addValue.z);
        }

        protected override Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
        {
            return TweenHelper.GetDifference(fromValue, toValue);
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
            var cp = new ScaleEffect();
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }
    }
    
    //TODO: make scale with velocity effect! 2020-11-09
//    public class ScaleWithVelocityEffect : GameFeelEffect
//    {   
//        public ScaleWithVelocityEffect()
//        {
//            Description = "Scale Effect allows you to scale an object using easing.";
//        }
//        
//        [Tooltip("The relative amount to rotate for every update.")]
//        [Range(0f, 1f)]
//        public float relativeAmount;
//        
//        private Quaternion start;
//        private Quaternion end;
//
//        protected void SetValue(GameObject target, Vector3 value)
//        {
//            if (target == null) return;
//            
//            target.transform.localScale = value;
//        }
//
//        protected Vector3 GetValue(GameObject target)
//        {
//            if (target == null) return Vector3.zero;
//            
//            return target.transform.localScale;
//        }
//
//        protected Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
//        {
//            return new Vector3(fromValue.x * addValue.x,fromValue.y * addValue.y,fromValue.z * addValue.z);
//        }
//
//        protected Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
//        {
//            return toValue - fromValue;
//        }
//        
//        protected override void ExecuteSetup()
//        {
//            start = GetValue(target);
//            end = GetValue(target);
//            
//            if (interactionDirection != null)
//            {
//               
//               end = target.transform.localScale * interactionDirection.Value;    
//            }
//            
//            base.ExecuteSetup();
//        }
//
//        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
//            Vector3? interactionDirection = null)
//        {
//            var cp = new ScaleEffect();
//            cp.Init(origin, target, unscaledTime, interactionDirection);
//            return DeepCopy(cp);
//        }
//
//        protected override bool ExecuteTick()
//        {
//            if (target == null) return true;
//            
//            SetValue(target, GameFeelTween.Interpolate(start, elapsed / duration, end, GetEaseFunc()));
//
//            return false;
//        }
//    }
}