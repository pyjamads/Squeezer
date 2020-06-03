using System;
using System.Collections;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class DisableEffect : GameFeelEffect
    {
        public DisableEffect()
        {
            Description = "Disable game object, unless onlyRenderer and/or onlyCollider is set, in which case it'll disable collisions and/or rendering.";
        }

        public bool onlyCollider;
        public bool onlyRenderer;
      
        //TODO: maybe Remove this effect, replace all logic with copies of the thing that the effect happened to. 11/02/2020

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new DisableEffect{onlyCollider = onlyCollider, onlyRenderer = onlyRenderer};
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {       
            if (target == null) return true;
            
            if (onlyCollider)
            {
                var col = target.GetComponent<Collider>();
                if (col != null)
                {
                    col.enabled = false;
                }
                else
                {
                    var col2D = target.GetComponent<Collider2D>(); 
                    if (col2D != null) col2D.enabled = false;    
                }
            }
            
            if (onlyRenderer)
            {
                var render = target.GetComponent<Renderer>();
                if (render != null)
                {
                    render.enabled = false;
                }
            }
            
            if(!onlyCollider && !onlyRenderer)
            {
                target.SetActive(false);   
            }

            return false;
        }
    }
}