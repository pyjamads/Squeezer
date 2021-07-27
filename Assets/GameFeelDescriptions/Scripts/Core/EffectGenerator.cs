using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#else
using System.Reflection;
#endif

using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public static class EffectGenerator
    {
        public enum EffectGeneratorCategories
        {
            RANDOM,
            PICKUP, //TODO: implement better Pickup.
            EXPLODE, //DESTROY
            JUMP, 
            IMPACT, //LAND/BOUNCE 
            SHOOT, 
            PROJECTILE_MOVE,  
            PLAYER_MOVE,
            //TODO: add BlipSelect,
            //TODO: add PowerUp,
        }
        
        //PickupCoin,
        //LaserShoot,
        //Explosion,
        //PowerUp,
        //HitHurt,
        //Jump,
        
        //Random,

        /// <summary>
        /// Generates an effect sequence for the specified category.
        /// </summary>
        /// <param name="category">The selected category to generate recipe based on.</param>
        /// <param name="intensity">The severity (1-10) of the effect, where 1 is mild and 10 is wild.</param>
        /// <returns>A randomly generated effect sequence based on the selected category and severity.</returns>
        public static List<GameFeelEffect> GenerateRecipe(EffectGeneratorCategories category, int intensity = 1, List<GameFeelEffect> locked = null)
        {
            if (locked == null)
            {
                locked = new List<GameFeelEffect>();
            }
            
            //Clamp severity between 1 and 10.
            intensity = Mathf.Clamp(intensity, 1, 10);
            //TODO: consider actually making this into the GameFeelRecipe class instead of just a list. 2020-09-29
            var recipe = new List<GameFeelEffect>();

            if (category == EffectGeneratorCategories.JUMP)
            {
                var stretch = (SquashAndStretchEffect) Activator.CreateInstance(typeof(SquashAndStretchEffect));
                //TODO: more varied settings for stretch
                stretch.Amount = Random.Range(0.01111f * intensity, 0.09999f * intensity); //Amount has two be [0,1[
                stretch.Stretch = true;

                if (locked.Any(item => item is SquashAndStretchEffect) == false)
                {
                    recipe.Add(stretch);    
                }

                var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                //TODO: more varied settings for synth 
                synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.Jump;
                synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                synth.LoadSynthParameters();
                
                if (locked.Any(item => item is AudioSynthPlayEffect) == false)
                {
                    recipe.Add(synth);    
                }
                
                if (Random.value > 0.5f)
                {
                    //TODO: add a particle poof instead of trail! once copying is moved to effects!
                    var particle = (ParticleScatterEffect) Activator.CreateInstance(typeof(ParticleScatterEffect));
                    particle.ApplyGravity = true;
                    particle.ScatterAngle = Vector3.one * 360;
                    particle.AmountOfParticles = 5 * intensity;
                    particle.ParticleScale = 0.01f;
                    particle.ParticlePrimitive = PrimitiveType.Sphere;
                    particle.Speed = 0.2f * intensity;

                    if (locked.Any(item => item is ParticleScatterEffect) == false)
                    {
                        recipe.Add(particle);    
                    }
                }
            }
            else if (category == EffectGeneratorCategories.IMPACT)
            {
                if (locked.Any(item => item is SquashAndStretchEffect) == false)
                {
                    var squash = (SquashAndStretchEffect) Activator.CreateInstance(typeof(SquashAndStretchEffect));
                    //TODO: more varied settings for squash
                    squash.Amount = Random.Range(0.01111f * intensity, 0.09999f * intensity); //Amount has two be [0,1[

                    recipe.Add(squash);    
                }

                if (locked.Any(item => item is AudioSynthPlayEffect) == false)
                {
                    var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                    //TODO: more varied settings for synth 
                    if (intensity >= 5)
                    {
                        synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.Explosion;
                    }
                    else
                    {
                        synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.HitHurt;
                    }
                
                    synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                    synth.LoadSynthParameters();
                    
                    recipe.Add(synth);    
                }
                
                if (locked.Any(item => item is MaterialColorChangeEffect) == false)
                {
                    var mat = (MaterialColorChangeEffect) Activator.CreateInstance(typeof(MaterialColorChangeEffect));
                    //TODO: more varied settings for squash
                    mat.loopType = TweenEffect<Color>.LoopType.Yoyo;
                    mat.repeat = 1;
                    mat.Duration = Random.Range(0.01f * intensity, 0.5f * intensity);
                    //mat.to = Color.red;
                    mat.relative = false;
                    
                    recipe.Add(mat);    
                }

                if (intensity > 5)
                {
                    // var particles = (ShatterEffect) Activator.CreateInstance(typeof(ShatterEffect));
                    // particles.AmountOfPieces = 3 * intensity;
                    //
                    // if (locked.Any(item => item is ShatterEffect) == false)
                    // {
                    //     recipe.Add(particles); 
                    // }
                    
                    if (locked.Any(item => item is CameraShakeEffect) == false)
                    {
                        var camShake = (CameraShakeEffect) Activator.CreateInstance(typeof(CameraShakeEffect));
                        camShake.amount = 0.05f * intensity;
                        camShake.axis = Random.onUnitSphere * 2;

                        recipe.Add(camShake);
                    }
                }
                
                if (locked.Any(item => item is ParticlePuffEffect) == false)
                {
                    var particle = (ParticlePuffEffect) Activator.CreateInstance(typeof(ParticlePuffEffect));

                    particle.AmountOfParticles = Random.Range(1, 10) * intensity;
                    
                    particle.ParticleScale = Random.Range(0.02f, 0.2f) * intensity;
                    particle.ParticleLifetime = Random.Range(0.05f, 0.5f) * intensity;
                    
                    particle.Radius = Random.Range(0.02f, 0.5f) * intensity;
                    particle.Height = Random.Range(0.02f, 0.5f) * intensity;

                    particle.ExpansionShape = EnumExtensions.GetRandomValue<ParticlePuffEffect.PuffShapes>();
                    
                    particle.ParticlePrimitive = EnumExtensions.GetRandomValue(new List<PrimitiveType>
                    {
                            PrimitiveType.Plane,
                            PrimitiveType.Quad
                        });
                    
                    recipe.Add(particle); 
                }   
               
            }
            else if (category == EffectGeneratorCategories.SHOOT)
            {
                var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                //TODO: more varied settings for synth 
                synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.LaserShoot;

                synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                synth.LoadSynthParameters();
                
                if (locked.Any(item => item is AudioSynthPlayEffect) == false)
                {
                    recipe.Add(synth);    
                }
                
                //TODO: We need a way to either store the "primitives" we create here, until they are needed,
                //TODO: or we just create a set of prefabs that we can pull from Resources.load  2020-08-13
                 
                var muzzleFlash = (PositionalFlashEffect) Activator.CreateInstance(typeof(PositionalFlashEffect));
                muzzleFlash.FlashPrimitive = PrimitiveType.Sphere;

                var scale = (ScaleEffect) Activator.CreateInstance(typeof(ScaleEffect));
                //Scale the flash based on  the severity
                scale.to = Vector3.one * Random.Range(0.02f, 0.2f) * intensity;
                scale.relative = false;
                scale.Duration = 0; //set immediately!
                
                muzzleFlash.ExecuteOnOffspring.Add(scale);
                
                if (locked.Any(item => item is PositionalFlashEffect) == false)
                {
                    recipe.Add(muzzleFlash);    
                }

                if (intensity > 6)
                {
                    var particles = (ParticleScatterEffect) Activator.CreateInstance(typeof(ParticleScatterEffect));
                    particles.usePrimitiveParticles = true;
                    particles.ParticlePrimitive = PrimitiveType.Sphere;
                    particles.AmountOfParticles = 3 * intensity;
                    particles.ApplyGravity = true;
                    particles.ScatterAngle = Vector3.one * 45f;

                    if (locked.Any(item => item is ShatterEffect) == false)
                    {
                        recipe.Add(particles);
                    }
                    
                    var camShake = (CameraShakeEffect) Activator.CreateInstance(typeof(CameraShakeEffect));
                    
                    camShake.amount = 0.05f * intensity;
                    camShake.axis = Random.onUnitSphere * 2;
                    
                    if (locked.Any(item => item is CameraShakeEffect) == false)
                    {
                        recipe.Add(camShake);
                    }
                }

                //Add recoil for severe "shots"
                var translate = (TranslateEffect) Activator.CreateInstance(typeof(TranslateEffect));
                translate.relative = true;
                translate.to = Vector3.zero;
                translate.useInteractionDirection = true;
                translate.interactionDirectionMultiplier = 0.2f * (Mathf.Clamp(intensity,3, 10) - 3);
                translate.loopType = DurationalGameFeelEffect.LoopType.Yoyo;
                translate.repeat = 1;

                if (locked.Any(item => item is TranslateEffect) == false)
                {
                    recipe.Add(translate); 
                }
            }
            else if (category == EffectGeneratorCategories.PICKUP)
            {
                var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                //TODO: more varied settings for synth 
                synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.PickupCoin;

                synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                synth.LoadSynthParameters();
                
                if (locked.Any(item => item is AudioSynthPlayEffect) == false)
                {
                    recipe.Add(synth);    
                }
                
                //Kinda needs a target position, or object to tween towards.
                //Also we might need a dynamic tween towards effect.
            }
            else if (category == EffectGeneratorCategories.EXPLODE)
            {
                if (locked.Any(item => item is AudioSynthPlayEffect) == false)
                {
                    var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                    //TODO: more varied settings for synth 
                    synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.Explosion;

                    synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                    synth.LoadSynthParameters();
                    
                    recipe.Add(synth);
                }
                
                //Add a flash, and a particle poof, maybe some debris for very strong explosions (primitives or prefabs from a shatter), camera shake as well
                if (locked.Any(item => item is PositionalFlashEffect) == false)
                {
                    var posFlash = (PositionalFlashEffect) Activator.CreateInstance(typeof(PositionalFlashEffect));
                    
                    posFlash.Scale = Vector3.one * Random.Range(0.01f, 0.2f) * intensity;
                    //Reddish, going towards more desaturated colors with more intensity.
                    posFlash.FlashColor = Random.ColorHSV(0f, 0.2f, 1f - 0.1f * intensity, 1f - 0.01f * intensity, 0.8f, 1f).withA(intensity < 5 ? 1f : 0.5f);
                    posFlash.FlashTransparency = intensity < 5;

                    recipe.Add(posFlash);
                }
                
                if (locked.Any(item => item is ParticlePuffEffect) == false)
                {
                    var particle = (ParticlePuffEffect) Activator.CreateInstance(typeof(ParticlePuffEffect));

                    particle.AmountOfParticles = Random.Range(1, 10) * intensity;
                    
                    particle.ParticleScale = Random.Range(0.02f, 0.2f) * intensity;
                    particle.ParticleLifetime = Random.Range(0.05f, 0.5f) * intensity;
                    
                    particle.Radius = Random.Range(0.02f, 0.5f) * intensity;
                    particle.Height = Random.Range(0.02f, 0.5f) * intensity;

                    particle.ExpansionShape = EnumExtensions.GetRandomValue<ParticlePuffEffect.PuffShapes>();
                    
                    particle.ParticlePrimitive = EnumExtensions.GetRandomValue(new List<PrimitiveType>
                    {
                        PrimitiveType.Plane,
                        PrimitiveType.Quad
                    });
                    
                    recipe.Add(particle); 
                }
                
                if (intensity > 6)
                {
                    if (Random.value > 0.5f)
                    {
                        if (locked.Any(item => item is ShatterEffect) == false)
                        {
                            var particles = (ShatterEffect) Activator.CreateInstance(typeof(ShatterEffect));
                            particles.usePrimitivePieces = true;
                            particles.PiecePrimitive = PrimitiveType.Cube;
                            particles.AmountOfPieces = 3 * intensity; //TODO: make the severity scale less linear?

                            var scale = new ScaleEffect();
                            scale.setFromValue = true;
                            scale.to = scale.@from = Vector3.one * Random.Range(0.01f, 0.2f);

                            particles.OnOffspring(scale);
                            recipe.Add(particles);
                        }    
                    }
                    else
                    {
                        if (locked.Any(item => item is ParticleScatterEffect) == false)
                        {
                            var particles = (ParticleScatterEffect) Activator.CreateInstance(typeof(ParticleScatterEffect));
                            particles.usePrimitiveParticles = true;
                            particles.ApplyGravity = true;
                            particles.ParticlePrimitive = PrimitiveType.Cube;
                            particles.AmountOfParticles = 3 * intensity; 

                            particles.ScatterAngle = Vector3.one * 36 * intensity;
                            
                            recipe.Add(particles);
                        } 
                    }
                    
                }

                if (locked.Any(item => item is CameraShakeEffect) == false)
                {
                    var camShake = (CameraShakeEffect) Activator.CreateInstance(typeof(CameraShakeEffect));
                    
                    camShake.amount = 0.05f * intensity;
                    camShake.axis = Random.onUnitSphere * 2;
                    
                    recipe.Add(camShake);
                }
            }
            else if (category == EffectGeneratorCategories.PLAYER_MOVE)
            {
                if (locked.Any(item => item is TrailEffect) == false)
                {
                    //TODO: Make more complex generator here! 
                    var trailEffect = (TrailEffect) Activator.CreateInstance(typeof(TrailEffect));
                    //TODO: add fancy trails!
                    //TODO: add footfall or continuous sounds, to signify the movement
                    
                    recipe.Add(trailEffect);
                }
            }
            else if (category == EffectGeneratorCategories.PROJECTILE_MOVE)
            {
                if (locked.Any(item => item is TrailEffect) == false)
                {
                    //TODO: Make more complex generator here!
                    var trailEffect = (TrailEffect) Activator.CreateInstance(typeof(TrailEffect));
                    //TODO: add fancy trails!
                    //TODO: add wooshing sound, with a short drop off (can only be heard close by) [requires more advanced sound setup]
                    
                    if (intensity > 6)
                    {
                        var ragdoll = (RagdollEffect) Activator.CreateInstance(typeof(RagdollEffect));
                        ragdoll.ApplyGravity = true;
                        ragdoll.AdditionalForce = Vector3.up * intensity;
                
                        trailEffect.ExecuteOnOffspring.Add(ragdoll);
                    
                        var shake = (ShakeEffect) Activator.CreateInstance(typeof(ShakeEffect));
                        shake.amount = 0.1f * intensity;
                        shake.Delay = 0.1f;
                        shake.Duration = 5f;
                    
                        trailEffect.ExecuteOnOffspring.Add(shake);
                    }

                    recipe.Add(trailEffect);
                }
            }
            else //if (category == EffectGeneratorCategories.RANDOM)
            {
                //TODO: with the addition of the Type to GameFeelBehaviorBase, we could do
                //TODO: GameFeelBehaviorBase<OnCollisionTrigger>.GetGameFeelEffects() to get things that match with collision Triggers. 2020-10-26
                //Get possible effects from trigger type
                var effects = GetGameFeelEffects();
                //TODO: we can also do the above for the MakeRecipe call.
                //Generate a recipe of up to 5 effects, from those effects.
                recipe = MakeRecipe(effects);

                //If there's room, and no sound effect has been added, add one.
                if (recipe.Count < 5 && !recipe.Any(item => item is AudioSynthPlayEffect))
                {
                    var synth = (AudioSynthPlayEffect) Activator.CreateInstance(typeof(AudioSynthPlayEffect));
                    //TODO: more varied settings for synth 
                    synth.soundGeneratorBase = AudioSynthPlayEffect.SynthBaseSounds.Random;
                    synth.synthParameters = synth.GenerateSynthParameters(false, intensity: intensity);
                    synth.LoadSynthParameters();
                    recipe.Add(synth);
                }
                
                //TODO: verify this is actually a good idea, maybe when you say randomize you're okay that the ones you've locked are repeated. 2020-08-17
                //Remove overlapping types
                recipe.RemoveAll(item => locked.Any(inner => inner.GetType() == item.GetType()));
            }

            return recipe;
        }

        public static List<Func<GameFeelEffect>> GetGameFeelEffects(GameFeelTriggerType triggerType = GameFeelTriggerType.OnCollision, params object[] context)
        {
            IEnumerable<Type> types;
            
#if UNITY_EDITOR
            types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
#else
            var assembly = Assembly.GetAssembly(typeof(GameFeelEffect));
            types = assembly.GetTypes().Where(item => item is GameFeelEffect);
#endif
            var gameFeelEffects = new List<Func<GameFeelEffect>>();

            foreach (var type in types)
            {
                // Skip abstract classes because they should not be instantiated
                if (type.IsAbstract)
                    continue;
                    
                //Determine (aka hardcode for first iteration) which effects can be applied to each trigger type.
                //Later this might be better determined by adding Attributes to each effect
                //based on the Triggers we'd like them to show up for.
                switch (triggerType)
                {
                    case GameFeelTriggerType.OnStart when 
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                        type == typeof(PropertyTweenEffect) ||
                        //type == typeof(FollowOriginEffect) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(SquashAndStretchEffect):
                    {
                        break;        
                    }

                    case GameFeelTriggerType.OnDestroy when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                        type == typeof(PropertyTweenEffect) ||
                        //type == typeof(FollowOriginEffect) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(SquashAndStretchEffect):
                    {
                        break;
                    }
                        
                    case GameFeelTriggerType.OnDisable when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
                        //type == typeof(AudioSynthPlayEffect) ||
                        type == typeof(PropertyTweenEffect) ||
                        //type == typeof(FollowOriginEffect) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(SquashAndStretchEffect):
                    {
                        break;
                    }
                        
                    case GameFeelTriggerType.OnMove when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
                        type == typeof(PropertyTweenEffect):// ||
//                            type == typeof(AudioSynthPlayEffect):// ||
                        //type == typeof(FollowOriginEffect) ||
                        //type == typeof(RotateTowardsDirectionEffect) ||
                        //type == typeof(SquashAndStretchEffect):
                    {
                        break;
                    }
                    case GameFeelTriggerType.OnRotate when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                        type == typeof(PropertyTweenEffect) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        //type == typeof(FollowOriginEffect) ||
                        type == typeof(RotateTowardsDirectionEffect):// ||
                        //type == typeof(SquashAndStretchEffect):
                    {
                        break;
                    }
                    case GameFeelTriggerType.OnCustomEvent when
//                            type == typeof(DisableEffect) ||
//                            type == typeof(EnableEffect) ||
//                            type == typeof(DestroyEffect) ||
                        //type == typeof(FollowOriginEffect) ||
//                            type == typeof(AudioSynthPlayEffect) ||
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(PropertyTweenEffect) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(SquashAndStretchEffect):
                    {
                        break;
                    }
                    case GameFeelTriggerType.OnCollision when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(PropertyTweenEffect):// ||
//                            type == typeof(AudioSynthPlayEffect):
                    {
                        break;
                    }
                    case GameFeelTriggerType.OnStateChanged when
                        type == typeof(DisableEffect) ||
                        type == typeof(EnableEffect) ||
                        type == typeof(DestroyEffect) ||
                        type == typeof(InvokeUnityEvent) ||
                        type == typeof(RotateTowardsDirectionEffect) ||
                        type == typeof(AudioClipModulationEffect) ||
                        type == typeof(AudioClipPlayEffect) ||
                        type == typeof(DisableRendererEffect) ||
                        type == typeof(TriggerCustomEventEffect) ||
                        type == typeof(PropertyTweenEffect):// ||
//                            type == typeof(AudioSynthPlayEffect):
                    {
                        break;
                    }
                    default:
                    {
                        gameFeelEffects.Add(() =>
                        {
                            var instance = (GameFeelEffect) Activator.CreateInstance(type);
                                
                            //Reasonable default values will be set in the effects themselves for now.
                                
                            //TODO: verify that the effect is compatible with the target. 11/05/2020
                                
                            return instance;
                        });
                        break;
                    }
                }
            }

//                var gameFeelEffectNames = new string[gameFeelEffects.Count];
//                for (var index = 0; index < gameFeelEffects.Count; index++)
//                {
//                    gameFeelEffectNames[index] = gameFeelEffects[index].Name;
//                }
            return gameFeelEffects;
        }

        public static List<GameFeelEffect> MakeRecipe(List<Func<GameFeelEffect>> effectTypes, int maxEffects = 5, float queueProp = 0.2f)
        {
            //Min 1, max = maxEffects
            var elements = 1 + Random.Range(0, maxEffects);
            var list = new List<GameFeelEffect>();
            for (int i = 0; i < elements; i++)
            {
                //TODO: consider making a factory function on GameFeelEffects that returns a randomized instance. 30/04/2020
                //TODO: consider making sure random elements don't repeat, such as multiple destroy / disables. 2020-08-13 
                var instance = effectTypes.GetRandomElement().Invoke();
                
                if (list.Count > 0 &&  Random.value < queueProp)
                {
                    list.GetRandomElement().OnComplete(instance);
                }
                else
                {
                    list.Add(instance);    
                }
            }

            return list;
        }
        
        public static void MutateGroup(GameFeelEffectGroup gameFeelEffectGroup, float mutateAmount = 0.05f,
            float addProbability = 0.05f, float removeProbability = 0.05f)
        {
            MutateGroup(gameFeelEffectGroup.EffectsToExecute, mutateAmount, addProbability, removeProbability);
        }

        public static void MutateGroup(List<GameFeelEffect> effectsToExecute, float mutateAmount = 0.05f,
            float addProbability = 0.05f, float removeProbability = 0.05f)
        {
            var constructors = EffectGenerator.GetGameFeelEffects();

            if (effectsToExecute.Count > 2)
            {
                //Group level remove, then mutate, then add!
                var unlockedEffects = effectsToExecute.Where(item => !item.Lock);
                if (RandomExtensions.Boolean(removeProbability))
                {
                    var effectToRemove = unlockedEffects.GetRandomElement();
                    effectsToExecute.Remove(effectToRemove);
                    //TODO: Consider whether this should be a "replace",
                    //TODO: because currently it's addProb * removeProb that a replace happens. 2020-07-15
                }    
            }

            //Mutate all effects in the list recursively. Also drop the probabilities for adding and removing by 10% per layer.
            effectsToExecute.ForEach(item =>
                MutateEffectsRecursive(constructors, item, mutateAmount, addProbability * 0.9f, removeProbability * 0.9f));

            //Add new effect to group...
            if (RandomExtensions.Boolean(addProbability))
            {
                var instance = constructors.GetRandomElement().Invoke();
                effectsToExecute.Add(instance);
            }
        }

        public static void MutateEffectsRecursive(List<Func<GameFeelEffect>> effectConstructors,
            GameFeelEffect outerEffect,
            float amount = 0.05f, float addProb = 0.05f, float removeProb = 0.05f, List<GameFeelEffect> traversed = null)
        {
            //Make a visited list!
            var visited = new List<GameFeelEffect>();
            if(traversed != null && !(outerEffect is ParticlePuffEffect || outerEffect is ShatterEffect)) visited.AddRange(traversed);
            visited.Add(outerEffect);
            
            //Remove an unlocked child
            if (RandomExtensions.Boolean(removeProb))
            {
                //If it's a spawner effect, add it to the offspring effects instead.
                if (outerEffect is SpawningGameFeelEffect spawner && spawner.ExecuteOnOffspring.Count > 0)
                {
                    var element = spawner.ExecuteOnOffspring.Where(item => !item.Lock).GetRandomElement();

                    //Don't remove Destroy effects from spawner offspring effects.
                    if (element is DestroyEffect || element.ExecuteAfterCompletion.Any(item => item is DestroyEffect))
                    {
                        //TODO: figure out if we should do something else instead. 2020-09-29
                    }
                    else
                    {
                        spawner.ExecuteOnOffspring.Remove(element);    
                    }
                }
                else if(outerEffect.ExecuteAfterCompletion.Count > 0)
                {
                    var element = outerEffect.ExecuteAfterCompletion.Where(item => !item.Lock).GetRandomElement();

                    //Don't remove Destroy effects from spawner offspring effects.
                    if ((element is DestroyEffect || element.ExecuteAfterCompletion.Any(item => item is DestroyEffect)) && 
                        visited.Any(item => item is SpawningGameFeelEffect))
                    {
                        //TODO: figure out if we should do something else instead. 2020-09-29
                    }
                    else
                    {
                        outerEffect.ExecuteAfterCompletion.Remove(element);    
                    }
                }
            }

            //Run mutate on the children.
            foreach (var innerEffect in outerEffect.ExecuteAfterCompletion)
            {
                if (!innerEffect.Lock)
                {
                    innerEffect.Mutate(amount);
                }

                MutateEffectsRecursive(effectConstructors, innerEffect, amount, addProb * 0.9f, removeProb * 0.9f, visited);
            }

            if (outerEffect is SpawningGameFeelEffect spawnerEffect)
            {
                //Run mutate on the children.
                foreach (var innerEffect in spawnerEffect.ExecuteOnOffspring)
                {
                    if (!innerEffect.Lock)
                    {
                        innerEffect.Mutate(amount);
                    }

                    MutateEffectsRecursive(effectConstructors, innerEffect, amount, addProb * 0.9f, removeProb * 0.9f, visited);
                }
            }

            //TODO: Consider moving this to only unlocked effects 2020-08-20
            //Always allow a small probability for adding an extra effect?
            if (RandomExtensions.Boolean(addProb))
            {
                var instance = effectConstructors.GetRandomElement().Invoke();

                var isParticleSpawner = instance is ShatterEffect || instance is ParticlePuffEffect;

                //Don't add Shatter or Particle puff effects to other shatter/puff offspring
                if (isParticleSpawner && visited.Any(item => item is ShatterEffect || item is ParticlePuffEffect))
                {
                    //TODO: figure out if we should do anything else here. 2020-09-29
                }
                else
                {
                    //If it's a spawner effect, add it to the offspring effects instead.
                    if (outerEffect is SpawningGameFeelEffect spawner)
                    {
                        //If a destroy effect exists, don't add the effect unless it's a spawningEffect.
                        if (spawner.ExecuteOnOffspring.Any(item => item is DestroyEffect) || 
                            visited.Any(item => item is DestroyEffect))
                        {
                            if (instance is SpawningGameFeelEffect)
                            {
                                spawner.ExecuteOnOffspring.Add(instance); 
                            }
                        }
                        else
                        {
                            spawner.ExecuteOnOffspring.Add(instance);    
                        }
                    }
                    else
                    {
                        //If a destroy effect exists, don't add the effect unless it's a spawningEffect.
                        if (outerEffect.ExecuteAfterCompletion.Any(item => item is DestroyEffect) || 
                            visited.Any(item => item is DestroyEffect))
                        {
                            if (instance is SpawningGameFeelEffect)
                            {
                                outerEffect.ExecuteAfterCompletion.Add(instance); 
                            }
                        }
                        else
                        {
                            outerEffect.ExecuteAfterCompletion.Add(instance);
                        }
                    }
                }
            }

            if (!outerEffect.Lock)
            {
                outerEffect.Mutate(amount);
                
                //Check if this effect is looping infinitely...
                var loopingInfinitely = outerEffect.GetRemainingTime() == float.PositiveInfinity;

                if (loopingInfinitely)
                {
                    //Remove all execute after completion effects if looping infinitely.
                    outerEffect.ExecuteAfterCompletion.Clear();
                }
            }
        }
    }
}