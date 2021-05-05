using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class BlinkEffect : DurationalGameFeelEffect
    {
        public BlinkEffect()
        {
            Description = "Disable game object, unless onlyRenderer and/or onlyCollider is set, in which case it'll disable collisions and/or rendering.";
        }
        
        public bool OnlyDisableRenderers = true;

        public float Interval = 0.02f;

        private float lastBlinkTime;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new BlinkEffect
            {
                OnlyDisableRenderers = OnlyDisableRenderers,
                Interval = Interval,
            };
            cp.Init(origin, target, triggerData);
            return DeepCopy(cp, ignoreCooldown);
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean())
            {
                OnlyDisableRenderers = !OnlyDisableRenderers;
            }
            
            if (RandomExtensions.Boolean())
            {
                Interval = Mathf.Max(0.01f, Interval + RandomExtensions.MutationAmount(amount));
            }

            base.Mutate(amount);
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
            
            var currentTime = (UnscaledTime ? Time.unscaledTime : Time.time);
            if (currentTime < lastBlinkTime + Interval) return false;

            lastBlinkTime = currentTime;
            
            if(OnlyDisableRenderers == false)
            {
                target.SetActive(!target.activeSelf);
            }
            else
            {
                var renderers = target.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if(renderer != null)
                    {
                        renderer.enabled = !renderer.enabled;
                    }    
                }
            }
            
            return false;
        }

        public override void ExecuteCleanUp()
        {
            if (target == null) return;
            
            if(OnlyDisableRenderers == false)
            {
                target.SetActive(true);
            }
            else
            {
                var renderers = target.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if(renderer != null)
                    {
                        renderer.enabled = true;
                    }    
                }
            }
            
            base.ExecuteCleanUp();
        }
    }
}