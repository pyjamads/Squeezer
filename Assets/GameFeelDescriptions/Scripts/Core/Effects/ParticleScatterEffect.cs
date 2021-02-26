using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class ParticleScatterEffect : SpawningGameFeelEffect
    {
        public ParticleScatterEffect()
        {
            Description = "Particle scatter creates an amount of particles in a scatter pattern based on the direction of the interaction and the angle settings.";
            
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
            //
            // var destroy = new DestroyEffect();
            // destroy.Delay = color.Duration;
            // destroy.RandomizeDelay = RandomExtensions.Boolean();
            // this.OnOffspring(destroy);

            //TODO: maybe have a lifetime value, that adjusts the Durations above. 2020-09-23
            //TODO: adjust velocity over time... 2020-09-16 
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

        [DisableFieldIf("setParticleForward", true)]
        [Header("Set the particle's transform.up to the expansion direction.")]
        public bool setParticleUp = true;
        
        [DisableFieldIf("setParticleUp", true)]
        [Header("Set the particle's transform.forward to the expansion direction.")]
        public bool setParticleForward;
        
        //[AdjustableRange(0, 360, lockMin = true, lockMax = true)]
        public Vector3 ScatterAngle = Vector3.one * Random.Range(0, 360);

        [Header("The size of the area to spawn particles in.")]
        public Vector3 Area = Vector3.zero;

        [Header("The initial speed of the particle.")]
        [AdjustableRange(0.01f, 5f, lockMin = true)]
        public float Speed = Random.Range(0.1f, 2f);
        
        [Header("The deceleration of the particle each time step. (negative values = acceleration)")]
        [AdjustableRange(-0.1f, 0.1f)]
        public float Drag = Random.Range(0.02f, 0.1f);

        public bool ApplyGravity = false;

        //Size of the puff, max size determined by Magnitude
        //[DynamicRange(0.01f, "Magnitude")]
        [AdjustableRange(0.01f, 5f, lockMin = true)]
        public float ParticleScale = Random.Range(0.1f, 1.5f);
        
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
                setParticleForward = !setParticleForward;
            }

            if (RandomExtensions.Boolean(amount))
            {
                setParticleUp = !setParticleUp;
            }

            if (RandomExtensions.Boolean())
            {
                Speed = Mathf.Max(0.01f, Speed + RandomExtensions.MutationAmount(amount));    
            }

            if (RandomExtensions.Boolean())
            {
                Drag = Mathf.Max(-1f, Drag + RandomExtensions.MutationAmount(amount));
            }

            if (RandomExtensions.Boolean())
            {
                ParticleScale = Mathf.Max(0.1f, ParticleScale + RandomExtensions.MutationAmount(amount));    
            }

            if (RandomExtensions.Boolean())
            {
                ScatterAngle.x = Mathf.Max(0, Mathf.RoundToInt(ScatterAngle.x + RandomExtensions.MutationAmount(amount, ScatterAngle.x)));
                ScatterAngle.y = Mathf.Max(0, Mathf.RoundToInt(ScatterAngle.y + RandomExtensions.MutationAmount(amount, ScatterAngle.y)));
                ScatterAngle.z = Mathf.Max(0, Mathf.RoundToInt(ScatterAngle.z + RandomExtensions.MutationAmount(amount, ScatterAngle.z)));
            }
        }

        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData)
        {
            var cp = new ParticleScatterEffect();
            
            cp.ParticlePrefabs = ParticlePrefabs;
            cp.AmountOfParticles = AmountOfParticles;
            cp.usePrimitiveParticles = usePrimitiveParticles;
            cp.ParticlePrimitive = ParticlePrimitive;
            
            cp.setParticleUp = setParticleUp;
            cp.setParticleForward = setParticleForward;
            cp.Area = Area;
            cp.Speed = Speed;
            cp.Drag = Drag;
            cp.ApplyGravity = ApplyGravity;
            cp.ScatterAngle = ScatterAngle;
            cp.ParticleScale = ParticleScale;

            
            

            if (target != null)
            {
                cp.targetPos = target.transform.position;
            }
                
            cp.Init(origin, target, triggerData);
            
            return DeepCopy(cp);
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
                // var translate = new TranslateEffect();
                // translate.relative = true;

                var angleX = RandomExtensions.MutationAmount(ScatterAngle.x/2f);
                var angleY = RandomExtensions.MutationAmount(ScatterAngle.y/2f);
                var angleZ = RandomExtensions.MutationAmount(ScatterAngle.z/2f);
                var quat = Quaternion.Euler(angleX, angleY, angleZ);
                
                var direction = quat * normal;

                //Randomize spawn position in the spawn area.
                var bounds = new Bounds(position, Area);
                var areaPos = RandomExtensions.PositionInBounds(bounds);

                //Check against direction, so they spawn on the same side as the direction they're going.
                // var diff = areaPos - position;
                
                //Align it to the same quadrant as the direction.
                // areaPos = position + diff.QuadrantAlign(direction);

                //Set the new position.
                particle.transform.position = areaPos;
                
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

                var movement = particle.AddComponent<SimplePhysicsMovement>();
                movement.Velocity = direction * (Speed * Random.Range(0.5f, 1f));
                movement.Drag = Drag;
                movement.TimeScaleIndependent = UnscaledTime;
                movement.ApplyGravity = ApplyGravity;
                
                
                // //Also scale direction by the cube root and the user determined size scale.
                // translate.to = direction;
                //
                // //translate.RandomizeDelay = true;
                // //translate.Delay = Random.Range(0, 0.1f * ParticleLifetime);
                //
                // // translate.RandomizeDuration = true;
                // // translate.DurationMin = 0.5f * ParticleLifetime;
                // translate.Duration = Random.Range(0.75f, 1.5f); //TODO: remove this, and add the speed thingy instead!
                //     
                // //TODO: Make better easing curve, that starts fast and slows down over time... 2020-09-17
                // translate.easing = EasingHelper.EaseType.QuadOut;
                //
                // //Maybe destroy it as well.
                // //translate.OnComplete(new DestroyEffect());
                // var translateTriggerData = new PositionalData (particle.transform.position, direction) { InCollisionUpdate = triggerData.InCollisionUpdate };
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