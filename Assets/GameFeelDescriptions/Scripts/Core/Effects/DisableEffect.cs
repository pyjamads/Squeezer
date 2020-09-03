using System;
using System.Collections;
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
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new BlinkEffect
            {
                OnlyDisableRenderers = OnlyDisableRenderers,
            };
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {       
            if (target == null) return true;
            
            if(OnlyDisableRenderers == false)
            {
                target.SetActive(false);   
            }
            else
            {
                var renderers = target.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    if(renderer != null)
                    {
                        renderer.enabled = false;
                    }    
                }
            }
            
            return false;
        }
    }
    
    [Serializable]
    public class DisableEffect : GameFeelEffect
    {
        public DisableEffect()
        {
            Description = "Disable game object, unless onlyRenderer and/or onlyCollider is set, in which case it'll disable collisions and/or rendering.";
        }
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new DisableEffect();
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {       
            if (target == null) return true;
            
             target.SetActive(false);

             return false;
        }
    }
}