using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    public class OnStartTrigger : GameFeelTrigger
    {
        public OnStartTrigger() : base(GameFeelTriggerType.OnStart) { }

        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (GameObject gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelStartScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                description.attachedTriggers.Add(component);
            }
        }
    }
}