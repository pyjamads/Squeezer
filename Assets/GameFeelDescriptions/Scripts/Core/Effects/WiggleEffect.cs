using UnityEngine;

namespace GameFeelDescriptions
{
    public class WiggleEffect : DurationalGameFeelEffect
    {
        public WiggleEffect()
        {
            Description = "Simple rotation shake effect. Can be used for shaking camera or other things.";
        }

        public float amount;
        public Vector3 axis = Vector3.one;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);
        
        public bool useResetRotationAfterShake;
        public Vector3 resetRotation;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetRotation;
        
        private Quaternion rotationOffset = Quaternion.identity;

        //Jitter control, eg. how often it switches target.
        private const float jitterCooldown = 0.01f;
        private float timeSinceLastJitter;
        private Quaternion jitterTarget;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new WiggleEffect
            {
                amount = amount,
                axis = axis,
                easeAmountInOut = easeAmountInOut,
                useResetRotationAfterShake = useResetRotationAfterShake, 
                resetRotation = resetRotation,
                doNotResetRotation = doNotResetRotation
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;

            jitterTarget = Random.rotation;

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
            
            //Reset every step
            //target.transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles - rotationOffset);
            //rotationOffset = Vector3.zero;

            timeSinceLastJitter += deltaTime;

            if (timeSinceLastJitter > jitterCooldown)
            {
                timeSinceLastJitter = 0;
                jitterTarget = Quaternion.AngleAxis(Random.value * 360 - 180, axis);

                target.transform.rotation = target.transform.rotation * Quaternion.Inverse(rotationOffset);
                
                rotationOffset = Quaternion.identity;
            }
            
            var easedAmount = amount * deltaTime; 
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            var rotation = Quaternion.SlerpUnclamped(target.transform.rotation, jitterTarget, easedAmount);
            rotationOffset *= Quaternion.Inverse(target.transform.rotation) * rotation;
            target.transform.rotation = rotation;

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;

            if (doNotResetRotation == false)
            {
                //Guarantee we end up at initialPosition.
                if (useResetRotationAfterShake)
                {
                    target.transform.rotation = Quaternion.Euler(resetRotation);
                }
                else
                {
                    target.transform.rotation = target.transform.rotation * Quaternion.Inverse(rotationOffset);
                }
            }
        }
    }
}