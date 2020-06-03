using UnityEngine;

namespace GameFeelDescriptions
{
    //NOTE: Ragdoll effect can change the target (to the specified Prefab),
    //which means all effects need to be added as an OnComplete to the Ragdoll,
    //in order to apply to the "new" target!
    
    public class RagdollEffect : GameFeelEffect
    {
        public RagdollEffect()
        {
            Description = "Add a Rigidbody that's affected by gravity to the target, and set an initial velocity based on the interaction.";
        }
        
        [Tooltip("Add a custom prefab to spawn instead of the target (warning: this will call Destroy(target)).")]
        public GameObject RagdollPrefab;

        [Tooltip("Initial velocity force multiplier (Can be negative)")]
        public float ForceMultiplier = Random.Range(1f, 5f);
        
        public Vector3 AdditionalForce;

        public bool ApplyGravity = true;

        [Tooltip("Destroy the ragdoll after a delay?")]
        public bool DestroyRagdoll = true;
        
        [HideFieldIf("DestroyRagdoll", false)]
        public float DestroyDelay = Random.Range(0.2f, 1.5f);
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new RagdollEffect();

            cp.RagdollPrefab = RagdollPrefab;
            cp.ForceMultiplier = ForceMultiplier;
            cp.AdditionalForce = AdditionalForce;
            cp.ApplyGravity = ApplyGravity;
            cp.DestroyRagdoll = DestroyRagdoll;
            cp.DestroyDelay = DestroyDelay;
            cp.Init(origin, target, unscaledTime, interactionDirection);
            
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            if (target == null) return true;
            
            //If there's a ragdoll prefab, Destroy the passed "target" and instantiate the ragdoll instead.
            if (RagdollPrefab != null)
            {
                var targetPos = target.transform.position;
                Object.Destroy(target);

                target = Object.Instantiate(RagdollPrefab, targetPos, Quaternion.identity, GameFeelEffectExecutor.Instance.transform);
            }
            
            if (!target.GetComponent<Rigidbody>() && !target.GetComponent<Rigidbody2D>())
            {
                var rigid = target.AddComponent<Rigidbody>();

                rigid.useGravity = ApplyGravity;
                rigid.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);                
            }
            else
            {
                var body = target.GetComponent<Rigidbody>();
                if (body)
                {
                    body.useGravity = ApplyGravity;
                    body.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);
                }
                else
                {
                    var body2D = target.GetComponent<Rigidbody2D>();
                    body2D.gravityScale = ApplyGravity ? 1f : 0f;
                    body2D.velocity = AdditionalForce + (interactionDirection.HasValue ? interactionDirection.Value.normalized * ForceMultiplier : Vector3.zero);
                }
            }

            if (DestroyRagdoll)
            {
                var destroy = new DestroyEffect();
                destroy.Delay = DestroyDelay;
                this.OnComplete(destroy);    
            }
            
            return true;
        }
    }
}