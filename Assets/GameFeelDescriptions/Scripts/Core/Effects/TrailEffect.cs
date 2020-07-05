using System;
using System.CodeDom;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    [Serializable]
    public class TrailEffect : GameFeelEffect //TODO: use SpawningGameFeelEffect instead, and fix effect trees!
    {
        public TrailEffect()
        {
            //TODO: Consider renaming to 'copy and fade effect', making a trail is just a side-effect of calling this continuously. 18/4/2020
            Description = "Make a trail of copies of the game object, which slowly fades.";
        }

        public GameObject TrailPrefab;
        
        [Tooltip("The time it takes for the trail copies to fade")]
        [HideFieldIf("CustomFadeEffects", null)]
        public float FadeTime = Random.Range(0.1f,2f);
        
        [HideFieldIf("CustomFadeEffects", null)]
        public float FadeDelay = 0.1f;
        
        [HideFieldIf("CustomFadeEffects", null)]
        public EasingHelper.EaseType FadeEase;
        
        [HideFieldIf("CustomFadeEffects", null)]
        public Vector3 FadeCopyOffset;
        
        [SerializeReference]
        [ShowTypeAttribute]
        [Tooltip("This list will replace the standard fade effect of the trail. Remember add a DestroyEffect to the list.")]
        public List<GameFeelEffect> CustomFadeEffects = new List<GameFeelEffect>();
        
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
            cp.FadeTime = FadeTime;
            cp.FadeDelay = FadeDelay;
            cp.FadeEase = FadeEase;
            cp.FadeCopyOffset = FadeCopyOffset;
            cp.DelayBetweenCopies = DelayBetweenCopies;
            cp.CustomFadeEffects = CustomFadeEffects;
            cp.Init(origin, target, unscaledTime, interactionDirection);
            
            LastCopyTime = Time.time;
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            if (target == null) return true;

//            var isSprite = true; 
//            if (!target.GetComponent<SpriteRenderer>())
//            {
//                isSprite = false;
            if (!target.GetComponent<Renderer>())
            {
                Debug.LogError("No renderer attached to object: " + target.name);
                return true;
            }
//            }

            GameObject trailObject;
            if (TrailPrefab != null)
            {
                trailObject = Object.Instantiate(TrailPrefab, GameFeelEffectExecutor.Instance.transform);
                trailObject.transform.position = target.transform.position;
            }
            else
            {
                //Get a copy and remove all scripts, rigidbodies and colliders.
                trailObject = Object.Instantiate(target, GameFeelEffectExecutor.Instance.transform, true);
                trailObject.tag = "Untagged";
                
                var scripts = trailObject.GetComponentsInChildren<MonoBehaviour>();
                foreach (var script in scripts)
                {
                    Object.Destroy(script);
                }

                var rigid = trailObject.GetComponent<Rigidbody>();
                Object.Destroy(rigid);
                
                var rigid2D = trailObject.GetComponent<Rigidbody2D>();
                Object.Destroy(rigid2D);
                
                var col = trailObject.GetComponent<Collider>();
                Object.Destroy(col);
                
                var col2D = trailObject.GetComponent<Collider2D>();
                Object.Destroy(col2D);
            }

            //TODO: Note that it probably doesn't make sense to have an ExecuteOnCompletion list in addition
            //TODO: to the CustomFadeEffect list, and as such effects such as the trail here,
            //TODO: could just set it's own target to the trailObject, and allow which ever effects exists,
            //TODO: in the ExecuteOnCompletion list to be executed on the trailObject. (See Ragdoll and Shatter) 13/05/2020   
            
            //if (CustomFadeEffects == null || CustomFadeEffects.Count > 0)
            if (CustomFadeEffects.Count > 0)
            {
                for (var i = 0; i < CustomFadeEffects.Count; i++)
                {
                    //If the effect is disabled, skip it.
                    if(CustomFadeEffects[i].Disabled) continue;
            
                    var effectCopy = CustomFadeEffects[i].CopyAndSetElapsed(origin, trailObject, unscaledTime);
            
                    if(effectCopy == null) continue;
                    
                    //We don't need to handle effect copies, because it's a new target.
                    effectCopy.QueueExecution();   
                }
            }
            else
            {
                //Add a translate effect, to move towards the offset.
                var translate = new TranslateEffect();
                translate.to = FadeCopyOffset;
                translate.easing = EasingHelper.EaseType.Linear;
                translate.relative = true;
                translate.Duration = FadeDelay + FadeTime;
                
                translate.Init(origin, trailObject, unscaledTime);
                translate.SetupLooping();
                translate.SetElapsed();
                
                translate.QueueExecution();
                
                //NOTE: this is a really neat use of our effect system, to queue a fade out and a destroy effect.
                TweenEffect<Color> fade;

//                if (isSprite)
//                {
//                    fade = new SpriteColorChangeEffect();
//                }
//                else
//                {
                    fade = new MaterialColorChangeEffect();
//                }
                
                fade.to = Color.clear;
                fade.relative = false;
                fade.Delay = FadeDelay;
                fade.Duration = FadeTime;
                fade.easing = FadeEase;
                
                //TODO: maybe just add the fade to this.OnComplete, and set target = trailObject. 13/05/2020
                fade.Init(origin, trailObject, unscaledTime);
                fade.SetupLooping();
                fade.SetElapsed();
            
                //Make sure to destroy the copy after the fade!
                fade.OnComplete(new DestroyEffect());
                
                //We don't need to handle effect copies, because it's a new target.
                fade.QueueExecution();
            }

            return false;
        }
    }
}