using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class RotateTowardsDirectionEffect : GameFeelEffect
    {
        public RotateTowardsDirectionEffect()
        {
            Description = "Allows you to rotate an object some amount towards a target direction.";
        }
        
        [Tooltip("Use global rotation instead of local rotation.")]
        public bool useGlobalRotation;

        [Tooltip("The relative amount to rotate for every update.")]
        [Range(0f, 1f)]
        public float relativeAmount;

        public bool only2D;
        
        private Quaternion start;
        private Quaternion end;

//        [Tooltip("The notion of forward for the object.")]
//        public Vector3 forwardDirection;
//        
//        [Tooltip("The notion of up for the object.")]
//        public Vector3 upDirection;

        //private Quaternion forwardCorrection;

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new RotateTowardsDirectionEffect{useGlobalRotation = useGlobalRotation, relativeAmount = relativeAmount, only2D = only2D};
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected void SetValue(GameObject target, Quaternion value)
        {
            if (target == null) return;
            
            if (useGlobalRotation)
            {
                target.transform.rotation = value;  
            }
            else
            {
                target.transform.localRotation = value;
            }
        }

        protected Quaternion GetValue(GameObject target)
        {
            if (target == null) return Quaternion.identity;
            
            if (useGlobalRotation)
            {
                return target.transform.rotation;    
            } 
            
            return target.transform.localRotation;
        }

        protected override void ExecuteSetup()
        {
            start = GetValue(target);
            end = GetValue(target);
            
            if (interactionDirection != null)
            {
                if (only2D)
                {   
                    //TODO: Assuming z-axis is always up (aka towards camera), consider adding an rotate around vector.
                    var angle = Vector3.SignedAngle(Vector3.right, interactionDirection.Value, Vector3.back);
                    
                    end = Quaternion.AngleAxis(angle, Vector3.back);
                }
                else
                {
                    end = Quaternion.LookRotation(interactionDirection.Value, target.transform.up);    
                }
            }
            
            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            if (target == null) return true;
            
            var nextRotation = TweenHelper.Interpolate(start, relativeAmount, end, EasingHelper.Linear);
            SetValue(target, nextRotation);

            return false;
        }
    }
}