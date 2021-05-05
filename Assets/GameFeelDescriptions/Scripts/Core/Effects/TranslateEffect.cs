using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
    //TODO: Make a customProperty drawer that can handle these, so it can be enabled!

    public class TranslateEffect : TweenEffect<Vector3>
    {
        [Tooltip("Use global position instead of local position.")]
        public bool useGlobalPosition;

        public bool useInteractionDirection;
        public float interactionDirectionMultiplier = 1f;
        
        public TranslateEffect()
        {
            Description = "Translate Effect allows you to move an object using easing.";
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new TranslateEffect
            {
                useGlobalPosition = useGlobalPosition, 
                useInteractionDirection = useInteractionDirection,
                interactionDirectionMultiplier = interactionDirectionMultiplier,
            };
            
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean(amount))
            {
                setFromValue = !setFromValue;
            }
            
            if(RandomExtensions.Boolean(amount))
            {
                useGlobalPosition = !useGlobalPosition;
            }
            
            if(RandomExtensions.Boolean(amount))
            {
                useInteractionDirection = !useInteractionDirection;
            }
            
            if(RandomExtensions.Boolean())
            {
                var multiplierAmount = Random.value * amount * 2 - amount;
                interactionDirectionMultiplier += multiplierAmount;
            }

            //Make a random vector, and add/subtract a proportional amount here.
            var rndAmount = Random.value * amount * 2 - amount;
            @from += Random.onUnitSphere * rndAmount;
        
            //Make a random vector, and add/subtract a proportional amount here.
            rndAmount = Random.value * amount * 2 - amount;
            to += Random.onUnitSphere * rndAmount;

            if (RandomExtensions.Boolean(amount))
            {
                easing = EnumExtensions.GetRandomValue(except: new List<EasingHelper.EaseType>{EasingHelper.EaseType.Curve});
            }

            if (RandomExtensions.Boolean(amount))
            {
                loopType = EnumExtensions.GetRandomValue<LoopType>();
            }

            if (RandomExtensions.Boolean(amount))
            {
                //Don't allow infinite looping
                repeat = Random.Range(0, 3);
            }

            base.Mutate(amount);
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

        protected override void ExecuteSetup()
        {
            if (useInteractionDirection)
            {
                var interactionDirection = Vector3.zero;

                switch (triggerData)
                {
                    case CollisionData collisionEvent:
                        interactionDirection = collisionEvent.GetInteractionDirection();
                        break;
                    case PositionalData positionalEvent:
                        interactionDirection = positionalEvent.DirectionDelta;
                        break;
                }

                end += interactionDirection * interactionDirectionMultiplier;
            }
            
            base.ExecuteSetup();
        }

        protected override bool TickTween()
        {
            if (target == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }

            var easeFunc = GetEaseFunc();
            if (relative)
            {
                //TODO: clean up and expand this to all the tweens. 2021-03-07
                var progress = elapsed / Duration;
                var prevProgress = oldElapsed / Duration;

                if (reverse)
                {
                    progress = 1 - progress;
                    prevProgress = 1 - prevProgress;
                }

                if (true) // alternative calculation, that works for discrete values as well!
                {
                    var prev = diffAmount * easeFunc.Invoke(prevProgress);
                    var current = diffAmount * easeFunc.Invoke(progress);
                    
                    //amount = end - start;
                    //current + (amount * easing(t1)) - (amount * - easing(t0));
                    SetValue(target, GetValue(target) + (reverse ? -1 : 1) * (current - prev));
                }
                else
                {
                    var easingDelta = easeFunc.Invoke(reverse ? 1 - elapsed / Duration : elapsed / Duration) 
                                      - easeFunc.Invoke(reverse ? 1 - oldElapsed / Duration : oldElapsed / Duration);
                
                    //current + (to - @from) * (easing(t1) - easing(t0));
                    SetValue(target, GetValue(target) + diffAmount * ((reverse ? -1 : 1) * easingDelta));
                }
            }
            else
            {
                //@from  + (to - @from) * easing(t);
                SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, easeFunc));    
            }

            return false;
        }
    }
}