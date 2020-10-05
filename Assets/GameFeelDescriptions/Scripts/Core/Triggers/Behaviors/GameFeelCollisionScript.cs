using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFeelDescriptions
{
    public class GameFeelCollisionScript : GameFeelBehaviorBase
    {
        [FormerlySerializedAs("OtherTags")] 
        public List<string> ReactTo;

        public OnCollisionTrigger.CollisionActivationType type;

        private List<bool> isTag;
        
        public OnCollisionTrigger.CollisionContextType ContextType;
        
        private void Start()
        {
            SetupInitialTargets();
            CheckReactTo();
        }

        private void CheckReactTo()
        {
            isTag = new List<bool>();
            foreach (var str in ReactTo)
            {
                isTag.Add(Helpers.DoesTagExist(str));
            }
        }

        private void CheckForActivation(OnCollisionTrigger.CollisionActivationType activationType, GameObject other,
            GameFeelTriggerData collisionData)
        {
            if (Disabled) return;
            
            if (ReactTo.Count != isTag.Count)
            {
                //Just redo the check, if someone is modifying the list in the editor.
                CheckReactTo();
            }
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }

            //Match activationType to trigger type 
            switch (activationType)
            {
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionEnter ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllEnter:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionExit ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllExit:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionStay ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllStay:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllEnter:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllExit:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllStay:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerEnter ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllEnter:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerExit ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllExit:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerStay ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllStay:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllEnter:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllExit:
                    
                    break;
                case OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D when  
                    type == OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D ||
                    type == OnCollisionTrigger.CollisionActivationType.OnAllStay:
                    
                    break;
                default:
                    //No cases match the activation type, which means we don't react.
                    return;
            }

            //TODO: find bug, where two "tags" have been defined as !tags but it still hits one of them (wall fx in paddle)
            var shouldHandleEffect = ReactTo.Any(item => item.Contains("!"));
            for (var index = 0; index < ReactTo.Count; index++)
            {
                var reactTo = ReactTo[index];
                var checkTag = isTag[index];
                
                //Check for ! and *, and if those are not in the tag string, check the tag itself.
                if (reactTo.Contains("!"))
                {
                    if (checkTag && other.tag.Contains(reactTo.Substring(1)))
                    {
                        shouldHandleEffect = false;
                    }
                    else if(!checkTag && other.name.Contains(reactTo.Substring(1)))
                    {
                        shouldHandleEffect = false;
                    }
                }
                else
                {
                    if (reactTo.Contains("*"))
                    {
                        shouldHandleEffect = true;
                        break;
                    }

                    if (checkTag && other.CompareTag(reactTo))
                    {
                        shouldHandleEffect = true;
                        break;
                    }

                    if (!checkTag && other.name.Equals(reactTo))
                    {
                        shouldHandleEffect = true;
                        break;
                    }
                }
            }

            if (!shouldHandleEffect) return;
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode(activationType, other, collisionData);
            }
#endif
            
            //Direction passed to the Effect will be the direction from the source of collision towards self.
            //Alternative options are, relative velocity of the impact, and the separation impulse:
            //other.relativeVelocity
            //other.impulse
            for (var i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(Description.TriggerList[TriggerIndex].EffectGroups[i], 
                    activationType, other, collisionData);
#endif

                //We pass a normalized direction vector.
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], collisionData, true);
            }
        }
        
        

        //TODO: consider the implications of using collision normal vs position difference... 20/02/2020
        //TODO: maybe give direction between gameObject.transform.position and other.contacts[0]?

        #region Collider overloads

        private void OnCollisionEnter(Collision other)
        {
            foreach (ContactPoint contact in other.contacts)
            {
                Debug.DrawRay(contact.point, other.relativeVelocity.normalized, Color.white);
            }
            foreach (ContactPoint contact in other.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.red);
            }
            Vector3 context = Vector3.zero;

            switch (ContextType)
            {
                case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                    context = other.relativeVelocity;
                    break;
                case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                    context = other.GetContact(0).normal;
                    break;
                case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                    context = other.impulse;
                    break;
                case OnCollisionTrigger.CollisionContextType.FirstPoint:
                    context = other.GetContact(0).point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPoint:
                    context = other.GetContact(other.contactCount-1).point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                    context = other.GetContact(other.contactCount-1).normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionEnter, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionEnter,
                    Collision = other,
                });
        }

        private void OnCollisionStay(Collision other)
        {
            Vector3 context = Vector3.zero;

            switch (ContextType)
            {
                case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                    context = other.relativeVelocity;
                    break;
                case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                    context = other.GetContact(0).normal;
                    break;
                case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                    context = other.impulse;
                    break;
                case OnCollisionTrigger.CollisionContextType.FirstPoint:
                    context = other.GetContact(0).point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPoint:
                    context = other.GetContact(other.contactCount-1).point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                    context = other.GetContact(other.contactCount-1).normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionStay, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionStay,
                    Collision = other,
                });
        }

        private void OnCollisionExit(Collision other)
        {
            Vector3 context = other.relativeVelocity;

            if (other.contactCount > 0)
            {
                var contactPoint2D = other.GetContact(0);
                var lastContactPoint2D = other.GetContact(other.contactCount-1);
                switch (ContextType)
                {
                    case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                        context = other.relativeVelocity;
                        break;
                    case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                        context = contactPoint2D.normal;
                        break;
                    case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                        context = other.impulse;
                        break;
                    case OnCollisionTrigger.CollisionContextType.FirstPoint:
                        context = contactPoint2D.point;
                        break;
                    case OnCollisionTrigger.CollisionContextType.LastPoint:
                        context = lastContactPoint2D.point;
                        break;
                    case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                        context = lastContactPoint2D.normal;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionExit, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionExit,
                    Collision = other,
                });
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            
            foreach (ContactPoint2D contact in other.contacts)
            {
                Debug.DrawRay(contact.point, contact.relativeVelocity.normalized, Color.white);
            }
            foreach (ContactPoint2D contact in other.contacts)
            {
                Debug.DrawRay(contact.point, contact.normal, Color.red);
            }
            
            Vector3 context = Vector3.zero;

            var contactPoint2D = other.GetContact(0);
            var lastContactPoint2D = other.GetContact(other.contactCount-1);
            switch (ContextType)
            {
                case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                    context = other.relativeVelocity;
                    break;
                case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                    context = contactPoint2D.normal;
                    break;
                case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                    context = contactPoint2D.normalImpulse * contactPoint2D.normal + Vector2.Perpendicular(contactPoint2D.normal) * contactPoint2D.tangentImpulse;
                    break;
                case OnCollisionTrigger.CollisionContextType.FirstPoint:
                    context = contactPoint2D.point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPoint:
                    context = lastContactPoint2D.point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                    context = lastContactPoint2D.normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D,
                    Collision2D = other,
                });
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            Vector3 context = Vector3.zero;

            var contactPoint2D = other.GetContact(0);
            var lastContactPoint2D = other.GetContact(other.contactCount-1);
            switch (ContextType)
            {
                case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                    context = other.relativeVelocity;
                    break;
                case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                    context = contactPoint2D.normal;
                    break;
                case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                    context = contactPoint2D.normalImpulse * contactPoint2D.normal + Vector2.Perpendicular(contactPoint2D.normal) * contactPoint2D.tangentImpulse;
                    break;
                case OnCollisionTrigger.CollisionContextType.FirstPoint:
                    context = contactPoint2D.point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPoint:
                    context = lastContactPoint2D.point;
                    break;
                case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                    context = lastContactPoint2D.normal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D,
                    Collision2D = other,
                });
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            Vector3 context = other.relativeVelocity;

            if (other.contactCount > 0)
            {
                var contactPoint2D = other.GetContact(0);
                var lastContactPoint2D = other.GetContact(other.contactCount-1);
                switch (ContextType)
                {
                    case OnCollisionTrigger.CollisionContextType.RelativeVelocity:
                        context = other.relativeVelocity;
                        break;
                    case OnCollisionTrigger.CollisionContextType.CollisionNormal:
                        context = contactPoint2D.normal;
                        break;
                    case OnCollisionTrigger.CollisionContextType.SeparationImpulse:
                        context = contactPoint2D.normalImpulse * contactPoint2D.normal + Vector2.Perpendicular(contactPoint2D.normal) * contactPoint2D.tangentImpulse;
                        break;
                    case OnCollisionTrigger.CollisionContextType.FirstPoint:
                        context = contactPoint2D.point;
                        break;
                    case OnCollisionTrigger.CollisionContextType.LastPoint:
                        context = lastContactPoint2D.point;
                        break;
                    case OnCollisionTrigger.CollisionContextType.LastPointNormal:
                        context = lastContactPoint2D.normal;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D,
                    Collision2D = other,
                });
        }

        //TODO: Get rigidbody and figure out trigger collision contexts like relativeVelocity, etc on our own here! 2020-08-27
        
        private void OnTriggerEnter(Collider other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position);

            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerEnter, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerEnter,
                    Collider = other,
                });
        }

        private void OnTriggerStay(Collider other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position);
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerStay, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerStay,
                    Collider = other,
                });
        }

        private void OnTriggerExit(Collider other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position);
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerExit, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerExit,
                    Collider = other,
                });
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3();
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D,
                    Collider2D = other,
                });
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3();
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D,
                    Collider2D = other,
                });
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            Vector3 context = gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3();
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D, other.gameObject,
                new CollisionData{Origin = gameObject, 
                    ActivationType = OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D,
                    Collider2D = other,
                });
        }

        #endregion
    }
}