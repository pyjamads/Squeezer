using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class RotateEffect : TweenEffect<Vector3>
    {
        public RotateEffect()
        {
            Description = "Rotate Effect allows you to rotate an object using easing.";
        }
        
        [Tooltip("Use global rotation instead of local rotation.")]
        public bool useGlobalRotation;

        protected override void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;
            
            if (useGlobalRotation)
            {
                target.transform.eulerAngles = value;  
            }
            else
            {
                target.transform.localEulerAngles = value;
            }
        }

        protected override Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            if (useGlobalRotation)
            {
                return target.transform.eulerAngles;    
            } 
            
            return target.transform.localEulerAngles;
        }

        protected override Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return fromValue + addValue;
        }

        protected override Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
        {
            return toValue - fromValue;
        }
        
        protected override bool TickTween()
        {
            if (target == null) return true;
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / duration, end, GetEaseFunc()));

            return false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new RotateEffect{useGlobalRotation = useGlobalRotation};
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }
    }
}