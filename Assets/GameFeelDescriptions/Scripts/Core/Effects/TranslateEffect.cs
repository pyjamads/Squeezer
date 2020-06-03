using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
    //TODO: Make a customProperty drawer that can handle these, so it can be enabled!

    public class TranslateEffect : TweenEffect<Vector3>
    {
        [Tooltip("Use global position instead of local position.")]
        public bool useGlobalPosition;
        
        public TranslateEffect()
        {
            Description = "Translate Effect allows you to move an object using easing.";
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new TranslateEffect {useGlobalPosition = useGlobalPosition};
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override void SetValue(GameObject target, Vector3 value)
        {
            if (target == null) return;
            
            if (useGlobalPosition)
            {
                target.transform.position = value;    
            }
            else
            {
                target.transform.localPosition = value;
            }
        }

        protected override Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            return useGlobalPosition ? target.transform.position : target.transform.localPosition;
        }

        protected override Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return TweenHelper.GetRelativeValue(fromValue, addValue);
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
    }
}