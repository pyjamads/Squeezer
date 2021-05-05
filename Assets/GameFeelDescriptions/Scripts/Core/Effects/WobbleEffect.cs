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
        public Vector3 axis = Vector3.one;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);
        
        public bool useResetScaleAfterShake;
        public Vector3 resetScale;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetScale;
        
        private float scaleOffset = 0f;
        
        //Jitter control, eg. how often it switches target.
        private const float jitterCooldown = 0.01f;
        private float timeSinceLastJitter;
        private float jitterTarget;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new WobbleEffect
            {
                amount = amount,
                axis = axis,
                easeAmountInOut = easeAmountInOut,
                useResetScaleAfterShake = useResetScaleAfterShake, 
                resetScale = resetScale,
                doNotResetScale = doNotResetScale
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;

            jitterTarget = 1 + (Random.value * amount * 2 - amount);
            
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
            
            timeSinceLastJitter += deltaTime;
            
            if (timeSinceLastJitter > jitterCooldown)
            {
                timeSinceLastJitter = 0;
                jitterTarget = Random.value * amount * 2 - amount;

                target.transform.localScale = target.transform.localScale - axis * scaleOffset;
                
                scaleOffset = 0f;
            }
            
            var easedAmount = deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            var scale = jitterTarget * easedAmount;
            
            scaleOffset += scale;
            
            target.transform.localScale = target.transform.localScale + axis * scale;

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;

            if (doNotResetScale == false)
            {
                //Guarantee we end up at initialPosition.
                if (useResetScaleAfterShake)
                {
                    target.transform.localScale = resetScale;
                }
                else
                {
                    target.transform.localScale = target.transform.localScale - axis * scaleOffset;
                }
            }
        }
    }
}