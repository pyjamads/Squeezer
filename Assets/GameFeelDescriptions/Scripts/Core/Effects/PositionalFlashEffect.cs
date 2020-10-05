using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    [Serializable]
    public class PositionalFlashEffect : SpawningGameFeelEffect
    {
        public PositionalFlashEffect()
        {
            Description = "Make a positional flash which quickly fades away.";
            
            var fadeTime = Random.Range(0.1f,0.3f);
            
            var fade = new MaterialColorChangeEffect();
            fade.to = Color.clear;
            fade.relative = false;
            fade.Duration = fadeTime;

            //Make sure to destroy the copy after the fade!
            fade.OnComplete(new DestroyEffect());
            
            this.OnOffspring(fade);
        }

        public Color FlashColor = Random.ColorHSV();
        public bool FlashTransparency;
        
        public GameObject FlashPrefab;

        public PrimitiveType FlashPrimitive;

        public Vector3 Scale = Vector3.one * (Random.Range(1, 200) / 100f);
        public Vector3 PositionOffset;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new PositionalFlashEffect();

            cp.FlashPrefab = FlashPrefab;
            cp.FlashPrimitive = FlashPrimitive;
            cp.PositionOffset = PositionOffset;
            cp.FlashColor = FlashColor;
            cp.FlashTransparency = FlashTransparency;
            cp.Scale = Scale;
            cp.Init(origin, target, triggerData);

            if (target == null && origin == null) return null;
            
            cp.targetPos = target != null ? target.transform.position : origin.transform.position;
            
            return DeepCopy(cp);
        }

        protected override void ExecuteSetup()
        {
            //Update target pos if target is still available.
            if (target != null)
            {
                targetPos = target.transform.position;
            }
            
            base.ExecuteSetup();
        }

        protected override bool ExecuteTick()
        {
            //Update target pos if target is still available.
            if (target != null)
            {
                targetPos = target.transform.position;
            }
            
            GameObject flashObject;
            if (FlashPrefab != null)
            {
                flashObject = Object.Instantiate(FlashPrefab, GameFeelEffectExecutor.Instance.transform);
                flashObject.transform.position = targetPos;
                
                var renderer = flashObject.GetComponent<Renderer>();
                //renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.material.color = FlashColor;
            }
            else
            {
                flashObject = GameObject.CreatePrimitive(FlashPrimitive);
                flashObject.transform.parent = GameFeelEffectExecutor.Instance.transform;
                flashObject.transform.position = targetPos;
                var renderer = flashObject.GetComponent<Renderer>();
                renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                if (FlashTransparency)
                {
                    SetMaterialTransparentBlendMode(renderer.material);
                }
                renderer.material.color = FlashColor;
            }

            flashObject.transform.position += PositionOffset;
            flashObject.transform.localScale = Scale;

            //TODO: Note that it probably doesn't make sense to have an ExecuteOnCompletion list in addition
            //TODO: to the CustomFadeEffect list, and as such effects such as the trail here,
            //TODO: could just set it's own target to the trailObject, and allow which ever effects exists,
            //TODO: in the ExecuteOnCompletion list to be executed on the trailObject. (See Ragdoll and Shatter) 13/05/2020   
            
            QueueOffspringEffects(flashObject);
            
            //We're done!
            return true;
        }
    }
}