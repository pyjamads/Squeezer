using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    [Serializable]
    public class TrailEffect : SpawningGameFeelEffect
    {
        public TrailEffect()
        {
            //TODO: Consider renaming to 'copy and fade effect', making a trail is just a side-effect of calling this continuously. 18/4/2020
            Description = "Make a trail of copies of the game object, which slowly fades.";
            
            var fadeTime = Random.Range(0.1f,2f);
            var fadeDelay = 0.1f;
            
            var fade = new MaterialColorChangeEffect();
            fade.to = Color.clear;
            fade.relative = false;
            fade.Delay = fadeDelay;
            fade.Duration = fadeTime;

            //Make sure to destroy the copy after the fade!
            fade.OnComplete(new DestroyEffect());
            
            ExecuteOnOffspring = new List<GameFeelEffect>
            {
                fade
            };
        }

        public GameObject TrailPrefab;

        public Vector3 TrailPositionOffset;
        
        // [SerializeReference]
        // [ShowTypeAttribute]
        // [Tooltip("This list will replace the standard fade effect of the trail. Remember add a DestroyEffect to the list.")]
        // public List<GameFeelEffect> CustomFadeEffects = new List<GameFeelEffect>();
        
        public float DelayBetweenCopies;

        private float LastCopyTime = 0;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            if (Time.time - LastCopyTime < DelayBetweenCopies)
            {
                return null;
            }
            
            var cp = new TrailEffect();

            cp.TrailPrefab = TrailPrefab;
            // cp.FadeTime = FadeTime;
            // cp.FadeDelay = FadeDelay;
            // cp.FadeEase = FadeEase;
            cp.TrailPositionOffset = TrailPositionOffset;
            cp.DelayBetweenCopies = DelayBetweenCopies;
            // cp.CustomFadeEffects = CustomFadeEffects;
            cp.Init(origin, target, unscaledTime, interactionDirection);
            
            LastCopyTime = Time.time;
            cp.targetPos = target != null ? target.transform.position : origin.transform.position;
            
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            GameObject trailObject;
            if (TrailPrefab != null)
            {
                trailObject = Object.Instantiate(TrailPrefab, GameFeelEffectExecutor.Instance.transform);
                trailObject.transform.position = targetPos;
            }
            else
            {
                if (target == null) return true;
            
                if (!target.GetComponent<Renderer>())
                {
                    Debug.LogError("No renderer attached to object: " + target.name);
                    return true;
                }
                
                //Get a copy and remove all scripts, rigidbodies and colliders.
                trailObject = CopyAndStripTarget(target);
            }

            trailObject.transform.position += TrailPositionOffset;

            //TODO: Note that it probably doesn't make sense to have an ExecuteOnCompletion list in addition
            //TODO: to the CustomFadeEffect list, and as such effects such as the trail here,
            //TODO: could just set it's own target to the trailObject, and allow which ever effects exists,
            //TODO: in the ExecuteOnCompletion list to be executed on the trailObject. (See Ragdoll and Shatter) 13/05/2020   
            
            QueueOffspringEffects(trailObject);
            
            return false;
        }
    }
}