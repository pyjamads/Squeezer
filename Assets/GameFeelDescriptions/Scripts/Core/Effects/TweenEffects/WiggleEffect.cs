using UnityEngine;

namespace GameFeelDescriptions
{
    public class WiggleEffect : DurationalGameFeelEffect
    {
        public WiggleEffect()
        {
            Description = "Simple shake effect. Can be used for shaking camera or other things.";
        }

        public bool useInteractionDirection;
        public float interactionDirectionMultiplier = 1f;
        
        public float amount;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);
        
        public bool useResetRotationAfterShake;
        public Vector3 resetRotation;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetPosition;
            
        private Vector3 initialRotation;

        private Vector3 interactionDirection = Vector3.zero;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new WiggleEffect
            {
                useInteractionDirection = useInteractionDirection, 
                amount = amount,
                useResetRotationAfterShake = useResetRotationAfterShake, 
                resetRotation = resetRotation
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;
            
            var rotation = target.transform.rotation;
            initialRotation = rotation.eulerAngles;
            
            interactionDirection = Vector3.zero;

            switch (triggerData)
            {
                case CollisionData collisionEvent:
                    interactionDirection = collisionEvent.GetInteractionDirection();
                    break;
                case PositionalData positionalEvent:
                    interactionDirection = positionalEvent.DirectionDelta;
                    break;
            }
            
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
            
            var direction = Random.onUnitSphere;
            
            if(useInteractionDirection)
            {
                direction *= 0.5f;
                direction += interactionDirection * interactionDirectionMultiplier * 0.5f;
            }

            var deltaTime = 1;//Time.deltaTime;
            //if (unscaledTime) deltaTime = Time.unscaledDeltaTime;
            
            var easedAmount = amount * deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            direction *= easedAmount; //* Random.value 
            
            target.transform.rotation = Quaternion.Euler(target.transform.rotation.eulerAngles + direction);

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;

            if (doNotResetPosition == false)
            {
                //Guarantee we end up at initialPosition.
                if (useResetRotationAfterShake)
                {
                    target.transform.rotation = Quaternion.Euler(resetRotation);
                }
                else
                {
                    target.transform.position = initialRotation;
                }
            }
        }
    }
}