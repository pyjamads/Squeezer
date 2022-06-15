using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    [UsedImplicitly]
    public class TimeScaleEffect : TweenEffect<float>
    {
        private static TimeScaleEffect singletonCopy;
        
        public TimeScaleEffect()
        {
            Description = "TimeScale Effect allows you to change timeScale using easing.";
            relative = false;

            to = Random.Range(0.01f, 2f);
            loopType = LoopType.Yoyo;
            repeat = 1;

            UnscaledTime = true;
            
            Duration = Random.Range(1, 30) / 100f;
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean())
            {
                //Make sure to stays above 0.
                to = Mathf.Max(0.01f, to + RandomExtensions.MutationAmount(amount, to));    
            }

            base.Mutate(amount);
            
            setFromValue = false;   
            loopType = LoopType.Yoyo;
            repeat = 1;
            UnscaledTime = true;
            SetElapsed();
        }

        protected override void SetValue(GameObject target, float value)
        {
            Time.timeScale = Mathf.Max(0, value);
        }

        protected override float GetValue(GameObject target)
        {
            return Time.timeScale;
        }

        protected override float GetRelativeValue(float fromValue, float addValue)
        {
            return fromValue + addValue;
        }

        protected override float GetDifference(float fromValue, float toValue)
        {
            return toValue - fromValue;
        }

        protected override bool TickTween()
        {
            //TODO: Add relative value change, which would remove the singleton need!!!
            /*
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
                 //current + (amount * easing(t1)) - (amount * easing(t0));
                 SetValue(target, GetValue(target) + (reverse ? -1 : 1) * (current - prev));
             }
             else
             {
                 //@from  + (to - @from) * easing(t);
                 SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, easeFunc));    
             }
             */
           
            SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));
            
            //We never need to breakout early here.
            return false;
        }

        public override void ExecuteCleanUp()
        {
            //This should always be the case!
            if (singletonCopy == this)
            {
                singletonCopy = null;
            }
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            //Time.timeScale is always the target here.
            return other is TimeScaleEffect;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new TimeScaleEffect();
            cp.Init(origin, target, triggerData);
            cp = DeepCopy(cp, ignoreCooldown);

            return cp;
        }

        public override (bool queueCopy, bool isOverlapping) HandleEffectOverlapping(GameFeelEffect previous)
        {
            var (queueCopy, isOverlapping) = base.HandleEffectOverlapping(singletonCopy);
            if(queueCopy)
            {
                //Handling StackEffectType.Add locally
                if (isOverlapping && StackingType == StackEffectType.Add)
                {
                    Debug.LogWarning("TimeScaleEffect: StackingType == Add, will be handled like Replace.");
                    singletonCopy.StopExecution();
                }

                singletonCopy = this;
                return (true, false);
            }

            return (false, isOverlapping);
        }
    }
}