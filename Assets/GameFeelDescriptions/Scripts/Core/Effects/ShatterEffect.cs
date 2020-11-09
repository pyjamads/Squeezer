using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class ShatterEffect : SpawningGameFeelEffect
    {
        public ShatterEffect()
        {
            Description = "Shatter an object into pieces and add a the Ragdoll effect to the pieces.";
            
            var ragdoll = new RagdollEffect();

            var destroy = new DestroyEffect();
            destroy.Delay = Random.Range(0.2f, 1.5f);
            destroy.RandomizeDelay = true;
            ragdoll.OnComplete(destroy);

            this.OnOffspring(ragdoll);
        }

        [Tooltip("Number of Pieces to spawn, if amount is different from the items in the list, they will be randomly chosen.")]
        public int AmountOfPieces = Random.Range(5, 10);

        [HideFieldIf("usePrimitivePieces", true)]
        [Tooltip("Add custom prefabs to spawn instead of using primitives.")]
        public List<GameObject> PrefabPieces;
        
        [Tooltip("Whether to use primitives or prefabs!")]
        public bool usePrimitivePieces = true;
        
        [HideFieldIf("usePrimitivePieces", false)]
        public PrimitiveType PiecePrimitive;
        
        //TODO: Add 2D option, and maybe a camera reference?, so we can make quad/sprite instead. 2020-08-13

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new ShatterEffect();

            cp.PrefabPieces = PrefabPieces;
            cp.AmountOfPieces = AmountOfPieces;
            cp.usePrimitivePieces = usePrimitivePieces;
            cp.PiecePrimitive = PiecePrimitive;
            
            cp.Init(origin, target, triggerData);

            cp.targetPos = target != null
                ? target.transform.position
                : (origin != null ? origin.transform.position : Vector3.zero);
            
            

            return DeepCopy(cp);
        }
        
        //TODO: add mutate!!!

        protected override bool ExecuteTick()
        {
            var shatteredPieces = new List<GameObject>();
            
            //If there's any prefabs instantiate them instead of copies of the target.
            if (!usePrimitivePieces && PrefabPieces?.Count > 0)
            {
                for (int i = 0; i < AmountOfPieces; i++)
                {
                    if (AmountOfPieces != PrefabPieces.Count)
                    {
                        shatteredPieces.Add(Object.Instantiate(PrefabPieces.GetRandomElement(), 
                            targetPos, Quaternion.identity, GameFeelEffectExecutor.Instance.transform));
                    }
                    else
                    {
                        shatteredPieces.Add(Object.Instantiate(PrefabPieces[i], 
                            targetPos, Quaternion.identity, GameFeelEffectExecutor.Instance.transform));
                    }
                }
            }
            else
            {
                //Scale the pieces, so approximate the same total size comes out (using the Cubic root: pieces^(1/3)).
                var qbrt = Mathf.Pow(AmountOfPieces, 1f / 3f);
                var scale = Vector3.one / qbrt;
                GameObject mold = null;

                if (!usePrimitivePieces)
                {
                    if (target != null)
                    {
                        Debug.LogWarning("ShatterEffect targeting ["+target.name+"] is set to use prefabs, but no prefabs have been added, using primitive instead.");    
                    }
                    else
                    {
                        Debug.LogWarning("ShatterEffect is set to use prefabs, but no prefabs have been added, using primitive instead.");
                    }
                }

                mold = GameObject.CreatePrimitive(PiecePrimitive);
                mold.transform.parent = GameFeelEffectExecutor.Instance.transform;
                mold.transform.position = targetPos;
                
                if (triggerData.InCollisionUpdate)
                {
                    GameObject.Destroy(mold.GetComponent<Collider>());
                }
                else
                {
                    GameObject.DestroyImmediate(mold.GetComponent<Collider>());
                }
                
                var renderer = mold.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                SetMaterialTransparentBlendMode(renderer.material); 
                renderer.material.color = Color.white;

                if (target != null)
                {
                    scale = target.transform.localScale / qbrt;

                    var targetRenderer = target.GetComponent<Renderer>();
                    if (targetRenderer.HasPropertyBlock() && !(targetRenderer is SpriteRenderer)) 
                    {
                        var materialPropertyBlock = new MaterialPropertyBlock();
                        targetRenderer.GetPropertyBlock(materialPropertyBlock);

                        renderer.SetPropertyBlock(materialPropertyBlock);
                    }
                    else
                    {
                        renderer.material.color = targetRenderer.material.color;
                    }
                }
                
                mold.transform.localScale = scale;

                shatteredPieces.Add(mold);

                //Copy the rest off of the first copy.
                for (var i = 1; i < AmountOfPieces; i++)
                {
                    var piece = Object.Instantiate(mold, GameFeelEffectExecutor.Instance.transform, true);

                    if (triggerData.InCollisionUpdate)
                    {
                        GameObject.Destroy(mold.GetComponent<Collider>());
                    }
                    
                    renderer = piece.GetComponent<Renderer>();
                    if (target != null)
                    {
                        var targetRenderer = target.GetComponent<Renderer>();
                        //NOTE: SpriteRenderer's will copy their texture as well, so we just do the material.color instead.
                        if (targetRenderer.HasPropertyBlock() && !(targetRenderer is SpriteRenderer))   
                        {
                            var materialPropertyBlock = new MaterialPropertyBlock();
                            targetRenderer.GetPropertyBlock(materialPropertyBlock);

                            renderer.SetPropertyBlock(materialPropertyBlock);
                        }
                        else
                        {
                            renderer.material.color = targetRenderer.material.color;
                        }
                    }

                    //Adjust position randomly based on the original position and the scale of the objects!
                    var pos = new Vector3(
                        targetPos.x + Random.Range(-scale.x, scale.x),  
                        targetPos.y + Random.Range(-scale.y, scale.y),
                        targetPos.z + Random.Range(-scale.z, scale.z));

                    piece.transform.position = pos;
                    
                    shatteredPieces.Add(piece);
                }
            }
            
            QueueOffspringEffects(shatteredPieces);
            
            //We're done!
            return true;
        }
    }
}