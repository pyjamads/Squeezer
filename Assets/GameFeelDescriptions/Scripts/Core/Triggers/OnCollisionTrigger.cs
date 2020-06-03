using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [Serializable]
    public class OnCollisionTrigger : GameFeelTrigger
    {
        public OnCollisionTrigger() : base(GameFeelTriggerType.OnCollision) { }
        
        public enum CollisionActivationType
        {
            OnAllEnter,
            OnAllExit,
            OnAllStay,
            OnCollisionEnter,
            OnCollisionExit,
            OnCollisionStay,
            OnCollisionEnter2D,
            OnCollisionExit2D,
            OnCollisionStay2D,
            OnTriggerEnter,
            OnTriggerExit,
            OnTriggerStay,
            OnTriggerEnter2D,
            OnTriggerExit2D,
            OnTriggerStay2D,
        }
        
        /// <summary>
        /// The tag for other objects, used in OnCollision triggers.
        /// </summary>
        [FormerlySerializedAs("OtherTags")] 
        public List<string> ReactTo = new List<string>{"*"};
        //TODO: rename to CollideWithTags or CollisionTags 20/04/2020
        //public GameObject[] ListenOtherObject;

        /// <summary>
        /// Which type of collider activation do you want.
        /// </summary>
        public CollisionActivationType type;
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {            
            if (ReactTo == null || ReactTo.Count == 0)
            {
                Debug.LogException(new Exception("OnCollisionTrigger: No OtherTags defined."));
                return;
            }

            foreach (var gameObject in attachTo)
            {
                if (gameObject.GetComponent<Collider>() != null && gameObject.GetComponent<Collider2D>() != null)
                {
                    Debug.LogWarning(gameObject.name +
                                     ": GameObject missing Collider or Collider2D component. GameFeelCollisionScript not attached!");
                    continue;
                }
                
                var component = gameObject.AddComponent<GameFeelCollisionScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.ReactTo = ReactTo;
                component.type = type;
                description.attachedTriggers.Add(component);
            }
        }
    }
}