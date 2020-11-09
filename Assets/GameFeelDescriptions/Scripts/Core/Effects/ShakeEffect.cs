using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class ShakeEffect : DurationalGameFeelEffect
    {
        public ShakeEffect()
        {
            Description = "Simple shake effect. Can be used for shaking camera or other things.";
        }

        public bool useInteractionDirection;
        public float interactionDirectionMultiplier = 1f;
        
        public float amount;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);

        
        public bool useResetPositionAfterShake;
        public Vector3 resetPosition;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetPosition;
            
        private Vector3 initialPosition;

        private Vector3 interactionDirection = Vector3.zero;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new ShakeEffect
            {
                useInteractionDirection = useInteractionDirection, 
                amount = amount,
                useResetPositionAfterShake = useResetPositionAfterShake, 
                resetPosition = resetPosition
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;
            
            var position = target.transform.position;
            initialPosition = new Vector3(position.x, position.y, position.z);
            
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
            if (target == null) return true;
            
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
            
            target.transform.position = target.transform.position + direction;

            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;

            if (doNotResetPosition == false)
            {
                //Guarantee we end up at initialPosition.
                if (useResetPositionAfterShake)
                {
                    target.transform.position = resetPosition;
                }
                else
                {
                    target.transform.position = initialPosition;
                }
            }
        }
    }
}