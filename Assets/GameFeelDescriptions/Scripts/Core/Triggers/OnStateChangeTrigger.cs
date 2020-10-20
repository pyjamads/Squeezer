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
        
        public enum TriggerConditionType
        {
            OnValueChanged,
            OnValueChangedAndConditionIsTrue,
            OnConditionAchieved,
            OnConditionLost,
            WhileConditionIsTrue,
        }
        
        [Header("The script containing the field (on GameObjects or Prefabs).")]
        public MonoBehaviour selectedComponent;

        [Header("The field to track.")]
        [MemberInfoSelector("selectedComponent")]
        public string field;
        
        [Header("When should the trigger react.")]
        public TriggerConditionType TriggerCondition;
        
        //[DisableFieldIf("TriggerCondition", TriggerConditionType.OnValueChanged)]
        [Header("Conditional operator to apply, when comparing values.")]
        public Conditionals Conditional;
        
        [DisableFieldIf("TriggerCondition", TriggerConditionType.OnValueChanged)]
        [Header("The value to use for the comparison. Leave blank for any change in value")]
        public string Value;

        
        [Header("Position offset, from transform.position")]
        public Vector3 localPositionOffset;
        
        [Header("Is position offset relative to transform.forward")]
        public bool useForwardForPositionOffset;
        
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
                component.TriggerCondition = TriggerCondition;
                component.Conditional = Conditional;
                component.Value = Value;
                component.localPositionOffset = localPositionOffset;
                component.useForwardForPositionOffset = useForwardForPositionOffset;
                component.forwardRotationOffset = forwardRotationOffset;
                description.attachedTriggers.Add(component);
            }
        }
    }
}