using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class CameraShakeEffect : DurationalGameFeelEffect
    {
        public CameraShakeEffect()
        {
            Description = "Simple camera shake effect.";
        }

        [Tooltip("No reference, makes the effect lookup a camera on the target or the main camera.")]
        public Camera cameraToModify;

        public bool useInteractionDirection;
        public float interactionDirectionMultiplier = 1f;
        
        public float amount = Random.value * 0.1f;

        public AnimationCurve easeAmountInOut = AnimationCurve.Constant(0, 1, 1f);

        // public bool useResetPositionAfterShake;
        // public Vector3 resetPosition;
        
        
        private Vector3 initialPosition;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new CameraShakeEffect
            {
                useInteractionDirection = useInteractionDirection, 
                amount = amount,
                cameraToModify = cameraToModify,
                // useResetPositionAfterShake = useResetPositionAfterShake, 
                // resetPosition = resetPosition
            };
            cp.Init(origin, target, triggerData);
            
            //Handling the cameraToModify setup here, to be able to a better use CompareTo
            if(cp.cameraToModify == null)
            {
                if (cp.target == null)
                {
                    cp.cameraToModify = Camera.main;
                }
                else
                {
                    cp.cameraToModify = cp.target.GetComponent<Camera>();
                    if(cp.cameraToModify == null)
                    {
                        cp.cameraToModify = Camera.main;
                    }    
                }
            }
            
            return DeepCopy(cp);
        }
        
        public override bool CompareTo(GameFeelEffect other)
        {
            return other is CameraShakeEffect cam && cam.cameraToModify == cameraToModify;
        }
        
        protected override void ExecuteSetup()
        {
            if (cameraToModify == null) return;
            
            var position = cameraToModify.transform.position;
            initialPosition = new Vector3(position.x, position.y, position.z);
            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (cameraToModify == null) return true;
            
            var direction = Random.onUnitSphere;
            
            if(useInteractionDirection)
            {
                var interactionDirection = Vector3.zero;
                
                switch (triggerData)
                {
                    case CollisionData collisionEvent:
                        interactionDirection = collisionEvent.GetInteractionDirection();
                        break;
                    //case MovementEvent movementEvent: //NOTE:MovementEvent is handled as PositionalEvent here!
                    case PositionalData positionalEvent:
                        interactionDirection = positionalEvent.DirectionDelta;
                        break;
                    // default:
                    //     throw new ArgumentOutOfRangeException(nameof(eventData), "EventType and useInteractionDirection not handled by CameraShakeEffect");
                }

                direction *= 0.5f;
                direction += interactionDirection * interactionDirectionMultiplier * 0.5f;
            }

            var deltaTime = Time.deltaTime;
            if (UnscaledTime) deltaTime = Time.unscaledDeltaTime;
            
            var easedAmount = amount * deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            direction *= easedAmount;
            
            cameraToModify.transform.position = cameraToModify.transform.position + direction;

            return false;
        }

        protected override void ExecuteComplete()
        {
            if (cameraToModify == null) return;
            
            //Guarantee we end up at initialPosition.
            // if (useResetPositionAfterShake)
            // {
            //     target.transform.position = resetPosition;
            // }
            // else
            // {
            cameraToModify.transform.position = initialPosition;
            //}
            
            base.ExecuteComplete();
        }
    }
}