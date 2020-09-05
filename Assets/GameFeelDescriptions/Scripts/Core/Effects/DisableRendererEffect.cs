using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class DisableRendererEffect : DurationalGameFeelEffect
    {
        public DisableRendererEffect()
        {
            Description = "Disable the renderer on the game object, optionally enable it again after the duration.";
        }

        public bool includingChildren;
        public bool enableAfterDuration;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            var cp = new DisableRendererEffect();
            cp.includingChildren = includingChildren;
            cp.enableAfterDuration = enableAfterDuration;
            cp.Init(origin, target, interactionDirection);
            return DeepCopy(cp);
        }

        protected override void ExecuteSetup()
        {   
            if (target == null) return;

            if (includingChildren)
            {
                var renderers = target.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0) return;

                for (int i = 0; i < renderers.Length; i++)
                {
                    renderers[i].enabled = false;    
                }
            }
            else
            {
                var render = target.GetComponent<Renderer>();
                if (render == null) return;
                
                render.enabled = false;
            }
        }

        protected override bool ExecuteTick()
        {
            //DoNothing
            return false;
        }

        protected override void ExecuteComplete()
        {
            if (enableAfterDuration)
            {
                if (includingChildren)
                {
                    var renderers = target.GetComponentsInChildren<Renderer>();
                    if (renderers.Length == 0) return;

                    for (int i = 0; i < renderers.Length; i++)
                    {
                        renderers[i].enabled = true;    
                    }
                }
                else
                {
                    var render = target.GetComponent<Renderer>();
                    if (render == null) return;
                
                    render.enabled = true;
                }  
            }    

            base.ExecuteComplete();
        }
    }
}