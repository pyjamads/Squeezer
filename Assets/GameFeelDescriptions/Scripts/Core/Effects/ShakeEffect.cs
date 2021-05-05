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
        public Vector3 axis = Vector3.one;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);
        
        public bool useResetPositionAfterShake;
        public Vector3 resetPosition;

        [HideFieldIf("useResetPositionAfterShake", true)]
        public bool doNotResetPosition; 
        
        
        private Vector3 positionOffset;

        private Vector3 interactionDirection = Vector3.zero;

        //Jitter control, eg. how often it switches target.
        private const float jitterCooldown = 0.01f;
        private float timeSinceLastJitter;
        private Vector3 jitterTarget;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new ShakeEffect
            {
                useInteractionDirection = useInteractionDirection, 
                interactionDirectionMultiplier = interactionDirectionMultiplier,
                amount = amount,
                axis = axis,
                easeAmountInOut = easeAmountInOut,
                useResetPositionAfterShake = useResetPositionAfterShake, 
                resetPosition = resetPosition,
                doNotResetPosition = doNotResetPosition
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }
        
        //TODO: add mutate!!!
        
        protected override void ExecuteSetup()
        {
            if (target == null) return;
            
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
            
            timeSinceLastJitter += deltaTime;
            
            if (timeSinceLastJitter > jitterCooldown)
            {
                timeSinceLastJitter = 0;
                
                var direction = Random.onUnitSphere;
            
                if(useInteractionDirection)
                {
                    direction += interactionDirection * interactionDirectionMultiplier;
                }
                jitterTarget = axis.multiplyElements(direction * amount);

                target.transform.position = target.transform.position - positionOffset;
                
                positionOffset = Vector3.zero;
            }
            
            var easedAmount = deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }
            
            positionOffset += jitterTarget * easedAmount;
            target.transform.position = target.transform.position + jitterTarget * easedAmount;

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
                    target.transform.position = target.transform.position - positionOffset;
                }
            }
        }
    }
}