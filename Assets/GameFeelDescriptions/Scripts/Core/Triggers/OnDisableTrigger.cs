using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [Serializable]
    public class OnDisableTrigger : GameFeelTrigger
    {
        public OnDisableTrigger() : base(GameFeelTriggerType.OnDisable) { }
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (var gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelDisableScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                description.attachedTriggers.Add(component);
            }
        }
    }
}