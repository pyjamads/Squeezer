using UnityEngine;

namespace GameFeelDescriptions
{
    public class WobbleEffect : DurationalGameFeelEffect
    {
        public WobbleEffect()
        {
            Description = "Simple rotation shake effect. Can be used for shaking camera or other things.";
        }
        
        public float amount;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);
        
        public bool useResetScaleAfterShake;
        public Vector3 resetScale;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetScale;
        
        private float scaleOffset = 1f;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new WobbleEffect
            {
                amount = amount,
                useResetScaleAfterShake = useResetScaleAfterShake, 
                resetScale = resetScale,
                doNotResetScale = doNotResetScale
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;

            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (target == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            //var deltaTime = 1;//Time.deltaTime;
            //if (unscaledTime) deltaTime = Time.unscaledDeltaTime;
            
            var easedAmount = 1 + (Random.value * amount * 2f - amount) * deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            var scale = easedAmount;
            scaleOffset *= scale;
            
            target.transform.localScale = target.transform.localScale * scale;

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;
            
            target.transform.localScale = target.transform.localScale * (1f/scaleOffset);
        }
    }
}