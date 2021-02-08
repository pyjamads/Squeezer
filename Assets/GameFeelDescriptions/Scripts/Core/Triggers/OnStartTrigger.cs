using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    public class OnStartTrigger : GameFeelTrigger
    {
        public OnStartTrigger() : base(GameFeelTriggerType.OnStart) { }

        [Header("Position offset, from transform.position")]
        public Vector3 localPositionOffset;
        
        [Header("Is position offset relative to transform.forward")]
        public bool useForwardForPositionOffset;
        
        [Header("Rotation offset, from transform.forward")]
        public Vector3 forwardRotationOffset;
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (GameObject gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelStartScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.localPositionOffset = localPositionOffset;
                component.useForwardForPositionOffset = useForwardForPositionOffset;
                component.forwardRotationOffset = forwardRotationOffset;
                description.attachedTriggers.Add(component);
            }
        }
    }
}