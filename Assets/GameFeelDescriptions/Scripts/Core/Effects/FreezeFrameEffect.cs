using JetBrains.Annotations;
using UnityEngine;

namespace GameFeelDescriptions
{
    [UsedImplicitly]
    public class FreezeFrameEffect : DurationalGameFeelEffect
    {
        private static FreezeFrameEffect singletonCopy;

        private float initialTimeScale;
        
        public FreezeFrameEffect()
        {
            Description = "Freeze Frame Effect allows you to pause time for a duration.";
            UnscaledTime = true;

            Duration = Random.Range(1, 30) / 100f;
        }

        protected override void ExecuteSetup()
        {
            base.ExecuteSetup();

            UnscaledTime = true;
            initialTimeScale = Time.timeScale;
            Time.timeScale = 0;
        }

        protected override bool ExecuteTick()
        {
            if (singletonCopy != this)
            {
                //This should never be the case, but if it is, end early!
                return true;
            }

            return false;
        }

        public override void ExecuteCleanUp()
        {
            //This should always be the case!
            if (singletonCopy == this)
            {
                singletonCopy = null;
            }
            
            Time.timeScale = initialTimeScale;

            base.ExecuteCleanUp();
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            //Time.timeScale is always the target here.
            return other is FreezeFrameEffect;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new FreezeFrameEffect();
            //Always uses unscaled time, because this sets Time.timeScale to 0.
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
                    Debug.LogWarning("FreezeFrameEffect: StackingType == Add, will be handled like Replace.");
                    singletonCopy.StopExecution();
                }

                singletonCopy = this;
                return (true, false);
            }

            return (false, isOverlapping);
        }
    }
}