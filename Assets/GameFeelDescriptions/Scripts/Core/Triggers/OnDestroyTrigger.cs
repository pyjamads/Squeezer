using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [Serializable]
    public class OnDestroyTrigger : GameFeelTrigger
    {
        public OnDestroyTrigger() : base(GameFeelTriggerType.OnDestroy) { }

        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (var gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelDestroyScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                description.attachedTriggers.Add(component);
            }
        }
    }
}