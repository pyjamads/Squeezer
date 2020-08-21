using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public abstract class SpawningGameFeelEffect : GameFeelEffect
    {
        
        [Header("A list of effects to be executed on the objects spawned by this effect!")]
        [SerializeReference]
        [ShowType]
        public List<GameFeelEffect> ExecuteOnOffspring = new List<GameFeelEffect>();

        protected Vector3 targetPos;
        
        protected override T DeepCopy<T>(T shallow) 
        {
            if (shallow is SpawningGameFeelEffect cp)
            {
                cp.ExecuteOnOffspring = new List<GameFeelEffect>(ExecuteOnOffspring);
                    
                return base.DeepCopy(cp as T);
            }
            
            return base.DeepCopy(shallow);
        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is SpawningGameFeelEffect && base.CompareTo(other);
        }

        protected void QueueOffspringEffects(GameObject offspring)
        {
            for (var i = 0; i < ExecuteOnOffspring.Count; i++)
            {
                //If the effect is disabled, skip it.
                if(ExecuteOnOffspring[i].Disabled) continue;
            
                var copy = ExecuteOnOffspring[i].CopyAndSetElapsed(origin, offspring, unscaledTime);
            
                if(copy == null) continue;
                    
                //Find previously active copy
                var previous = copy.CurrentActiveEffect();

                //Handle overlapping
                var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

                //Queue the effect
                if (queueCopy)
                {
                    copy.QueueExecution(forceQueue: false);   
                }
            }
        }
        
        protected void QueueOffspringEffects(List<GameObject> offspring)
        {
            foreach (var obj in offspring)
            {
                QueueOffspringEffects(obj);
            }
        }

        protected GameObject CopyAndStripTarget(GameObject target, bool stripRigidbodies = true, bool stripColliders = true, bool stripScripts = true)
        {
            if (target == null) return null;
                
            //Get a copy and remove all scripts, rigidbodies and colliders.
            var targetCopy = Object.Instantiate(target, GameFeelEffectExecutor.Instance.transform, true);
            targetCopy.tag = "Untagged";

            if (stripScripts)
            {
                var scripts = targetCopy.GetComponentsInChildren<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    Object.DestroyImmediate(script);
                }    
            }

            if (stripRigidbodies)
            {
                var rigids = targetCopy.GetComponentsInChildren<Rigidbody>();
                foreach (var rigid in rigids)
                {
                    Object.DestroyImmediate(rigid);
                }
                
                var rigid2Ds = targetCopy.GetComponentsInChildren<Rigidbody2D>();
                foreach (var rigid2D in rigid2Ds)
                {
                    Object.DestroyImmediate(rigid2D);    
                }
            }

            if (stripColliders)
            {
                var cols = targetCopy.GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    Object.DestroyImmediate(col);
                }

                var col2Ds = targetCopy.GetComponentsInChildren<Collider2D>();
                foreach (var col2D in col2Ds)
                {
                    Object.DestroyImmediate(col2D);
                }
            }
            return targetCopy;
        }

        protected void SetMaterialTransparentBlendMode(Material material)
        {
            // case BlendMode.Fade:
             material.SetFloat("_Mode", 2);//SetOverrideTag("RenderType", "Transparent");
             material.SetOverrideTag("RenderType", "Transparent");
             material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
             material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
             material.SetInt("_ZWrite", 0);
             material.DisableKeyword("_ALPHATEST_ON");
             material.EnableKeyword("_ALPHABLEND_ON");
             material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
             material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
             // break;
            // case BlendMode.Transparent:
            // material.SetFloat("_Mode", 3);//SetOverrideTag("RenderType", "Transparent");
            // material.SetOverrideTag("RenderType", "Transparent");
            // material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            // material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // material.SetInt("_ZWrite", 0);
            // material.DisableKeyword("_ALPHATEST_ON");
            // material.DisableKeyword("_ALPHABLEND_ON");
            // material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            // material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            // break;
        }
    }
}