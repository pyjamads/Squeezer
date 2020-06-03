using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [Serializable]
    public class OnRotateTrigger : GameFeelTrigger
    {
        public OnRotateTrigger() : base(GameFeelTriggerType.OnRotate) { }

        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (GameObject gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelRotationScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                description.attachedTriggers.Add(component);
            }
        }
    }
}