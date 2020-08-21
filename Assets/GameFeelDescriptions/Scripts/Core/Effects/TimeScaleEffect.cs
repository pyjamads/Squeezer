using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using JetBrains.Annotations;
using UnityEngine;

namespace GameFeelDescriptions
{
    [UsedImplicitly]
    public class TimeScaleEffect : TweenEffect<float>
    {
        private static TimeScaleEffect singletonCopy;

//        [EnableFieldIf("ExecuteAfterCompletion", true)]
//        public bool showOnCompleteEffects = true;
        
        public TimeScaleEffect()
        {
            Description = "TimeScale Effect allows you to change timeScale using easing.";
            relative = false;
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
            SetValue(target, TweenHelper.Interpolate(start, elapsed / duration, end, GetEaseFunc()));
            
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

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime, Vector3? interactionDirection = null)
        {
            var cp = new TimeScaleEffect();
            cp.Init(origin, target, unscaledTime, interactionDirection);
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

//            if (singletonCopy != null)
//            {
//                switch (StackingType)
//                {
//                    case StackEffectType.Discard:
//                        return null;
//                    case StackEffectType.Replace:
//                        GameFeelEffectExecutor.Instance.RemoveEffect(singletonCopy);
//                        singletonCopy = cp;
//                        break;
//                    case StackEffectType.OverrideGoal:
//                        //Update end value.
//                        singletonCopy.end = GetEndValue();
//                        //Update time to match relative progression (linearly).
//                        singletonCopy.elapsed = GameFeelTween.InverseLerp(singletonCopy.start, singletonCopy.end, singletonCopy.GetValue(singletonCopy.target));
//                        return null;
//                    case StackEffectType.Queue:
//                        singletonCopy.ExecuteAfterCompletion.Add(cp);
//                        return null;
//                    default:
//                        throw new ArgumentOutOfRangeException();
//                }
//            }
//            else
//            {
//                singletonCopy = cp;
//            }
//
//            return cp;
        }
    }
}