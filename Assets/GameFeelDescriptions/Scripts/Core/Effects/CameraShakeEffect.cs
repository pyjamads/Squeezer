using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    //TODO: consider making a camera wiggle (rotation) and wobble (using FOV and/or Orth-size, or two different ones) 2021-03-16
    public class CameraShakeEffect : ShakeEffect
    {
        public CameraShakeEffect()
        {
            Description = "Simple camera shake effect.";
        }

        [Tooltip("No reference, makes the effect lookup a camera on the target or the main camera.")]
        public Camera cameraToModify;

        public override void Mutate(float amount = 0.05f)
        {
            //TODO: make mutate for animationCurves!! 2020-11-09
            //easeAmountInOut.

            if (RandomExtensions.Boolean())
            {
                this.amount = Mathf.Max(0, amount + RandomExtensions.MutationAmount(amount));
            }

            if (RandomExtensions.Boolean(amount))
            {
                useInteractionDirection = !useInteractionDirection;
            }

            if (RandomExtensions.Boolean())
            {
                interactionDirectionMultiplier = Mathf.Min(0f, interactionDirectionMultiplier + RandomExtensions.MutationAmount(amount));
            }
            
            base.Mutate(amount);
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new CameraShakeEffect
            {
                cameraToModify = cameraToModify,
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
            
            return DeepCopy(cp, ignoreCooldown);
        }
        
        public override bool CompareTo(GameFeelEffect other)
        {
            return other is CameraShakeEffect cam && cam.cameraToModify == cameraToModify;
        }
        
        protected override void ExecuteSetup()
        {
            if (cameraToModify == null) return;
            
            target = cameraToModify.gameObject;
            
            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (cameraToModify == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }

            //This is done in ExecuteSetup
            //target = cameraToModify.gameObject;
            
            //Run the ShakeEffect.ExecuteTick
            return base.ExecuteTick();
        }

        public override void ExecuteCleanUp()
        {
            if (cameraToModify == null) return;
            
            target = cameraToModify.gameObject;
            
            base.ExecuteCleanUp();
        }
    }
}