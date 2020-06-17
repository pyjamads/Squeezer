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
        
        private void Start()
        {
            SetupInitialTargets(true);
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
            Vector3 direction)
        {
            if (ReactTo.Count != isTag.Count)
            {
                //Just redo the check, if someone is modifying the list in the editor.
                CheckReactTo();
            }
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets(true);
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
                HandleStepThroughMode(activationType, other, direction);
            }
#endif
            
            //Direction passed to the Effect will be the direction from the source of collision towards self.
            //Alternative options are, relative velocity of the impact, and the separation impulse:
            //other.relativeVelocity
            //other.impulse
            for (var i = 0; i < EffectGroups.Count; i++)
            {
                //var direction = other.transform.position - transform.position;
                if (EffectGroups[i].AppliesTo == GameFeelTarget.Other)
                {
                    targets[i].Clear();
                    targets[i].Add(other);
                }
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(Description.TriggerList[TriggerIndex].EffectGroups[i], 
                    activationType, other, direction);
#endif

                //We pass a normalized direction vector.
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], direction, true);
            }
        }
        
        

        //TODO: consider the implications of using collision normal vs position difference... 20/02/2020
        //TODO: maybe give direction between gameObject.transform.position and other.contacts[0]?

        #region Collider overloads

        private void OnCollisionEnter(Collision other)
        {
            foreach (ContactPoint contact in other.contacts)
            {
                Debug.DrawRay(contact.point, other.relativeVelocity, Color.white);
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionEnter, other.gameObject,
                other.relativeVelocity);
        }

        private void OnCollisionStay(Collision other)
        {
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionStay, other.gameObject,
                other.relativeVelocity);
        }

        private void OnCollisionExit(Collision other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionExit, other.gameObject,
                other.relativeVelocity);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            
            foreach (ContactPoint2D contact in other.contacts)
            {
                Debug.DrawRay(contact.point, contact.relativeVelocity, Color.white);
            }
            
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D, other.gameObject,
                other.relativeVelocity);
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D, other.gameObject,
                other.relativeVelocity);
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D, other.gameObject,
                other.relativeVelocity);
        }

        //TODO: Get rigidbody and figure out "relativeVelocity" on our own here!
        
        private void OnTriggerEnter(Collider other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerEnter, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position));
        }

        private void OnTriggerStay(Collider other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerStay, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position));
        }

        private void OnTriggerExit(Collider other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerExit, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3());
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3());
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            CheckForActivation(OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D, other.gameObject,
                gameObject.transform.position - other.ClosestPoint(gameObject.transform.position).AsVector3());
        }

        #endregion
    }
}