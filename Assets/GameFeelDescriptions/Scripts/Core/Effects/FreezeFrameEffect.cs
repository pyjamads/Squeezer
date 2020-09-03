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
        }

        protected override void ExecuteSetup()
        {
            base.ExecuteSetup();

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

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new FreezeFrameEffect();
            //Always uses unscaled time, because this sets Time.timeScale to 0.
            cp.Init(origin, target, true, interactionDirection);
            cp = DeepCopy(cp);

            var (queueCopy, isOverlapping) = cp.HandleEffectOverlapping(singletonCopy);
            if (queueCopy)
            {
                //Handling StackEffectType.Add locally
                if (isOverlapping && StackingType == StackEffectType.Add)
                {
                    Debug.LogWarning("FreezeFrameEffect: StackingType == Add, will be handled like Queue.");
                    singletonCopy.OnComplete(cp);

                }
                else
                {
                    singletonCopy = cp;
                }

                return cp;
            }

            return null;
        }
    }
}