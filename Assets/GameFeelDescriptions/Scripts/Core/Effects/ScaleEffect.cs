using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class ScaleEffect : TweenEffect<Vector3>
    {   
        public ScaleEffect()
        {
            Description = "Scale Effect allows you to scale an object using easing.";
        }

        protected override void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;
            
            target.transform.localScale = value;
        }

        protected override Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            return target.transform.localScale;
        }

        protected override Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return new Vector3(fromValue.x * addValue.x,fromValue.y * addValue.y,fromValue.z * addValue.z);
        }

        protected override Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
        {
            return TweenHelper.GetDifference(fromValue, toValue);
        }

        //TODO: implement interactionDirection in effects. 07/02/2020
        
        protected override bool TickTween()
        {
            if (target == null) return true;
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / duration, end, GetEaseFunc()));

            return false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new ScaleEffect();
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }
    }
    
//    public class ScaleWithVelocityEffect : GameFeelEffect
//    {   
//        public ScaleWithVelocityEffect()
//        {
//            Description = "Scale Effect allows you to scale an object using easing.";
//        }
//        
//        [Tooltip("The relative amount to rotate for every update.")]
//        [Range(0f, 1f)]
//        public float relativeAmount;
//        
//        private Quaternion start;
//        private Quaternion end;
//
//        protected void SetValue(GameObject target, Vector3 value)
//        {
//            if (target == null) return;
//            
//            target.transform.localScale = value;
//        }
//
//        protected Vector3 GetValue(GameObject target)
//        {
//            if (target == null) return Vector3.zero;
//            
//            return target.transform.localScale;
//        }
//
//        protected Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
//        {
//            return new Vector3(fromValue.x * addValue.x,fromValue.y * addValue.y,fromValue.z * addValue.z);
//        }
//
//        protected Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
//        {
//            return toValue - fromValue;
//        }
//        
//        protected override void ExecuteSetup()
//        {
//            start = GetValue(target);
//            end = GetValue(target);
//            
//            if (interactionDirection != null)
//            {
//               
//               end = target.transform.localScale * interactionDirection.Value;    
//            }
//            
//            base.ExecuteSetup();
//        }
//
//        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
//            Vector3? interactionDirection = null)
//        {
//            var cp = new ScaleEffect();
//            cp.Init(origin, target, unscaledTime, interactionDirection);
//            return DeepCopy(cp);
//        }
//
//        protected override bool ExecuteTick()
//        {
//            if (target == null) return true;
//            
//            SetValue(target, GameFeelTween.Interpolate(start, elapsed / duration, end, GetEaseFunc()));
//
//            return false;
//        }
//    }
}