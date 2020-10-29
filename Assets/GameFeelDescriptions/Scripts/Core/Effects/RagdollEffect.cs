using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class RagdollEffect : GameFeelEffect
    {
        public RagdollEffect()
        {
            Description = "Add a Rigidbody that's affected by gravity to the target, and set an initial velocity based on the interaction.";
            
            // //NOTE 2020-07-09: this should be handled in the ExecuteOnOffspring list,
            // //and maybe we should even handle the addition of forces and toggling of gravity the same way.   
            //
            // var destroy = new DestroyEffect();
            // destroy.Delay = Random.Range(0.2f, 1.5f);
            //
            // ExecuteOnOffspring = new List<GameFeelEffect>
            // {
            //     destroy
            // };
        }
        
        // [Tooltip("Add a custom prefab to spawn instead of changing the target.")]
        // public GameObject RagdollPrefab;

        [Tooltip("Initial velocity force multiplier (Can be negative)")]
        public float ForceMultiplier = Random.Range(1f, 5f);
        
        [Tooltip("Apply a randomization of the Additional force vector.")]
        public bool RandomizeAdditionalForce = false;
        
        public Vector3 AdditionalForce;

        public float MaxVelocity = 10f;

        public bool ApplyGravity = true;

        // [Tooltip("Destroy the ragdoll after a delay?")]
        // public bool DestroyRagdoll = true;
        //
        // [HideFieldIf("DestroyRagdoll", false)]
        // public float DestroyDelay = Random.Range(0.2f, 1.5f);
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new RagdollEffect();

            // cp.RagdollPrefab = RagdollPrefab;
            cp.ForceMultiplier = ForceMultiplier;
            cp.AdditionalForce = AdditionalForce;
            cp.MaxVelocity = MaxVelocity;
            cp.ApplyGravity = ApplyGravity;
            cp.RandomizeAdditionalForce = RandomizeAdditionalForce;
            // cp.DestroyRagdoll = DestroyRagdoll;
            // cp.DestroyDelay = DestroyDelay;
            cp.Init(origin, target, triggerData);
            
            if (target == null && origin == null) return null;
            
            // cp.targetPos = target != null ? target.transform.position : origin.transform.position;
            
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            // if (target == null && RagdollPrefab == null) return true;
            if (target == null) return true;
            
            var interactionDirection = Vector3.zero;

            switch (triggerData)
            {
                case CollisionData collisionEvent:
                    interactionDirection = collisionEvent.GetInteractionDirection();
                    break;
                case PositionalData positionalEvent:
                    interactionDirection = positionalEvent.DirectionDelta;
                    break;
            }
            
            // var ragdoll = target;
            //If there's a ragdoll prefab, Destroy the passed "target" and instantiate the ragdoll instead.
            // if (RagdollPrefab != null)
            // {
            //     ragdoll = Object.Instantiate(RagdollPrefab, targetPos, Quaternion.identity, GameFeelEffectExecutor.Instance.transform);
            // }

            var additionalForce = AdditionalForce;
            if (RandomizeAdditionalForce)
            {
                additionalForce = AdditionalForce.multiplyElements(Random.onUnitSphere);
            }
            
            if (!target.GetComponent<Rigidbody>() && !target.GetComponent<Rigidbody2D>())
            {
                
                if (target.GetComponent<Collider2D>())
                {
                    var rigid = target.AddComponent<Rigidbody2D>();
                    //TODO: saw a null ref here, when not 'copying' the object, and it had a Rigidbody2D attached...
                    rigid.simulated = true;
                    rigid.isKinematic = false;
                    rigid.bodyType = RigidbodyType2D.Dynamic;
                    rigid.gravityScale = ApplyGravity ? 1f : 0f;
                    rigid.velocity = additionalForce + interactionDirection * ForceMultiplier;     
                    if (rigid.velocity.magnitude > MaxVelocity)
                    {
                        rigid.velocity = rigid.velocity.normalized * MaxVelocity;
                    }
                }
                else
                {
                    var rigid = target.AddComponent<Rigidbody>();
                    //TODO: saw a null ref here, when not 'copying' the object, and it had a Rigidbody2D attached...
                    rigid.useGravity = ApplyGravity;
                    rigid.isKinematic = false;
                    rigid.velocity = additionalForce + interactionDirection * ForceMultiplier;     
                    if (rigid.velocity.magnitude > MaxVelocity)
                    {
                        rigid.velocity = rigid.velocity.normalized * MaxVelocity;
                    }
                }
            }
            else
            {
                var body = target.GetComponent<Rigidbody>();
                if (body)
                {
                    body.useGravity = ApplyGravity;
                    body.isKinematic = false;
                    body.velocity = additionalForce + interactionDirection * ForceMultiplier;
                    if (body.velocity.magnitude > MaxVelocity)
                    {
                        body.velocity = body.velocity.normalized * MaxVelocity;
                    }
                }
                else
                {
                    var body2D = target.GetComponent<Rigidbody2D>();
                    body2D.isKinematic = false;
                    body2D.bodyType = RigidbodyType2D.Dynamic;
                    body2D.gravityScale = ApplyGravity ? 1f : 0f;
                    body2D.velocity = additionalForce + interactionDirection * ForceMultiplier;
                    if (body2D.velocity.magnitude > MaxVelocity)
                    {
                        body2D.velocity = body2D.velocity.normalized * MaxVelocity;
                    }
                }
            }

            // //NOTE 2020-07-09: this should be handled in the ExecuteOnOffspring list,
            // //and maybe we should even handle the addition of forces and toggling of gravity the same way.   
            // if (DestroyRagdoll)
            // {
            //     var destroy = new DestroyEffect();
            //     destroy.Delay = DestroyDelay;
            //     this.OnComplete(destroy);
            // }
            
            // QueueOffspringEffects(ragdoll);
            
            //We're done!
            return true;
        }
    }
}