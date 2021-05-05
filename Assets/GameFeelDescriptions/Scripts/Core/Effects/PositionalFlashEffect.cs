using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new PositionalFlashEffect();

            cp.FlashPrefab = FlashPrefab;
            cp.FlashPrimitive = FlashPrimitive;
            cp.PositionOffset = PositionOffset;
            cp.FlashColor = FlashColor;
            cp.FlashTransparency = FlashTransparency;
            cp.Scale = Scale;
            cp.Init(origin, target, triggerData);

            if (target == null && origin == null)
            {
                cp.targetPos = Vector3.zero;
            }
            else
            {
                cp.targetPos = target != null ? target.transform.position : origin.transform.position;    
            }

            return DeepCopy(cp, ignoreCooldown);
        }

        public override void Mutate(float amount = 0.05f)
        {
            if (RandomExtensions.Boolean())
            {
                //Make a random color, and add/subtract a proportional amount here.
                FlashColor += RandomExtensions.Sign() * amount * Random.ColorHSV().withA(Random.value);
            }

            if (RandomExtensions.Boolean(amount))
            {
                FlashTransparency = !FlashTransparency;
            }

            if (RandomExtensions.Boolean(amount))
            {
                FlashPrimitive = RandomExtensions.GetRandomElement(new List<PrimitiveType> {PrimitiveType.Plane});
            }

            if (RandomExtensions.Boolean())
            {
                PositionOffset += Random.insideUnitSphere * amount;
            }

            if (RandomExtensions.Boolean())
            {
                //Make a random scale, and add/subtract a proportional amount here.
                Scale += Vector3.one * RandomExtensions.MutationAmount(amount); 
                if (Scale.x < 0)
                {
                    Scale = Vector3.zero;
                }
            }
            
            base.Mutate(amount);
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

        protected override bool ExecuteTick() //Scene scene
        {
            //STEP 1: Get position!
            //NOTE: Use target position as fallback.
            if (target != null)
            {
                targetPos = target.transform.position;
            }

            //If triggerData provides position and normal, use those.
            if (triggerData is CollisionData collisionData)
            {
                //NOTE: For "OnTriggerEnter/Exit/Stay" collisions,
                //this returns a point on the bounds + the direction between the center of the two colliders.
                var (position, _) = collisionData.GetPositionAndNormal();

                //If it's a 2D collision set the z axis from the target Position.
                if (collisionData.wasCollision2D())
                {
                    position.z = target != null ? target.transform.position.z : targetPos.z;
                }

                targetPos = position;
            }
            else if (triggerData is PositionalData positionalData)
            {
                targetPos = positionalData.Position;
            }
            
            GameObject flashObject;
            if (FlashPrefab != null)
            {
                flashObject = GameFeelEffectExecutor.Instantiate(FlashPrefab, targetPos);

                var renderer = flashObject.GetComponent<Renderer>();
                //renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                renderer.sharedMaterial.color = FlashColor;
            }
            else
            {
                flashObject = GameFeelEffectExecutor.Instantiate(FlashPrimitive, targetPos);
                
                var renderer = flashObject.GetComponent<Renderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                if (FlashTransparency)
                {
                    SetMaterialTransparentBlendMode(renderer.sharedMaterial);
                }
                renderer.sharedMaterial.color = FlashColor;
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