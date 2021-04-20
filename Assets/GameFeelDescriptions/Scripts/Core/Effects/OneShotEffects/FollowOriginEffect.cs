using System.Collections;
using UnityEngine;
using UnityEngine.Assertions.Must;

namespace GameFeelDescriptions
{
    //NOTE: this is not very useful right now, I think we need to do this differently, it was meant to be used in the case,
    //where you disable the renderer on the original object and make a visual copy that you can apply effects to without,
    //causing unintended gameplay impacts, such as rotating the "square ball" in a breakout, that causes collision normals
    //to rotate, and makes the angles wonky. 06/05/2020
//    public class FollowOriginEffect : GameFeelEffect
//    {
//        public FollowOriginEffect()
//        {
//            Description = "Make the Target follow the transform where the Effect originated. This Effect is meant to be Triggered continuously.";
//        }
//        
//        public bool UseGlobalPosition;
//
//        public GameFeelEasing.EaseType Easing;
//        
//        public float MaxDistance;
//
//        public float MinDistance = 0.01f;
//        
//        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
//            Vector3? interactionDirection = null)
//        {
//            var cp = new FollowOriginEffect{UseGlobalPosition = UseGlobalPosition, MinDistance = MinDistance, MaxDistance = MaxDistance, Easing = Easing};
//            cp.Init(origin, target, unscaledTime, interactionDirection);
//            return DeepCopy(cp);
//        }
//
//        private void SetValue(GameObject target, Vector3 value)
//        {
//            if (UseGlobalPosition)
//            {
//                target.transform.position = value;    
//            }
//            else
//            {
//                target.transform.localPosition = value;
//            }
//        }
//        
//        private Vector3 GetValue(GameObject target)
//        {
//            if (UseGlobalPosition)
//            {
//                return target.transform.position;    
//            }
//            else
//            {
//                return target.transform.localPosition;
//            }
//        }
//
//        protected override bool ExecuteTick()
//        {
//            //NOTE: this would work if we're only called when the position is updated, and we need to move there with this delay.
//            //TODO: make this work with a dynamically moving target (ie. get origin position at every update). 20/02/2020
//
//            if (target == null) return true;
//            
//            var originPos = GetValue(origin);
//            var targetPos = GetValue(target);
//
//            var distance = Vector3.Distance(originPos, targetPos);
//
//            //Avoid jitter by having a min distance.
//            if (distance < MinDistance) return false;
//            
//            if (distance > MaxDistance)
//            {
//                //If we're above max distance, lerp directly back to max distance.
//                SetValue(target,
//                    GameFeelTween.Interpolate(targetPos, 1f - (MaxDistance / distance), originPos,
//                        GameFeelEasing.Linear));
//            }
//            else
//            {
//                SetValue(target,
//                    GameFeelTween.Interpolate(targetPos, distance / MaxDistance, originPos,
//                        GameFeelEasing.Ease(Easing)));
//            }
//
//            return false;
//        }
//    }
}