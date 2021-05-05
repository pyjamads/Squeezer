using System.Collections.Generic;
using System.Linq;
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
        
        protected override T DeepCopy<T>(T shallow, bool ignoreCooldown) 
        {
            //If the shallow is not SpawningGameFeelEffect, return null instead. 
            if (!(shallow is SpawningGameFeelEffect cp)) return null;

            if (ignoreCooldown)
            {
                cp.ExecuteOnOffspring = new List<GameFeelEffect>();
            }
            else
            {
                cp.ExecuteOnOffspring = new List<GameFeelEffect>(ExecuteOnOffspring);    
            }
            
                    
            return base.DeepCopy(cp as T, ignoreCooldown);

        }

        public override bool CompareTo(GameFeelEffect other)
        {
            return other is SpawningGameFeelEffect && base.CompareTo(other);
        }

        protected void QueueOffspringEffects(GameObject offspring, GameFeelTriggerData data = null)
        {
            var queuedEffects = new List<GameFeelEffect>();
            
            for (var i = 0; i < ExecuteOnOffspring.Count; i++)
            {
                //If the effect is disabled, skip it.
                if(ExecuteOnOffspring[i].Disabled) continue;
            
                var copy = ExecuteOnOffspring[i].CopyAndSetElapsed(origin, offspring, data ?? triggerData);
            
                if(copy == null) continue;
                    
                //Find previously active copy
                var previous = copy.CurrentActiveEffect();

                //Handle overlapping
                var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

                //Queue the effect
                if (queueCopy)
                {
                    if (copy is WaitForAbove waitForAboveEffect)
                    {
                        //NOTE: there's only one offspring in this case, so it'll always queue the wait.
                        //if (waitForAboveEffect.WaitForAllTargets && inner != offspring.Count - 1) continue;
                        
                        waitForAboveEffect.WaitFor(queuedEffects.ToList());
                        GameFeelEffectExecutor.Instance.QueueEffect(waitForAboveEffect);
                        queuedEffects.Add(waitForAboveEffect);
                    }
                    else
                    {
                        GameFeelEffectExecutor.Instance.QueueEffect(copy);
                        queuedEffects.Add(copy);    
                    }
                }
            }
        }
        
        protected void QueueOffspringEffects(List<GameObject> offspring, GameFeelTriggerData data = null)
        {
            var queuedEffects = new List<GameFeelEffect>();

            for (var outer = 0; outer < ExecuteOnOffspring.Count; outer++)
            {
                //If the effect is disabled, skip it.
                if(ExecuteOnOffspring[outer].Disabled) continue;

                for (var inner = 0; inner < offspring.Count; inner++)
                {
                    var copy = ExecuteOnOffspring[outer].CopyAndSetElapsed(origin, offspring[inner], data ?? triggerData);

                    if (copy == null) continue;

                    //Find previously active copy
                    var previous = copy.CurrentActiveEffect();

                    //Handle overlapping
                    var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

                    //Queue the effect
                    if (queueCopy)
                    {
                        if (copy is WaitForAbove waitForAboveEffect)
                        {
                            //For multiple targets, only queue the wait once!
                            if (waitForAboveEffect.WaitForAllTargets && inner != offspring.Count - 1) continue;
                            
                            waitForAboveEffect.WaitFor(queuedEffects.Where(item => waitForAboveEffect.WaitForAllTargets || item.target == offspring[inner]));
                            GameFeelEffectExecutor.Instance.QueueEffect(waitForAboveEffect);
                            queuedEffects.Add(waitForAboveEffect);
                        }
                        else
                        {
                            GameFeelEffectExecutor.Instance.QueueEffect(copy);
                            queuedEffects.Add(copy);
                        }
                    }
                }
            }
            
            
            //    QueueOffspringEffects(offspring[index], queuedEffects, (index == offspring.Count - 1));
            //}
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
                    if (triggerData.InCollisionUpdate)
                    {
                        Object.Destroy(script);   
                    }
                    else
                    {
                        Object.DestroyImmediate(script);    
                    }
                }    
            }

            if (stripRigidbodies)
            {
                var rigids = targetCopy.GetComponentsInChildren<Rigidbody>();
                foreach (var rigid in rigids)
                {
                    if (triggerData.InCollisionUpdate)
                    {
                        Object.Destroy(rigid);   
                    }
                    else
                    {
                        Object.DestroyImmediate(rigid);    
                    }
                }
                
                var rigid2Ds = targetCopy.GetComponentsInChildren<Rigidbody2D>();
                foreach (var rigid2D in rigid2Ds)
                {
                    if (triggerData.InCollisionUpdate)
                    {
                        Object.Destroy(rigid2D);   
                    }
                    else
                    {
                        Object.DestroyImmediate(rigid2D);    
                    }    
                }
            }

            if (stripColliders)
            {
                var cols = targetCopy.GetComponentsInChildren<Collider>();
                foreach (var col in cols)
                {
                    if (triggerData.InCollisionUpdate)
                    {
                        Object.Destroy(col);   
                    }
                    else
                    {
                        Object.DestroyImmediate(col);    
                    } 
                }

                var col2Ds = targetCopy.GetComponentsInChildren<Collider2D>();
                foreach (var col2D in col2Ds)
                {
                    if (triggerData.InCollisionUpdate)
                    {
                        Object.Destroy(col2D);   
                    }
                    else
                    {
                        Object.DestroyImmediate(col2D);    
                    } 
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
        
        
        // public override float SubtreeCost(GameFeelEffect other)
        // {
        //     //First let's figure out the difference in child count.
        //     float distanceCost = Mathf.Abs(other.ExecuteAfterCompletion.Count - ExecuteAfterCompletion.Count);
        //
        //     var treeCost = new float[ExecuteAfterCompletion.Count, other.ExecuteAfterCompletion.Count];
        //
        //     //Get the distance costs of each child combination.
        //     for (var i = 0; i < ExecuteAfterCompletion.Count; i++)
        //     {
        //         var minValue = float.MaxValue;
        //     
        //         for (int j = 0; j < other.ExecuteAfterCompletion.Count; j++)
        //         {
        //             treeCost[i,j] = ExecuteAfterCompletion[i].DistanceCost(other.ExecuteAfterCompletion[j]);
        //             if (treeCost[i, j] < minValue)
        //             {
        //                 minValue = treeCost[i, j];
        //             }
        //         }
        //     
        //         //Then for each child find the lowest replacementCost (NOTE: can be 0, if they match exactly)
        //         //TODO: this does not take into account, that multiple i's could select a single j. please fix 2020-11-27
        //         distanceCost += minValue;
        //     }
        //
        //     if (other is SpawningGameFeelEffect spawner)
        //     {
        //         var treeCost2 = new float[ExecuteOnOffspring.Count, spawner.ExecuteOnOffspring.Count];
        //         
        //         
        //     }
        //     
        //     return distanceCost;
        // }
    }
}