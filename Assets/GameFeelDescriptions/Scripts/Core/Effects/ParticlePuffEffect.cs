using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class ParticlePuffEffect : SpawningGameFeelEffect
    {
        public ParticlePuffEffect()
        {
            Description = "Particle puff creates a cloud of particles emanating from a point/area, in a selected shape.";
            
            //NOTE: these probably have to be added during construction!
            //TODO: Add rotation, scaling, coloring and velocity over the lifetime 2020-09-16
            
            //TODO: randomize rotation per "particle", ie. have a "mutateOnCopy" flag and an amount? 2020-09-22
            var rotation = new ContinuousRotationEffect();
            rotation.RotationPerSecond = Random.onUnitSphere * (Mathf.PI - Random.value * 2 * Mathf.PI) * Mathf.Rad2Deg;
            rotation.RandomizeInitialRotation = true;
            rotation.Duration = 1f;
            rotation.loopType = DurationalGameFeelEffect.LoopType.Restart;
            rotation.repeat = -1;
            this.OnOffspring(rotation);
            
            var scale = new ScaleEffect();
            scale.relative = true;
            scale.to = Vector3.one * (Random.value * 2);
            scale.Delay = Random.Range(0, 0.3f);
            scale.RandomizeDelay = true;
            scale.Duration = Random.Range(0.3f, 1.3f);
            this.OnOffspring(scale);
            
            var color = new MaterialColorChangeEffect();
            color.setFromValue = true;
            color.@from = Random.ColorHSV();
            color.to = color.@from.withA(0); //Maybe use Color.clear instead.
            color.RandomizeDuration = true;
            color.DurationMin = 0.3f;
            color.Duration = Random.Range(0.4f, 1.6f);
            this.OnOffspring(color);
            
            color.OnComplete(new DestroyEffect());

            //TODO: maybe have a lifetime value, that adjusts the Durations above. 2020-09-23
            //TODO: adjust velocity over time... 2020-09-16 
        }

        public enum PuffShapes
        {
            SemiSphere, //Normal based half sphere
            InverseSemiSphere, //Inverted Normal based half sphere
            Cylinder, //Normal based half sphere, with extra height.
            Sphere, //Sphere.
            ConeFlat, //Cone shape, expanding in normal direction
            ConeRound, //Cone shape, expanding in normal direction
            InverseCone, //Inverted Cone shape, pointy end in normal direction
        }

        [Tooltip(
            "Number of Pieces to spawn, if amount is different from the items in the list, they will be randomly chosen.")]
        public int AmountOfParticles = Random.Range(5, 10);

        [HideFieldIf("usePrimitiveParticles", true)]
        [Tooltip("Add custom prefabs to spawn instead of the primitives.")]
        public List<GameObject> ParticlePrefabs;

        public bool usePrimitiveParticles = true;

        [HideFieldIf("usePrimitiveParticles", false)]
        public PrimitiveType ParticlePrimitive;

        //TODO: make this scale "exponential" in nature,
        //TODO: 1-3 is the footfall to land like effects,
        //TODO: 4-6 also adds shaking and color changes etc,
        //TODO: 7-9 debris and positionalFlashes,
        //TODO: 10 NUKE!
        // [Range(1, 10)] 
        // public int Magnitude = Random.Range(1, 11);
        
        [Tooltip("The shape of the particle blast")]
        public PuffShapes ExpansionShape = EnumExtensions.GetRandomValue<PuffShapes>();
        
        [DisableFieldIf("setParticleForward", true)]
        [Header("Set the particle's transform.up to the expansion direction.")]
        public bool setParticleUp = true;
        
        [DisableFieldIf("setParticleUp", true)]
        [Header("Set the particle's transform.forward to the expansion direction.")]
        public bool setParticleForward;
        
        [Header("The size of the area to spawn particles in.")]
        public Vector3 Area = Vector3.zero;
        
        [Tooltip("The height of the shape, in the normal direction")]
        [AdjustableRange(0.01f, 5f, lockMin = true)]
        public float Height = Random.Range(0.1f, 2f);
        
        [AdjustableRange(0.01f, 5f, lockMin = true)]
        public float Radius = Random.Range(0.1f, 2f);

        //Size of the puff, max size determined by Magnitude
        //[DynamicRange(0.01f, "Magnitude")]
        [AdjustableRange(0.01f, 5f, lockMin = true)]
        public float ParticleScale = Random.Range(0.1f, 1.5f);
        
        //Duration slider min/max duration can be adjusted, but it can never be set to a negative number.
        [AdjustableRange(0.01f, 1f, lockMin = true)]
        public float ParticleLifetime = Random.Range(0.1f, 1.5f);


        public override void Mutate(float amount = 0.05f)
        {
            base.Mutate(amount);

            if (RandomExtensions.Boolean())
            {
                AmountOfParticles = Mathf.Max(1, AmountOfParticles + RandomExtensions.Sign() * Mathf.CeilToInt(AmountOfParticles * amount));
            }

            if (RandomExtensions.Boolean(amount))
            {
                ParticlePrimitive = EnumExtensions.GetRandomValue(
                    new List<PrimitiveType> //Remove 2D objects for now. 2020-09-28
                    {
                        PrimitiveType.Plane, 
                        PrimitiveType.Quad
                    });    
            }

            if (RandomExtensions.Boolean(amount))
            {
                ExpansionShape = EnumExtensions.GetRandomValue<PuffShapes>();
            }

            if (RandomExtensions.Boolean(amount))
            {
                setParticleForward = !setParticleForward;
            }

            if (RandomExtensions.Boolean(amount))
            {
                setParticleUp = !setParticleUp;
            }

            if (RandomExtensions.Boolean())
            {
                Height = Mathf.Max(0.1f, Height + RandomExtensions.MutationAmount(amount));    
            }

            if (RandomExtensions.Boolean())
            {
                Radius = Mathf.Max(0.1f, Radius + RandomExtensions.MutationAmount(amount));
            }

            if (RandomExtensions.Boolean())
            {
                ParticleScale = Mathf.Max(0.1f, ParticleScale + RandomExtensions.MutationAmount(amount));    
            }

            if (RandomExtensions.Boolean())
            {
                ParticleLifetime = Mathf.Max(0.1f, ParticleLifetime + RandomExtensions.MutationAmount(amount));
            }
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new ParticlePuffEffect();
            
            cp.ParticlePrefabs = ParticlePrefabs;
            cp.AmountOfParticles = AmountOfParticles;
            cp.usePrimitiveParticles = usePrimitiveParticles;
            cp.ParticlePrimitive = ParticlePrimitive;

            cp.ExpansionShape = ExpansionShape;
            cp.setParticleUp = setParticleUp;
            cp.setParticleForward = setParticleForward;
            cp.Area = Area;
            cp.Height = Height;
            cp.Radius = Radius;
            cp.ParticleScale = ParticleScale;
            cp.ParticleLifetime = ParticleLifetime;

            if (target == null && origin == null)
            {
                cp.targetPos = Vector3.zero;
            }
            else
            {
                cp.targetPos = target != null ? target.transform.position : origin.transform.position;    
            }
                
            cp.Init(origin, target, triggerData);
            
            return DeepCopy(cp, ignoreCooldown);
        }

        //TODO: This Effect might need it's own custom "editor" like the SFXR editor... to generate the various kinds of puffs, and mutate/randomize 2020-09-16
        //TODO: just like we're generating simple sounds, we might need to generate simple textures / shaders for the particles! 2020-09-16
        //NOTE: The generated effects might have several layers of particlePuffs, each with their own size, intensity and type... 
        
        protected override bool ExecuteTick()
        {
            //STEP 1: Get position and direction!
            var position = Vector3.zero;
            var normal = Vector3.zero;
            
            //NOTE: Use target position as fallback.
            if (target != null)
            {
                position = target.transform.position;
            }
            else
            {
                position = targetPos;
            }
            
            //If triggerData provides position and normal, use those.
            if (triggerData is CollisionData collisionData)
            {
                //NOTE: For "OnTriggerEnter/Exit/Stay" collisions,
                //this returns a point on the bounds + the direction between the center of the two colliders.
                (position, normal) = collisionData.GetPositionAndNormal();

                //If it's a 2D collision set the z axis from the target Position.
                if (collisionData.wasCollision2D())
                {
                    position.z = target != null ? target.transform.position.z : targetPos.z;
                }
                
                //NOTE: this normalization might not be necessary.
                normal.Normalize();
            }
            else if (triggerData is PositionalData positionalData)
            {
                position = positionalData.Position;
                
                //NOTE: this normalization might not be necessary.
                normal = positionalData.DirectionDelta.normalized;
            }
            else if (triggerData is DirectionalData directionalData)
            {
                //NOTE: this normalization might not be necessary.
                normal = directionalData.DirectionDelta.normalized;
            }
            
            //Make triggerData for particles
            //var particleTriggerData = new DirectionalData(normal){ InCollisionUpdate = triggerData.InCollisionUpdate };
            
            //STEP 2: Spawn primitives or prefabs.
            var particles = new List<GameObject>();
            
            //Scale particles and movement.
            var scale = Vector3.one * ParticleScale;
            
            //If there's any prefabs instantiate them instead of copies of the target.
            if (!usePrimitiveParticles && ParticlePrefabs?.Count > 0)
            {
                for (int i = 0; i < AmountOfParticles; i++)
                {
                    if (AmountOfParticles != ParticlePrefabs.Count)
                    {
                        var particle = Object.Instantiate(ParticlePrefabs.GetRandomElement(), GameFeelEffectExecutor.Instance.transform);
                        particle.transform.position = position;
                        particles.Add(particle);
                    }
                    else
                    {
                        var particle = Object.Instantiate(ParticlePrefabs[i], GameFeelEffectExecutor.Instance.transform);
                        particle.transform.position = position;
                        particles.Add(particle);
                    }
                }
            }
            else
            {
                if (!usePrimitiveParticles)
                {
                    Debug.LogWarning("ParticlePuffEffect targeting ["+target.name+"] is set to use prefabs, but no prefabs have been added, using primitive instead.");
                }
                
                var mold = GameObject.CreatePrimitive(ParticlePrimitive);
                mold.transform.parent = GameFeelEffectExecutor.Instance.transform;
                mold.transform.position = position;
                
                //TODO: figure out how to make better primitive particles,
                //TODO: because having the collider causes a physics separation impulse when the objects are spawned, 
                //TODO: and will also disrupt physics based simulations ... 2020-09-17
                if (triggerData.InCollisionUpdate)
                {
                    GameObject.Destroy(mold.GetComponent<Collider>());
                }
                else
                {
                    GameObject.DestroyImmediate(mold.GetComponent<Collider>());
                }

                var renderer = mold.GetComponent<Renderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
                SetMaterialTransparentBlendMode(renderer.sharedMaterial);
                renderer.sharedMaterial.color = Color.white;

                mold.transform.localScale = scale;

                particles.Add(mold);

                //Copy the rest off of the first copy.
                for (var i = 1; i < AmountOfParticles; i++)
                {
                    var particle = Object.Instantiate(mold, GameFeelEffectExecutor.Instance.transform, true);

                    if (triggerData.InCollisionUpdate)
                    {
                        GameObject.Destroy(particle.GetComponent<Collider>());
                    }

                    //NOTE: collider is NOT copied, when it's already been marked for destruction in the mold. 2020-09-18
                    //So no need to remove it here, also no impact separation should happen between these copies. 
                    //GameObject.Destroy(particle.GetComponent<Collider>());
                    
                    particles.Add(particle);
                }
            }

            // var xRange = Random.Range(0, normal.x * scale.x);
            // var yRange = Random.Range(0, normal.y * scale.y);
            // var zRange = Random.Range(0, normal.z * scale.z);

            foreach (var particle in particles)
            {
                //Adjust position randomly based on the original position and the scale of the objects!
                // var pos = new Vector3(
                //     position.x + Random.Range(0, xRange),  
                //     position.y + Random.Range(0, yRange),
                //     position.z + Random.Range(0, zRange)
                // );
                    
                //particle.transform.position = pos;

                //STEP 3: Get random directions on the tangent plane from position + direction (maybe add a fraction of the direction vector as well), and set them on their way
                //Normal direction Fallback: Random.onUnitSphere per piece
                //Add the movement either as a translation effect
                var translate = new TranslateEffect();
                translate.relative = true;
                
                var direction = Random.onUnitSphere * Radius;

                //NOTE: Sphere and SemiSphere does not take Height into account.
                switch (ExpansionShape)
                {
                    case PuffShapes.SemiSphere:
                    {
                        //If the direction is opposite the normal, flip it!
                        if (Vector3.Dot(direction, normal) < 0)
                        {
                            direction *= -1f;
                        }
                        
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the Normal, based on the projection onto the normal (meaning scale more towards the center).
                        var normalScaled = normal * dot;
                        
                        //Adjust the scaling based on the height.
                        normalScaled *= Height - 1f;
                        
                        direction += normalScaled;

                        break;
                    }
                    case PuffShapes.Sphere:
                    {
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the Normal, based on the projection onto the normal (meaning scale more towards the center).
                        var normalScaled = normal * dot;
                        
                        //Adjust the scaling based on the height.
                        normalScaled *= Height - 1f;
                        
                        direction += normalScaled;

                        break;
                    }
                    case PuffShapes.InverseSemiSphere:
                    {
                        //If the direction is same as the normal, flip it!
                        if (Vector3.Dot(direction, normal) > 0)
                        {
                            direction *= -1f;
                        }
                        
                        //Then move it up by the radius.
                        var normalScaled = Radius * normal;
                        direction += normalScaled;
                        
                        //Lastly squish it by the dot projection.
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the Normal, based on the projection onto the normal (meaning scale more towards the center).
                        normalScaled = normal * dot;
                        
                        //Adjust the scaling based on the height.
                        normalScaled *= Height - 1f;
                        
                        direction += normalScaled;

                        break;
                    }
                    //TODO: maybe make this flat wide semi circle an option as well! 2020-09-23
                    // case PuffShapes.: //Flat-Wide semi circle..
                    // {
                    //     //If the direction is opposite the normal, flip it!
                    //     if (Vector3.Dot(direction, normal) < 0)
                    //     {
                    //         direction *= -1f;
                    //     }
                    //     
                    //     var dot = Vector3.Dot(direction, normal);
                    //     
                    //     //Scale the Normal, based on the projection onto the normal (meaning scale more towards the center).
                    //     var normalScaled = normal * dot;
                    //     
                    //     //Adjust the scaling based on the height.
                    //     normalScaled *= (Height - dot) * Radius; //this plus R=2 and H=1 results in reverseSemiSphere
                    //
                    //     direction += normalScaled;
                    //
                    //     break;
                    // }
                    case PuffShapes.Cylinder:
                    {
                        //If the direction is opposite the normal, flip it!
                        if (Vector3.Dot(direction, normal) < 0)
                        {
                            direction *= -1f;
                        }

                        //First make the semi-sphere into a disc. 
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the normal based on the directions projection onto the normal
                        //We add this to the directions, to make the shape completely flat, at the "Height". 
                        var normalScaled = normal * (Height - dot);
                        direction += normalScaled;

                        //And remove a random amount of normal * height, this makes a Cylinder shape with a flat top.
                        direction -= normal * Height * Random.value;

                        break;
                    }
                    case PuffShapes.ConeFlat:
                    {
                        //If the direction is opposite the normal, flip it!
                        if (Vector3.Dot(direction, normal) < 0)
                        {
                            direction *= -1f;
                        }
                        
                        //First make the semi-sphere into a disc. 
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the normal based on the directions projection onto the normal
                        //We add this to the directions, to make the shape completely flat, at the bottom. 
                        var normalScaled = normal * -dot;
                        direction += normalScaled;

                        //Move up to the height, based on how close it is to the center.
                        direction += normal * Height * direction.magnitude + normal * ParticleScale * 0.5f;
                        
                        break;
                    }
                    case PuffShapes.ConeRound:
                    {
                        //If the direction is opposite the normal, flip it!
                        if (Vector3.Dot(direction, normal) < 0)
                        {
                            direction *= -1f;
                        }
                        
                        //And add the normal, this makes a small semi-sphere at the end of the normal,
                        //which makes a cone with a semi-circle at the top.
                        direction += normal * Height;
                        
                        //Make some of the directions not reach the full distance.
                        direction *= RandomExtensions.Boolean(0.7f) ? Random.Range(0.1f, 1f) : 1f;
                        
                        break;
                    }
                    case PuffShapes.InverseCone:
                    {
                        //If the direction is opposite the normal, flip it!
                        if (Vector3.Dot(direction, normal) < 0)
                        {
                            direction *= -1f;
                        }
                        
                        //First make the semi-sphere into a disc. 
                        var dot = Vector3.Dot(direction, normal);
                        
                        //Scale the normal based on the directions projection onto the normal
                        //We add this to the directions, to make the shape completely flat, at the bottom. 
                        var normalScaled = normal * -dot;
                        direction += normalScaled;

                        //Move up to the height, based on the dot product.
                        direction += normal * Height * (Radius - direction.magnitude) + normal * ParticleScale * 0.5f;
                        
                        //TODO: get more of the center points out to the edge 2020-09-23

                        break;
                    }
                }
                
                //Randomize spawn position in the spawn area.
                var bounds = new Bounds(position, Area);
                var areaPos = RandomExtensions.PositionInBounds(bounds);

                //Check against direction, so they spawn on the same side as the direction they're going.
                var diff = areaPos - position;
                
                //Align it to the same quadrant as the direction.
                areaPos = position + diff.QuadrantAlign(direction);

                //Set the new position.
                particle.transform.position = areaPos;
                
                //Also scale direction by the cube root and the user determined size scale.
                translate.to = direction;

                if (setParticleUp)
                {
                    //Set initial rotation based on direction.
                    particle.transform.up = direction;    
                }
                else if (setParticleForward)
                {
                    //Set initial rotation based on direction.
                    particle.transform.forward = direction;
                }

                //translate.RandomizeDelay = true;
                translate.Delay = Random.Range(0, 0.1f * ParticleLifetime);
                
                // translate.RandomizeDuration = true;
                // translate.DurationMin = 0.5f * ParticleLifetime;
                translate.Duration = ParticleLifetime - translate.Delay;
                    
                //TODO: Make better easing curve, that starts fast and slows down over time... 2020-09-17
                translate.easing = EasingHelper.EaseType.QuadOut;

                //Maybe destroy it as well.
                //translate.OnComplete(new DestroyEffect());
                
                Squeezer.Trigger(translate, particle, direction);
                
                //var translateTriggerData = new PositionalData (particle.transform.position, direction) { InCollisionUpdate = triggerData.InCollisionUpdate };
                // translate.Init(origin, particle, translateTriggerData);
                // translate.SetElapsed();
                // translate.QueueExecution();
                
                //NOTE: If we do this instead of queuing them all, the waitForAbove won't wait for all particles to finish.
                //NOTE: However, now each particle will also transmit it's own direction down the tree. 2020-10-22 
                QueueOffspringEffects(particle, new DirectionalData(direction){ InCollisionUpdate = triggerData.InCollisionUpdate });
            }

            //STEP 4: Queue the rest of the effects on all particles!
            //QueueOffspringEffects(particles, particleTriggerData);
            
            //We're done!
            return true;
        }
    }
}