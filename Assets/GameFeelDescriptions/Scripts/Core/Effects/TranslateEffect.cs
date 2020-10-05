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
            GameFeelTriggerData triggerData)
        {
            var cp = new TranslateEffect
            {
                useGlobalPosition = useGlobalPosition, 
                useInteractionDirection = useInteractionDirection,
                interactionDirectionMultiplier = interactionDirectionMultiplier,
            };
            
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp);
        }
        
        public override void Randomize()
        {
            base.Randomize();
            
            setFromValue = false;

            useGlobalPosition = RandomExtensions.Boolean(0.25f);
            useInteractionDirection = RandomExtensions.Boolean(0.25f);
            interactionDirectionMultiplier = Random.Range(0f, 5f);

            @from = Vector3.zero;

            relative = true;
            
            //Just point somewhere! and scale it between 0-5x
            to = Random.onUnitSphere * Random.Range(0f, 5f);
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
            
            if(RandomExtensions.Boolean(amount))
            {
                var multiplierAmount = Random.value * amount * 2 - amount;
                interactionDirectionMultiplier += multiplierAmount;
            }

            //Make a random color, and add/subtract a proportional amount here.
            var rndAmount = Random.value * amount * 2 - amount;
            @from += Random.onUnitSphere * rndAmount;
        
            //Make a random color, and add/subtract a proportional amount here.
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
                repeat = Random.Range(-1, 3);
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
            base.ExecuteSetup();

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
        }

        protected override bool TickTween()
        {
            if (target == null) return true;
            
            SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, GetEaseFunc()));

            return false;
        }
    }
}