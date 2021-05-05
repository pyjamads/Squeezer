using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public abstract class TweenEffect<TTween> : DurationalGameFeelEffect
    {
        [EnableFieldIf("from", true)]
        public bool setFromValue;
        
        [Tooltip("If @setFromValue is true, this will be used as start value.")]
        [HideInInspector]
        public TTween @from;
        
        public bool relative = true;
        //Relative difference between to and from.
        protected TTween diffAmount;
        
        [Tooltip("If @relative is true, this will be added to the initial value.")]
        public TTween to = (TTween)TweenHelper.GetRandomValue(typeof(TTween), Random.Range(0f, 5f));

        [Tooltip("Easing adjusts the value non-linearly over the duration.")]
        public EasingHelper.EaseType easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});

        [HideInInspector]
        public AnimationCurve curve; 

        // [Tooltip("Restart starts over at each iteration.\nYoyo goes back and forth between to and from.\nRelative uses the end value as a starting point for the next loop.")]
        // [EnableFieldIf("repeat", (int)LoopType.None, negate: true)]
        // [EnableFieldIf("DelayBetweenLoops", (int)LoopType.None, negate: true)]
        // public LoopType loopType;
        //
        // [HideInInspector]
        // [Tooltip("Determines the number of times the effect should repeat (-1 for infinite). Duration is subdivided by repetition count.")]
        // public int repeat = 1;
        //
        // public float DelayBetweenLoops = 0f;
        //
        // protected bool reverse;
        // protected float duration;
        
        protected TTween start;
        protected TTween end;
        
        protected override T DeepCopy<T>(T shallow, bool ignoreCooldown)
        {
            //If the shallow is not TweenEffect, return null instead. 
            if (!(shallow is TweenEffect<TTween> cp)) return null;
            
            cp.setFromValue = setFromValue;
            cp.@from = @from;
            cp.relative = relative;
            cp.to = to;
            cp.easing = easing;
            cp.curve = curve;
                
            return base.DeepCopy(cp as T, ignoreCooldown);

        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is TweenEffect<TTween> && base.CompareTo(other);
        }
        
        public Func<float, float> GetEaseFunc()
        {
            if (easing != EasingHelper.EaseType.Curve)
                return EasingHelper.Ease(easing);

            if (curve == null)
            {
                Debug.LogWarning(GetType().Name + " has easeType set to Curve, but no curve defined!");
                return EasingHelper.Linear;
            }
            
            return t => curve.Evaluate(t);
        }

        protected override void UpdateRestartValues()
        {
            //Reset it back to the start!
            SetValue(target, start);
        }

        protected override void UpdateRelativeValues()
        {
            //Update Start and End values, relative to the current value.
            end = GetRelativeValue(GetValue(target), GetDifference(start, end));
            start = GetValue(target);
        }

        protected virtual TTween GetStartValue()
        {
            return setFromValue ? @from : GetValue(target);
        }

        protected virtual TTween GetEndValue()
        {
            return relative ? GetRelativeValue(GetValue(target), to) : to;
        }
        
        protected override void ExecuteSetup()
        {
            //TODO: handle the issue of tweening a target list with relative start points. ie. slide in many blocks from different positions. 07/02/2020
            
            //Setup start and end values.
            start = GetStartValue();
            end = GetEndValue();
            SetValue(target, start);

            diffAmount = GetDifference(start, end);
        }

        protected override bool ExecuteTick()
        {
            if (Duration > 0)
            {
                return TickTween();
            }

            //If duration is 0, just set the end value directly.
            SetValue(target, end);
            
            //If we got here, the tween is also done!
            return true;
        }

        /// <summary>
        /// Returns the current value.
        /// </summary>
        /// <returns></returns>
        protected abstract void SetValue(GameObject target, TTween value);

      
        /// <summary>
        /// Returns the current value.
        /// </summary>
        /// <returns></returns>
        protected abstract TTween GetValue(GameObject target);
      
        /// <summary>
        /// Returns the relative value based on <see cref="to"/>.
        /// </summary>
        /// <returns></returns>
        protected abstract TTween GetRelativeValue(TTween fromValue, TTween addValue);

        /// <summary>
        /// Returns the relative difference between <param name="from"/> and <param name="to"/>.
        /// </summary>
        /// <paramIEnumerator name="fromValue"></param>
        /// <param name="toValue"></param>
        /// <returns></returns>
        protected abstract TTween GetDifference(TTween fromValue, TTween toValue);

        /// <summary>
        /// Executes the actual tween.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="start"></param>
        /// <param name="duration"></param>
        /// <param name="end"></param>
        /// <param name="interactionDirection"></param>
        /// <param name="unscaledTime"></param>
        /// <returns></returns>
        protected abstract bool TickTween();
    }
}