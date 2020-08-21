using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public abstract class TweenEffect<TTween> : DurationalGameFeelEffect
    {
        public enum LoopType
        {
            None,
            Yoyo,
            Restart,
            Relative,
        }
        
        [EnableFieldIf("from", true)]
        public bool setFromValue;
        
        [Tooltip("If @setFromValue is true, this will be used as start value.")]
        [HideInInspector]
        public TTween @from;
        
        public bool relative = true;
        
        [Tooltip("If @relative is true, this will be added to the initial value.")]
        public TTween to = (TTween)TweenHelper.GetRandomValue(typeof(TTween), Random.Range(0f, 5f));

        [Tooltip("Easing adjusts the value non-linearly over the duration.")]
        public EasingHelper.EaseType easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});

        [HideInInspector]
        public AnimationCurve curve; 

        [Tooltip("Restart starts over at each iteration.\nYoyo goes back and forth between to and from.\nRelative uses the end value as a starting point for the next loop.")]
        [EnableFieldIf("repeat", (int)LoopType.None, negate: true)]
        public LoopType loopType;
        
        [HideInInspector]
        [Tooltip("Determines the number of times the effect should repeat (-1 for infinite). Duration is subdivided by repetition count.")]
        public int repeat = 1;
        
        protected TTween start;
        protected TTween end;

        protected bool reverse;
        protected float duration;

        protected override T DeepCopy<T>(T shallow)
        {
            if (shallow is TweenEffect<TTween> cp)
            {
                cp.setFromValue = setFromValue;
                cp.@from = @from;
                cp.relative = relative;
                cp.to = to;
                cp.easing = easing;
                cp.curve = curve;
                cp.loopType = loopType;
                cp.repeat = repeat;
                
                cp = base.DeepCopy(cp);

                cp.SetupLooping();

                return cp as T;
            }
            
            return base.DeepCopy(shallow);
        }
        
        public override void Randomize()
        {
            //10% chance to setFromValue 
            setFromValue = RandomExtensions.Boolean(0.1f);

            if (setFromValue)
            {
                @from = (TTween)TweenHelper.GetRandomValue(typeof(TTween), Random.Range(0f, 5f));
            }
            else
            {
                @from = default;
            }

            //No chance to set the relative value to true, because it's kinda weird with colors.
            //relative = RandomExtensions.Boolean(0.05f);
            relative = false;
            
            to = (TTween)TweenHelper.GetRandomValue(typeof(TTween), Random.Range(0f, 5f));
            
            
            easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});
            //NOTE: curve is excluded here.
            
            loopType = EnumExtensions.GetRandomValue<LoopType>();
            repeat = Random.Range(-1, 3);

            base.Randomize();
            
            //NOTE: Need that SetElapsed in DurationalGameFeelEffect to be run first.
            SetupLooping();
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is TweenEffect<TTween> && base.CompareTo(other);
        }

        public override bool Tick()
        {
            //If this is the first Tick, after the delay, run setup
            if (firstTick && elapsed >= 0)
            {
                ExecuteSetup();
                firstTick = false;
            }
            
            var complete = false;
         
            var elapsedTimeExcess = 0f;
            if(!reverse && elapsed >= duration)
            {
                elapsedTimeExcess = elapsed - duration;
                elapsed = duration;
                complete = true;
            }
            else if(reverse && elapsed <= 0)
            {
                elapsedTimeExcess = 0 - elapsed;
                elapsed = 0f;
                complete = true;
            }
            
            //negative elapsed, is used for setting delays.
            if(elapsed >= 0 && elapsed <= duration)
            {
                //ExecuteTick can finish early, due to missing targets or other settings.
                complete = ExecuteTick() || complete;
            }
            
            // if we have a loopType and we are complete (meaning we reached 0 or duration) handle the loop.
            if (complete && (repeat > 0 || repeat == -1))
            {
                complete = HandleLooping( elapsedTimeExcess );

                //In case the tween is not complete, update the tween with the excess time.
                if (!complete && elapsedTimeExcess > 0)
                {
                    //ExecuteTick can finish early, due to missing targets or other settings.
                    complete = ExecuteTick() || complete;
                }
            }
            
            var deltaTime = unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        
            // running in reverse? then we need to subtract deltaTime
            if (reverse)
            {
                elapsed -= deltaTime;
            }
            else
            {
                elapsed += deltaTime;
            }

            if (!complete) return false;
            
            //Queue effects in the ExecuteAfterCompletion list
            ExecuteComplete();
            
            return true;
        }

        private bool HandleLooping(float excessTime)
        {
            //If repeat is -1, we repeat infinitely.
            if (repeat != -1)
            {
                repeat--;
            }
            
            switch (loopType)
            {
                case LoopType.Yoyo:
                    //Start and End are still the same.
                    //Elapsed is either 0 or duration.
                    //Flip direction of the tween.
                    reverse = !reverse;
                    break;
                case LoopType.Restart:
                    //Start and End are still the same.
                    //Reset elapsed time
                    elapsed = 0;
                    break;
                case LoopType.Relative:
                    //Update Start and End values, relative to the current value.
                    end = GetRelativeValue(GetValue(target), GetDifference(start, end));
                    start = GetValue(target);
                    //Reset elapsed time
                    elapsed = 0;
                    break;
            }

            // if we have loops left to process reset our state back to Running so we can continue processing them
            if(loopType != LoopType.None && (repeat >= 0 || repeat == -1))
            {
                // now we need to set our elapsed time and factor in our elapsedTimeExcess
                if (reverse)
                {
                    elapsed -= excessTime;
                }
                else
                {    
                    elapsed = excessTime;
                }
					
                //Signal that we're not quite done yet.
                return false;
            }

            //If we got here, the loops are done!
            return true;
        }
        
        public override float GetRemainingTime()
        {
            var total = duration - elapsed;
            
            if (loopType == LoopType.None)
            {
                return total;
            }
            
            if (repeat > 0)
            {
                total += repeat * duration;
            }
            else if (repeat == -1)
            {
                total = float.PositiveInfinity;
            }

            return total;
        }

        public void SetupLooping()
        {
            //Setup looping.
            if (loopType != LoopType.None && repeat > 0)
            {
                duration = Duration / (repeat + 1);  
            }
            else
            {
                //No loops, means use Duration directly.
                duration = Duration;
            }
        }

        public Func<float, float> GetEaseFunc()
        {
            if (easing != EasingHelper.EaseType.Curve)
                return EasingHelper.Ease(easing);

            if (curve == null)
            {
                Debug.LogWarning(GetType().Name + " has easeType set to Curve, but no curve defined!");
                return (t) => t;
            }
            
            return t => curve.Evaluate(t);
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
        }

        protected override bool ExecuteTick()
        {
            if (duration > 0)
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