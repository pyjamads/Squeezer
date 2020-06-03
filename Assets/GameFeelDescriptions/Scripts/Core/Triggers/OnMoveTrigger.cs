using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [Serializable]
    public class OnMoveTrigger : GameFeelTrigger
    {
        public OnMoveTrigger() : base(GameFeelTriggerType.OnMove) { }
        
        public enum MovementActivationType 
        {
            OnBeginMoving,
            WhileMoving,
            OnDirectionChange,
            OnStopMoving,
            WhileNotMoving,
            OnAnyStateChange, //For Step-through mode
        }

        public MovementActivationType type; 
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (var gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelMovementScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.type = type;
                description.attachedTriggers.Add(component);
            }
        }
    }
}