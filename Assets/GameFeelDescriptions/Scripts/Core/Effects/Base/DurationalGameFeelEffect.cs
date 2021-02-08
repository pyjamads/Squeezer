using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public abstract class DurationalGameFeelEffect : GameFeelEffect
    {
        public enum LoopType
        {
            None,
            Yoyo,
            Restart,
            Relative,
        }
        
        [Header("Either specify Duration or allow the system to Randomize it, which can be restricted with DurationMin.")]
        public bool RandomizeDuration;
        
        //NOTE: HideFieldIf doesn't work when you have additional propertyDrawers on a field.
        //[HideFieldIf("RandomizeDuration", false)]
        [Tooltip("Used in conjunction with RandomizeDuration, to specify a lower bound for the random Duration.")]
        [DynamicRange(0f, "Duration")]
        public float DurationMin;
        
        /// <summary>
        /// Duration in seconds of execution.
        /// </summary>
        [Range(0f, 10f)] //Min is locked to zero.
        public float Duration = Random.Range(10, 121) / 100f;//Default value 0.1-1.2 in steps of 0.01.

        [Tooltip("Restart starts over at each iteration.\nYoyo goes back and forth between to and from.\nRelative uses the end value as a starting point for the next loop.")]
        [EnableFieldIf("repeat", (int)LoopType.None, negate: true)]
        [EnableFieldIf("DelayBetweenLoops", (int)LoopType.None, negate: true)]
        [EnableFieldIf("ExecuteEffectsOnLooping", (int)LoopType.None, negate: true)]
        public LoopType loopType;
        
        [HideInInspector]
        [Tooltip("Determines the number of times the effect should repeat (-1 for infinite). Duration is subdivided by repetition count.")]
        public int repeat = 1;

        [HideInInspector]
        public float DelayBetweenLoops = 0f;
        
        [HideInInspector]
        public bool ExecuteEffectsOnLooping;
        
        protected bool reverse;

        public override bool Tick(float unscaledDeltaTime)
        {
            //If this is the first Tick, after the delay, run setup
            if (firstTick && elapsed >= 0)
            {
                ExecuteSetup();
                firstTick = false;
            }
            
            var complete = false;
         
            var elapsedTimeExcess = 0f;
            if(!reverse && elapsed >= Duration)
            {
                elapsedTimeExcess = elapsed - Duration;
                elapsed = Duration;
                complete = true;
            }
            else if(reverse && elapsed <= 0)
            {
                elapsedTimeExcess = 0 - elapsed;
                elapsed = 0f;
                complete = true;
            }
            
            //negative elapsed, is used for setting delays.
            if(elapsed >= 0 && elapsed <= Duration)
            {
                //ExecuteTick can finish early, due to missing targets or other settings.
                complete = ExecuteTick() || complete;
            }
            
            // if we have a loopType and we are complete (meaning we reached 0 or duration) handle the loop.
            if (complete && (repeat > 0 || repeat == -1))
            {
                complete = HandleLooping( elapsedTimeExcess );

                //In case the effect is not complete, update again with the excess time.
                if (!complete && elapsedTimeExcess > 0)
                {
                    //ExecuteTick can finish early, due to missing targets or other settings.
                    complete = ExecuteTick() || complete;
                }

                //In case of looping, and executing effects on every loop
                if (!complete && ExecuteEffectsOnLooping)
                {
                    QueueEffects();
                }
            }
            
            var deltaTime = UnscaledTime ? unscaledDeltaTime : Time.timeScale * unscaledDeltaTime;
        
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
                    UpdateRelativeValues();
                    //Reset elapsed time
                    elapsed = 0;
                    break;
            }
            
            //if we have a delay between loops, add or remove that value to/from elapsed.
            if (DelayBetweenLoops > 0)
            {
                if (reverse)
                {
                    elapsed += DelayBetweenLoops;
                }
                else
                {
                    elapsed -= DelayBetweenLoops;
                }
            }

            // if we have loops left to process reset our state with the excess time and continue
            if(loopType != LoopType.None && (repeat >= 0 || repeat == -1))
            {
                // now we need to set our elapsed time and factor in our excess time
                if (reverse)
                {
                    elapsed -= excessTime;
                }
                else
                {    
                    elapsed += excessTime;
                }
					
                //Signal that we're not quite done yet.
                return false;
            }

            //If we got here, the loops are done!
            return true;
        }

        /// <summary>
        /// Implement this to control how relative values are updated in a looping context.
        /// </summary>
        protected virtual void UpdateRelativeValues()
        {
            //NOTE: Handle updating relative values between loops.
        }
        
        public override float GetRemainingTime(bool includeDelay = false)
        {
            var total = Duration - elapsed;
            
            if (includeDelay)
            {
                total = Duration + Delay;
            }

            if (loopType == LoopType.None)
            {
                return total;
            }
            
            if (repeat > 0)
            {
                total += repeat * Duration + repeat * DelayBetweenLoops;
            }
            else if (repeat == -1)
            {
                total = float.PositiveInfinity;
            }

            return total;
        }

        protected override T DeepCopy<T>(T shallow) 
        {
            //If the shallow is not DurationalGameFeelEffect, return null instead. 
            if (!(shallow is DurationalGameFeelEffect cp)) return null;
            
            //Setup Duration when we copy it!
            //NOTE: This is slightly hidden logic... 2020-09-22 
            if (RandomizeDuration)
            {
                cp.Duration = Random.Range(DurationMin, Duration);
            }
            else
            {
                cp.Duration = Duration;
            }
            
            cp.loopType = loopType;
            cp.repeat = repeat;
            cp.DelayBetweenLoops = DelayBetweenLoops;
            cp.ExecuteEffectsOnLooping = ExecuteEffectsOnLooping;

            return base.DeepCopy(cp as T);
        }

        public override void Mutate(float amount = 0.05f)
        {
            base.Mutate(amount);
            
            if (RandomExtensions.Boolean())
            {
                var durationAmount = Random.value * amount * 2 - amount;
                Duration = Mathf.Max(0,Duration + durationAmount);    
            }

            //NOTE: We won't mutate loop type here, individual effects can do that if it makes sense.
            
            if (RandomExtensions.Boolean())
            {
                var delayAmount = Random.value * amount * 2 - amount;
                DelayBetweenLoops = Mathf.Max(0, DelayBetweenLoops + delayAmount);
            }
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is DurationalGameFeelEffect && base.CompareTo(other);
        }
    }
}