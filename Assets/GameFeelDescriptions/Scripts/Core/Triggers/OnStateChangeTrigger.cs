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
        
        [Header("The script containing the field (on GameObjects or Prefabs).")]
        public MonoBehaviour selectedComponent;

        [Header("The field to track.")]
        [MemberInfoSelector("selectedComponent")]
        public string field;
        
        [Header("Conditional operator to apply, when comparing values.")]
        public Conditionals Conditional;
        
        [Header("The value to use for the comparison. Leave blank for any change in value")]
        public string Value;
        
        [Header("Should the trigger react to a specific condition.")]
        public bool ReactToSpecificValue;
        
        [DisableFieldIf("ReactToSpecificValue", false)]
        [Header("Should the trigger active when achieving or loosing the specified condition.")]
        public bool ReactOnObtainingValue = true;

        [Header("Position offset, from transform.position")]
        public Vector3 localPositionOffset;
        
        [Header("Rotation offset, from transform.forward")]
        public Vector3 forwardRotationOffset;
        
        public OnStateChangeTrigger() : base(GameFeelTriggerType.OnStateChanged) {  }
        
        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            foreach (var gameObject in attachTo)
            {
                var component = gameObject.AddComponent<StateChangedTriggerScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.selectedComponent = selectedComponent;
                component.field = field;
                component.ReactOnObtainingValue = ReactOnObtainingValue;
                component.ReactToSpecificValue = ReactToSpecificValue;
                component.Conditional = Conditional;
                component.Value = Value;
                component.localPositionOffset = localPositionOffset;
                component.forwardRotationOffset = forwardRotationOffset;
                description.attachedTriggers.Add(component);
            }
        }
    }
}