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
            
            this.OnOffspring(fade);
        }

        public GameObject TrailPrefab;

        public Vector3 TrailPositionOffset;
        
        // [SerializeReference]
        // [ShowTypeAttribute]
        // [Tooltip("This list will replace the standard fade effect of the trail. Remember add a DestroyEffect to the list.")]
        // public List<GameFeelEffect> CustomFadeEffects = new List<GameFeelEffect>();
        
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            Vector3? interactionDirection = null)
        {
            var cp = new TrailEffect();

            cp.TrailPrefab = TrailPrefab;
            // cp.FadeTime = FadeTime;
            // cp.FadeDelay = FadeDelay;
            // cp.FadeEase = FadeEase;
            cp.TrailPositionOffset = TrailPositionOffset;
            // cp.CustomFadeEffects = CustomFadeEffects;
            cp.Init(origin, target, interactionDirection);
            
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
            
            QueueOffspringEffects(trailObject);
            
            //We're done!
            return true;
        }
    }
}