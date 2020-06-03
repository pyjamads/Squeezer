using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class DurationalGameFeelEffect : GameFeelEffect
    {
        //TODO: make a base class that doesn't have duration... 20/03/2020
        //NOTE: If this is 0, then any OnCompletes will be executed immediately.
        //NOTE: It might not be a relevant value for all effects.
        /// <summary>
        /// Duration in seconds of execution.
        /// </summary>
        [Range(0, 5)]
        public float Duration = Random.Range(10, 121) / 100f;//Default value 0.1-1.2 in steps of 0.01.

        public override float GetRemainingTime()
        {
            return Duration - elapsed;
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
         
            if(elapsed >= Duration)
            {   
                elapsed = Duration;
                complete = true;
            }
         
            //negative elapsed, is used for setting delays.
            if(elapsed >= 0 && elapsed <= Duration)
            {
                //ExecuteTick can finish early, due to missing targets or other settings.
                complete = ExecuteTick() || complete;
            }
         
            elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (!complete) return false;
         
            //Queue effects in the ExecuteAfterCompletion list
            ExecuteComplete();
            
            return true;
        }

        protected override T DeepCopy<T>(T shallow) 
        {
            if (shallow is DurationalGameFeelEffect cp)
            {
                cp.Duration = Duration;

                return base.DeepCopy(cp as T);
            }
            
            return base.DeepCopy(shallow);
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is DurationalGameFeelEffect && base.CompareTo(other);
        }
    }
}