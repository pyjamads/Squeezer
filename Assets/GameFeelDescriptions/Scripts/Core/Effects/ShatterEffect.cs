using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    // public class ParticlePoofEffect : SpawningGameFeelEffect
    // {
    //     public ParticlePoofEffect()
    //     {
    //         Description = "Particle poof creates anything from a dust cloud from a footfall to nuclear explosion.";
    //     }
    //     
    //     [Tooltip("Number of Pieces to spawn, if amount is different from the items in the list, they will be randomly chosen.")]
    //     public int AmountOfParticles = Random.Range(5, 10);
    //     
    //     [Tooltip("Add custom prefabs to spawn instead of the copies of the target.")]
    //     public List<GameObject> ParticlePrefabs;
    //
    //     [Range(1, 10)]
    //     public int Magnitude = Random.Range(1, 11);
    //
    //     // public override void Randomize()
    //     // {
    //     //     base.Randomize();
    //     //
    //     //     AmountOfParticles = Random.Range(5, 50);
    //     //     ParticlePrefabs = null; // or have some resources loaded here!
    //     //     Magnitude = Random.Range(1, 11);
    //     //
    //     //     GenerateParticlePoof(); //Make the effect list, and add it to ExecuteOnOffspring!!
    //     // }
    //
    //     // public override void Mutate(float amount = 0.05f)
    //     // {
    //     //     base.Mutate(amount);
    //     //     
    //     //     AmountOfParticles += Mathf.RoundToInt(AmountOfParticles * amount * RandomExtensions.Sign());
    //     //     
    //     //     //TODO: mutate every effect in the ExecuteOnOffspring list
    //     // }
    //     
    //     public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
    //         Vector3? interactionDirection = null)
    //     {
    //         var cp = new ParticlePoofEffect();
    //
    //         cp.ParticlePrefabs = ParticlePrefabs;
    //         cp.AmountOfParticles = AmountOfParticles;
    //         cp.Magnitude = Magnitude;
    //         cp.Init(origin, target, unscaledTime, interactionDirection);
    //         
    //         return DeepCopy(cp);
    //     }
    //
    //     protected override bool ExecuteTick()
    //     {
    //         throw new System.NotImplementedException();
    //         //TODO: create "particles"
    //         //TODO: Add effect list to particles, and queue the effects...
    //     }
    // }
    
    public class ShatterEffect : SpawningGameFeelEffect
    {
        public ShatterEffect()
        {
            Description = "Shatter an object into pieces and add a the Ragdoll effect to the pieces.";
            
            var ragdoll = new RagdollEffect();
            if (ragdoll.ExecuteOnOffspring.Count > 0)
            {
                ragdoll.ExecuteOnOffspring.Clear();    
            }
            
            var destroy = new DestroyEffect();
            destroy.Delay = Random.Range(0.2f, 1.5f);
            destroy.RandomizeDelay = true;
            ragdoll.ExecuteOnOffspring.Add(destroy);

            this.OnOffspring(ragdoll);
        }

        [Tooltip("Number of Pieces to spawn, if amount is different from the items in the list, they will be randomly chosen.")]
        public int AmountOfPieces = Random.Range(5, 10);

        [Tooltip("Add custom prefabs to spawn instead of the copies of the target.")]
        public List<GameObject> PrefabPieces;
        
        public bool usePrimitivePieces;
        
        [HideFieldIf("usePrimitivePieces", false)]
        public PrimitiveType PiecePrimitive;
        
        //TODO: Add 2D option, and maybe a camera reference?, so we can make quad/sprite instead. 2020-08-13

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            var cp = new ShatterEffect();

            cp.PrefabPieces = PrefabPieces;
            cp.AmountOfPieces = AmountOfPieces;
            cp.usePrimitivePieces = usePrimitivePieces;
            cp.PiecePrimitive = PiecePrimitive;
            
            cp.Init(origin, target, interactionDirection);

            cp.targetPos = target != null ? target.transform.position : origin.transform.position;

            return DeepCopy(cp);
        }

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
                
                if (usePrimitivePieces)
                {    
                    mold = GameObject.CreatePrimitive(PiecePrimitive);
                    mold.transform.parent = GameFeelEffectExecutor.Instance.transform;
                    mold.transform.position = targetPos;
                    var renderer = mold.GetComponent<Renderer>();
                    renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                    SetMaterialTransparentBlendMode(renderer.material); 
                    renderer.material.color = Color.white;

                    if (target != null)
                    {
                        scale = target.transform.localScale / qbrt;

                        var targetRenderer = target.GetComponent<Renderer>();
                        if (targetRenderer.HasPropertyBlock())
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
                }
                else if (target != null)
                {
                    //Get a copy and remove all scripts, rigidbodies and colliders.
                    mold = CopyAndStripTarget(target);

                    scale = target.transform.localScale / qbrt;
                    mold.transform.localScale = scale;
                }

                if (mold == null) return false;
                
                shatteredPieces.Add(mold);

                //Copy the rest off of the first copy.
                for (var i = 1; i < AmountOfPieces; i++)
                {
                    var piece = Object.Instantiate(mold, GameFeelEffectExecutor.Instance.transform, true);

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