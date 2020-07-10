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
    
    public class ShatterEffect : SpawningGameFeelEffect //TODO: use SpawningGameFeelEffect instead, and fix effect trees!
    {
        public ShatterEffect()
        {
            Description = "Shatter an object into pieces and add a the Ragdoll effect to the pieces.";
            
            var ragdoll = new RagdollEffect();
            if (ragdoll.ExecuteOnOffspring.Count > 0)
            {
                ragdoll.ExecuteOnOffspring[0].RandomizeDelay = true;    
            }
            else
            {
                var destroy = new DestroyEffect();
                destroy.Delay = Random.Range(0.2f, 1.5f);
                destroy.RandomizeDelay = true;
                ragdoll.ExecuteOnOffspring.Add(destroy);
            }

            ExecuteOnOffspring = new List<GameFeelEffect>
            {
                ragdoll //destroy is queued on ragdoll.OnComplete
            };
        }

        [Tooltip("Number of Pieces to spawn, if amount is different from the items in the list, they will be randomly chosen.")]
        public int AmountOfPieces = Random.Range(5, 10);
        
        [Tooltip("Add custom prefabs to spawn instead of the copies of the target.")]
        public List<GameObject> PrefabPieces;
        
        // [Tooltip("Initial velocity force multiplier (Can be negative)")]
        // public float ForceMultiplier = 1f;
        //
        // [Tooltip("Force to add in addition to the collision force.")]
        // public Vector3 AdditionalForce = Vector3.one;
        //
        // [Tooltip("Apply a randomization of the Additional force vector.")]
        // public bool RandomizeAdditionalForce = true;
        //
        // public bool ApplyGravity = true;
        //
        // [Tooltip("Destroy the pieces after a randomized delay?")]
        // public bool DestroyPieces = true;
        //
        // [HideFieldIf("DestroyPieces", false)]
        // public float DestroyDelay = Random.Range(0.2f, 1.5f);

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new ShatterEffect();

            // cp.PrefabPieces = PrefabPieces;
            // cp.AmountOfPieces = AmountOfPieces;
            // cp.ApplyGravity = ApplyGravity;
            // cp.ForceMultiplier = ForceMultiplier;
            // cp.RandomizeAdditionalForce = RandomizeAdditionalForce;
            // cp.AdditionalForce = AdditionalForce;
            // cp.DestroyPieces = DestroyPieces;
            // cp.DestroyDelay = DestroyDelay;
            cp.Init(origin, target, unscaledTime, interactionDirection);

            cp.targetPos = target != null ? target.transform.position : origin.transform.position;

            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            var shatteredPieces = new List<GameObject>();
            
            //If there's any prefabs instantiate them instead of copies of the target.
            if (PrefabPieces?.Count > 0)
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
                if (target == null) return true;
                
                //Get a copy and remove all scripts, rigidbodies and colliders.
                var targetCopy = Object.Instantiate(target, GameFeelEffectExecutor.Instance.transform, true);
                targetCopy.tag = "Untagged";
                
                var scripts = targetCopy.GetComponentsInChildren<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    Object.Destroy(script);
                }

                //NOTE: the ragdoll effect adds these again back, unless they already exist.
//                var rigid = targetCopy.GetComponent<Rigidbody>();
//                Object.Destroy(rigid);
//                
//                var rigid2D = targetCopy.GetComponent<Rigidbody2D>();
//                Object.Destroy(rigid2D);
//                
                var col = targetCopy.GetComponent<Collider>();
                Object.Destroy(col);
                
                var col2D = targetCopy.GetComponent<Collider2D>();
                Object.Destroy(col2D);
                
                //Scale the pieces, so approximate the same total size comes out (using the Cubic root: pieces^(1/3)).
                var qbrt = Mathf.Pow(AmountOfPieces, 1f / 3f);
                var scale = target.transform.localScale / qbrt;
                targetCopy.transform.localScale = scale;
                
                shatteredPieces.Add(targetCopy);

                //Copy the rest off of the first copy.
                for (var i = 1; i < AmountOfPieces; i++)
                {
                    var piece = Object.Instantiate(targetCopy, GameFeelEffectExecutor.Instance.transform, true);

                    //Adjust position randomly based on the original position and the scale of the objects!
                    var pos = new Vector3(
                        targetPos.x + Random.Range(-scale.x, scale.x),  
                        targetPos.y + Random.Range(-scale.y, scale.y),
                        targetPos.z + Random.Range(-scale.z, scale.z));

                    piece.transform.position = pos;
                    
                    shatteredPieces.Add(piece);
                }
            }
            
            //NOTE: Doing it manually, might remove a 1 frame delay.
//            if (!target.GetComponent<Rigidbody>() && !target.GetComponent<Rigidbody2D>())
//            {
//                var rigid = target.AddComponent<Rigidbody>();
//
//                rigid.useGravity = true;
//                rigid.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);                
//            }
//            else
//            {
//                var body = target.GetComponent<Rigidbody>();
//                if (body)
//                {
//                    body.useGravity = true;
//                    body.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);
//                }
//                else
//                {
//                    var body2D = target.GetComponent<Rigidbody2D>();
//                    body2D.gravityScale = 1;
//                    body2D.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);
//                }
//            }

            // for (int i = 0; i < shatteredPieces.Count; i++)
            // {
            //     var ragdoll = new RagdollEffect
            //     {
            //         ForceMultiplier = ForceMultiplier, 
            //         ApplyGravity = ApplyGravity,
            //         AdditionalForce = RandomizeAdditionalForce ? 
            //             AdditionalForce.multiplyElements(Random.onUnitSphere) : AdditionalForce,
            //         DestroyRagdoll = false, //We're adding a custom destroy effect that randomizes the delay. 
            //     };
            //     
            //     if (DestroyPieces)
            //     {
            //         var destroy = new DestroyEffect();
            //         destroy.Delay = DestroyDelay;
            //         destroy.RandomizeDelay = true;
            //         ragdoll.OnComplete(destroy);    
            //     }
            //
            //     //Add whichever followup effects defined on this to the ragdoll.
            //     ragdoll.OnComplete(ExecuteAfterCompletion);
            //     
            //     ragdoll.Init(origin, shatteredPieces[i], unscaledTime, interactionDirection);
            //     ragdoll.SetElapsed();
            //     
            //     ragdoll.QueueExecution();
            // }
            
            QueueOffspringEffects(shatteredPieces);
            
            return true;
        }
    }
}