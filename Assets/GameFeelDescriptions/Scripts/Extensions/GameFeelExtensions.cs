using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class GameFeelExtensions
    {
        /// <summary>
        /// Allows adding effects to the offspring list.
        /// <code>
        /// ShakeEffect.OnComplete(BlinkEffect).OnComplete(ExplodeEffect);
        /// </code>
        /// </summary>
        /// <param name="spawner">The effect to wait for.</param>
        /// <param name="effect">The effect to execute afterwards.</param>
        /// <returns>Returns the added effect, for further chaining.</returns>
        public static void OnOffspring(this SpawningGameFeelEffect spawner, GameFeelEffect effect)
        {
            if (spawner.ExecuteOnOffspring == null)
            {
                spawner.ExecuteOnOffspring = new List<GameFeelEffect>();        
            }

            spawner.ExecuteOnOffspring.Add(effect);
        }
        
        /// <summary>
        /// Allows adding a list of effects to be executed after another effect is done.
        /// </summary>
        /// <param name="spawner"></param>
        /// <param name="effects"></param>
        public static void OnOffspring(this SpawningGameFeelEffect spawner, IEnumerable<GameFeelEffect> effects)
        {
            if (spawner.ExecuteOnOffspring == null)
            {
                spawner.ExecuteOnOffspring = new List<GameFeelEffect>();        
            }

            spawner.ExecuteOnOffspring.AddRange(effects);
        }
        
        /// <summary>
        /// Allows chaining effects together.
        /// <code>
        /// ShakeEffect.OnComplete(BlinkEffect).OnComplete(ExplodeEffect);
        /// </code>
        /// </summary>
        /// <param name="waitFor">The effect to wait for.</param>
        /// <param name="effect">The effect to execute afterwards.</param>
        /// <returns>Returns the added effect, for further chaining.</returns>
        public static GameFeelEffect OnComplete(this GameFeelEffect waitFor, GameFeelEffect effect)
        {
            if (waitFor.ExecuteAfterCompletion == null)
            {
                waitFor.ExecuteAfterCompletion = new List<GameFeelEffect>();        
            }

            waitFor.ExecuteAfterCompletion.Add(effect);
            
            return effect;
        }
        
        /// <summary>
        /// Allows adding a list of effects to be executed after another effect is done.
        /// </summary>
        /// <param name="waitFor"></param>
        /// <param name="effects"></param>
        public static void OnComplete(this GameFeelEffect waitFor, IEnumerable<GameFeelEffect> effects)
        {
            if (waitFor.ExecuteAfterCompletion == null)
            {
                waitFor.ExecuteAfterCompletion = new List<GameFeelEffect>();        
            }

            waitFor.ExecuteAfterCompletion.AddRange(effects);
        }
        
        public static void QueueExecution(this GameFeelEffect effect)
        {
            GameFeelEffectExecutor.Instance.QueueEffect(effect);
        }

        public static GameFeelEffect CurrentActiveEffect(this GameFeelEffect effect)
        {
            return GameFeelEffectExecutor.Instance.activeEffects.FirstOrDefault(effect.CompareTo);
        }
        
        public static IEnumerable<GameFeelEffect> CurrentActiveEffects(this GameFeelEffect effect)
        {
            return GameFeelEffectExecutor.Instance.activeEffects.Where(effect.CompareTo);
        }
        
        public static void StopExecution(this GameFeelEffect effect)
        {
            //Make sure to clean up any residue, created by the effect, before removing the effect.
            effect.ExecuteCleanUp();
            GameFeelEffectExecutor.Instance.RemoveEffect(effect);
        }


        public static Vector3 GetInteractionDirection(this CollisionData collisionData, bool normal = false)
        {
            var interactionDirection = Vector3.zero;
            switch (collisionData.ActivationType)
            {
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay:
                    if (normal)
                    {
                        interactionDirection = collisionData.Collision.impulse;
                    }
                    else
                    {
                        interactionDirection = collisionData.Collision.relativeVelocity;
                    }

                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D:
                    if (normal)
                    {
                        if (collisionData.Collision2D.contactCount > 0)
                        {
                            var contact = collisionData.Collision2D.GetContact(0);
                            interactionDirection = contact.normal * contact.normalImpulse;
                        }
                        else
                        {
                            //NOTE: this is a bad approximation
                            interactionDirection = collisionData.Collision2D.relativeVelocity;
                        }
                    }
                    else
                    {
                        interactionDirection = collisionData.Collision2D.relativeVelocity;
                    }
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay:
                    interactionDirection = collisionData.Origin.transform.position - collisionData.Collider.transform.position;
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D:
                    interactionDirection = collisionData.Origin.transform.position - collisionData.Collider2D.transform.position;
                    break;
            }

            return interactionDirection;
        }

        public static (Vector3 position, Vector3 normal) GetPositionAndNormal(this CollisionData collisionData, bool averagePosition = true, bool averageNormal = false)
        {
            var pos = Vector3.zero;
            var norm = Vector3.one;
            
            switch (collisionData.ActivationType)
            {
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay:

                    if (collisionData.Collision.contactCount > 0)
                    {
                        var list = new List<ContactPoint>();
                        var contacts = collisionData.Collision.GetContacts(list);
                        
                        if (averagePosition)
                        {
                            pos = list[0].point;
                            for (var index = 1; index < contacts; index++)
                            {
                                var contactPoint = list[index];
                                pos += contactPoint.point;
                            }

                            //Get the average position.
                            pos /= contacts;
                        }
                        else
                        {
                            pos = collisionData.Collision.GetContact(0).point;    
                        }

                        if (averageNormal)
                        {
                            norm = list[0].normal;
                            for (var index = 1; index < contacts; index++)
                            {
                                var contactPoint = list[index];
                                norm += contactPoint.normal;
                            }

                            //Get the average normal.
                            norm /= contacts;
                        }
                        else
                        {
                            norm = collisionData.Collision.GetContact(0).normal; 
                        }
                    }
                    else
                    {
                        pos = collisionData.Collision.collider.ClosestPointOnBounds(collisionData.Origin.transform.position);
                        norm = collisionData.Collision.impulse.normalized;    
                    }
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D:
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D:
                    if (collisionData.Collision2D.contactCount > 0)
                    {
                        var list = new List<ContactPoint2D>();
                        var contacts = collisionData.Collision2D.GetContacts(list);
                        
                        if (averagePosition)
                        {
                            pos = list[0].point;
                            for (var index = 1; index < contacts; index++)
                            {
                                var contactPoint = list[index];
                                pos += contactPoint.point.AsVector3();
                            }

                            //Get the average position.
                            pos /= contacts;
                        }
                        else
                        {
                            pos = collisionData.Collision.GetContact(0).point;    
                        }

                        if (averageNormal)
                        {
                            norm = list[0].normal;
                            for (var index = 1; index < contacts; index++)
                            {
                                var contactPoint = list[index];
                                norm += contactPoint.normal.AsVector3();
                            }

                            //Get the average normal.
                            norm /= contacts;
                        }
                        else
                        {
                            norm = collisionData.Collision2D.GetContact(0).normal;
                        }
                        
                    }
                    else
                    {
                        pos = collisionData.Collision2D.collider.ClosestPoint(collisionData.Origin.transform.position);
                        //NOTE: this is a bad approximation
                        norm = collisionData.Collision2D.relativeVelocity.normalized;     
                    }
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay:
                    //TODO: Consider getting the average position and normal between all contacts. 2020-09-10
                    pos = collisionData.Collider.ClosestPointOnBounds(collisionData.Origin.transform.position);
                    //NOTE: this is a bad approximation
                    norm = collisionData.Origin.transform.position - collisionData.Collider.transform.position;
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D:
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D:
                    //TODO: Consider getting the average position and normal between all contacts. 2020-09-10
                    pos = collisionData.Collider2D.ClosestPoint(collisionData.Origin.transform.position);
                    //NOTE: this is a bad approximation
                    norm = collisionData.Origin.transform.position - collisionData.Collider2D.transform.position;
                    break;
            }

            return (pos, norm);
        }
    }
}