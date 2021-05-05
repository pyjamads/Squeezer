using UnityEngine;

namespace GameFeelDescriptions
{
    public class ContinuousRotationEffect : DurationalGameFeelEffect
    {
        public ContinuousRotationEffect()
        {
            Description = "Continuous Rotation Effect allows you to rotate an object continuously over a duration.";
        }
        
        [Tooltip("Use global rotation instead of local rotation.")]
        public bool useGlobalRotation;

        public bool RandomizeInitialRotation;
        public Vector3 RotationPerSecond;

        protected void SetValue(GameObject target, Vector3 value)
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

        protected Vector3 GetValue(GameObject target)
        {
            if (target == null) return Vector3.zero;
            
            if (useGlobalRotation)
            {
                return target.transform.eulerAngles;    
            } 
            
            return target.transform.localEulerAngles;
        }

        protected override void ExecuteSetup()
        {
            base.ExecuteSetup();

            if (RandomizeInitialRotation)
            {
                SetValue(target, GetValue(target) + RotationPerSecond * Random.value * 360f);
            }
        }

        protected override bool ExecuteTick()
        {
            if (target == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            SetValue(target, GetValue(target) + RotationPerSecond * (UnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));

            return false;
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new ContinuousRotationEffect();
            cp.useGlobalRotation = useGlobalRotation;
            cp.RotationPerSecond = RotationPerSecond;
            cp.RandomizeInitialRotation = RandomizeInitialRotation;
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }

        public override void Mutate(float amount = 0.05f)
        {
            if(RandomExtensions.Boolean(amount))
            {
                useGlobalRotation = !useGlobalRotation;
            }

            if (RandomExtensions.Boolean())
            {
                //Make a random rotation, and add/subtract a proportional amount here.
                var rndAmount = RandomExtensions.MutationAmount(amount);
                RotationPerSecond += Random.onUnitSphere * rndAmount * Mathf.Rad2Deg;    
            }

            if (RandomExtensions.Boolean(amount))
            {
                RandomizeInitialRotation = !RandomizeInitialRotation;
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
    }
}