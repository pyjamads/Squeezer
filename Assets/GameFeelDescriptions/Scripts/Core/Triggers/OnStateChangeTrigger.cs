using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class OnStateChangeTrigger : GameFeelTrigger
    {
        public enum Conditionals
        {
            Equals,
            NotEquals,
            LessThan,
            GreaterThan,
            LessThanOrEquals,
            GreaterThanOrEquals,
        }
        
        [Header("The component containing the field or property.")]
        public string ComponentName;
        
        [Header("The field or property to track.")]
        public string StateField;
        
        [Header("Conditional operator to apply, when comparing values.")]
        public Conditionals Conditional;
        
        [Header("The value to use for the comparison. Leave blank for any change in value")]
        public string Value;
        
        [Header("Should the trigger react to a specific condition.")]
        public bool ReactToSpecificValue;
        
        [DisableFieldIf("ReactToSpecificValue", false)]
        [Header("Should the trigger active when achieving or loosing the specified condition.")]
        public bool ReactOnObtainingValue = true;
        
        public OnStateChangeTrigger() : base(GameFeelTriggerType.OnStateChanged) {  }
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (var gameObject in attachTo)
            {
                var component = gameObject.AddComponent<StateChangedTriggerScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.ComponentName = ComponentName;
                component.StateField = StateField;
                component.ReactToSpecificValue = ReactToSpecificValue;
                component.Conditional = Conditional;
                component.Value = Value;
                component.ReactOnObtainingValue = ReactOnObtainingValue;
                description.attachedTriggers.Add(component);
            }
        }
    }
}