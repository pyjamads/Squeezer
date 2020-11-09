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
            SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));
            
            //We never need to breakout early here.
            return false;
        }

        protected override void ExecuteComplete()
        {
            //This should always be the case!
            if (singletonCopy == this)
            {
                singletonCopy = null;
            }
            
            base.ExecuteComplete();
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            //Time.timeScale is always the target here.
            return other is TimeScaleEffect;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new TimeScaleEffect();
            cp.Init(origin, target, triggerData);
            cp = DeepCopy(cp);

            var (queueCopy, isOverlapping) = cp.HandleEffectOverlapping(singletonCopy);
            if(queueCopy)
            {
                //Handling StackEffectType.Add locally
                if (isOverlapping && StackingType == StackEffectType.Add)
                {
                   Debug.LogWarning("TimeScaleEffect: StackingType == Add, will be handled like Replace.");
                   singletonCopy.StopExecution();
                }

                singletonCopy = cp;
                return cp;
            }
            
            return null;
        }
    }
}