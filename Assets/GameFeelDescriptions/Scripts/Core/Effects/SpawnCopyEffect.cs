using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class SpawnCopyEffect : SpawningGameFeelEffect
    {
        public SpawnCopyEffect()
        {
            Description = "Make a copy of the target.";
            
            // NOTE: Consider destroying the copy after a bit!
            // this.OnOffspring(new DestroyEffect{Delay = 2f, RandomizeDelay = true});
        }

        public string SetTag = "Untagged";
        public bool RemoveColliders = true;
        public bool RemoveRigidbodies = true;
        public bool RemoveScripts = true;

        public bool FollowTarget;
        public float SmoothTime = 0.01f;

        public float ScaleFactor = 1f;
        public Vector3 PositionOffset;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            var cp = new SpawnCopyEffect();

            cp.SetTag = SetTag;
            cp.RemoveColliders = RemoveColliders;
            cp.RemoveScripts = RemoveScripts;
            cp.RemoveRigidbodies = RemoveRigidbodies;
            
            cp.FollowTarget = FollowTarget;
            cp.SmoothTime = SmoothTime;
            
            cp.ScaleFactor = ScaleFactor;
            cp.PositionOffset = PositionOffset;
            cp.Init(origin, target, interactionDirection);
            
            cp.targetPos = target != null ? target.transform.position : origin.transform.position;
            
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            if (target == null) return true;

            var copy = CopyAndStripTarget(target, RemoveRigidbodies, RemoveColliders, RemoveScripts);
            copy.tag = SetTag;
            
            copy.transform.position += PositionOffset;
            copy.transform.localScale *= ScaleFactor;

            if (FollowTarget)
            {
                var follow = copy.AddComponent<SmoothDampFollow>();
                follow.follow = target;
                follow.SmoothTime = SmoothTime;
                follow.offset = PositionOffset;
            }

            QueueOffspringEffects(copy);
            
            //We're done!
            return true;
        }
    }
}