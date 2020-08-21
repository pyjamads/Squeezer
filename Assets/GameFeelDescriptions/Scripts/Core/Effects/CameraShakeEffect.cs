using UnityEngine;

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

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new CameraShakeEffect
            {
                useInteractionDirection = useInteractionDirection, 
                amount = amount,
                cameraToModify = cameraToModify,
                // useResetPositionAfterShake = useResetPositionAfterShake, 
                // resetPosition = resetPosition
            };
            cp.Init(origin, target, unscaledTime, interactionDirection);
            
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
            
            if(useInteractionDirection && interactionDirection != null)
            {
                direction *= 0.5f;
                direction += interactionDirection.Value * interactionDirectionMultiplier * 0.5f;
            }

            var deltaTime = 1;//Time.deltaTime;
            //if (unscaledTime) deltaTime = Time.unscaledDeltaTime;
            
            var easedAmount = amount * deltaTime;
            if (Duration > 0)
            {
                easedAmount = easeAmountInOut.Evaluate(elapsed / Duration) * easedAmount;
            }

            direction *= easedAmount; //* Random.value 
            
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