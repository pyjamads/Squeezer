using System;
using System.Reflection;
using UnityEngine;
using Conditionals = GameFeelDescriptions.OnStateChangeTrigger.Conditionals;
using TriggerConditionType = GameFeelDescriptions.OnStateChangeTrigger.TriggerConditionType;

namespace GameFeelDescriptions
{
    public class StateChangedTriggerScript : GameFeelBehaviorBase<OnStateChangeTrigger>
    {
        [Header("The script containing the field (on GameObjects or Prefabs).")]
        public MonoBehaviour selectedComponent;
        
        [Header("The field to track.")]
        [MemberInfoSelector("selectedComponent")]
        public string field;
        
        [Header("Conditional operator to apply, when comparing values.")]
        public Conditionals Conditional;
        
        [Header("The value to use for the comparison.")]
        public string Value;

        [Header("When should the trigger react.")]
        public TriggerConditionType TriggerCondition;
        
        [Header("Position offset, from transform.position")]
        public Vector3 localPositionOffset;
        
        [Header("Is position offset relative to transform.forward")]
        public bool useForwardForPositionOffset;
        
        [Header("Rotation offset, from transform.forward")]
        public Vector3 forwardRotationOffset;

        //public Component selectedComponent;
       
        private MemberInfo memberInfo;
        private object storedValue;
        

        private void Start()
        {
            SetupInitialTargets();

            //selectedComponent = GetComponent(selectedComponent);
            if (selectedComponent == null)
            {
                Debug.LogError("No component selected!");//"Failed to find "+selectedComponent.name+" on "+gameObject.name);
                return;
            }
            
            //Get the type from the selectedComponent.
            var type = selectedComponent.GetType();
            
            //Get component on attached game object!
            selectedComponent = (MonoBehaviour)GetComponent(type);
            
            var memberInfos = type.GetMember(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (memberInfos.Length == 0)
            {
                Debug.LogError("Failed to find "+field+" on "+selectedComponent.name);
                return;
            }
            
            //Get the member info.
            memberInfo = memberInfos[0];
            //Check that the member is a field or property.
            if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
            {
                //Get the current value.
                storedValue = memberInfo.GetValue(selectedComponent);   
            }
            else
            {
                Debug.LogError(field+" on "+selectedComponent.name+ " is neither a Property or Field.");
            }
        }

        void Update()
        {
            if (selectedComponent != null && memberInfo != null)
            {
                var currentValue = memberInfo.GetValue(selectedComponent);

                switch (TriggerCondition)
                {
                    case TriggerConditionType.OnValueChanged:
                        //If value changed, trigger
                        if (storedValue.Equals(currentValue) == false)
                        { 
                            //Trigger the effects!
                            ExecuteTrigger();
                        }
                        break;
                    case TriggerConditionType.OnValueChangedAndConditionIsTrue:
                        //If value changed, check condition
                        if (storedValue.Equals(currentValue) == false)
                        {
                            //If the condition is true, trigger
                            if (CheckValue(currentValue))
                            {
                                //if it's fulfilled, execute trigger
                                ExecuteTrigger();
                            }
                        }
                        break;
                    case TriggerConditionType.OnConditionAchieved:
                        //If value changed, check condition
                        if (storedValue.Equals(currentValue) == false)
                        {
                            var before = CheckValue(storedValue);
                            var after = CheckValue(currentValue);

                            //If condition was false before, but true after the value change, trigger
                            if (!before && after)
                            {
                                //Trigger the effects!
                                ExecuteTrigger();
                            }
                        }
                        break;
                    case TriggerConditionType.OnConditionLost:
                        //If value changed, check condition
                        if (storedValue.Equals(currentValue) == false)
                        {
                            var before = CheckValue(storedValue);
                            var after = CheckValue(currentValue);

                            //If condition was true before, but not after the value change, trigger
                            if (before && !after)
                            {
                                //Trigger the effects!
                                ExecuteTrigger();
                            }
                        }
                        break;
                    case TriggerConditionType.WhileConditionIsTrue:
                        //If the condition is true, trigger 
                        if (CheckValue(currentValue))
                        {
                            //if it's fulfilled, execute trigger
                            ExecuteTrigger();
                        }
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                //Lastly update the storedValue
                storedValue = currentValue;
            }
        }

        private bool CheckValue(object val)
        {
            if (string.IsNullOrWhiteSpace(Value))
            {
                Debug.LogWarning("No Value is set for StateChangedTrigger on: "+gameObject.name);
                return false;
            }

            switch (val)
            {
                case bool boolean:
                    var valuesAreEqual = string.Compare(Value, boolean.ToString()) == 0;
                    switch (Conditional)
                    {
                        case Conditionals.Equals:
                            return valuesAreEqual;
                        case Conditionals.LessThanOrEquals: // boolean <= Value
                            return valuesAreEqual || !boolean;
                        case Conditionals.GreaterThanOrEquals:
                            return valuesAreEqual || boolean; // boolean >= Value
                        case Conditionals.NotEquals:
                            return !valuesAreEqual;
                        case Conditionals.LessThan:
                            return !valuesAreEqual && !boolean; 
                        case Conditionals.GreaterThan:
                            return !valuesAreEqual && boolean;
                    }
                    break;
                case Enum enumVal:
                    var valueIndex = EnumExtensions.GetValueIndex(enumVal.GetType(), Value);
                    var currentIndex = enumVal.GetValueIndex();
                    switch (Conditional)
                    {
                        case Conditionals.Equals:
                            return currentIndex == valueIndex;
                        case Conditionals.NotEquals:
                            return currentIndex != valueIndex;
                        case Conditionals.LessThan:
                            return currentIndex < valueIndex;
                        case Conditionals.GreaterThan:
                            return currentIndex > valueIndex;
                        case Conditionals.LessThanOrEquals:
                            return currentIndex <= valueIndex;
                        case Conditionals.GreaterThanOrEquals:
                            return currentIndex >= valueIndex;
                    }
                    break;
                case Delegate delegateVal:
                    //For delegates just use the string comparison value, on the method name.
                    switch (Conditional)
                    {
                        case Conditionals.Equals:
                            return string.Compare(delegateVal.Method.Name, Value) == 0;
                        case Conditionals.NotEquals:
                            return string.Compare(delegateVal.Method.Name, Value) != 0;
                        case Conditionals.LessThan:
                            return string.Compare(delegateVal.Method.Name, Value) < 0;
                        case Conditionals.GreaterThan:
                            return string.Compare(delegateVal.Method.Name, Value) > 0;
                        case Conditionals.LessThanOrEquals:
                            return string.Compare(delegateVal.Method.Name, Value) <= 0;
                        case Conditionals.GreaterThanOrEquals:
                            return string.Compare(delegateVal.Method.Name, Value) >= 0;
                    }
                    break;
                case float floatVal:
                    float f;
                    var floatParsed = float.TryParse(Value, out f);

                    if (floatParsed)
                    {
                        switch (Conditional)
                        {
                            case Conditionals.Equals:
                                return Math.Abs(floatVal - f) < float.Epsilon;
                            case Conditionals.NotEquals:
                                return Math.Abs(floatVal - f) > float.Epsilon;
                            case Conditionals.LessThan:
                                return floatVal < f;
                            case Conditionals.GreaterThan:
                                return floatVal > f;
                            case Conditionals.LessThanOrEquals:
                                return floatVal <= f;
                            case Conditionals.GreaterThanOrEquals:
                                return floatVal >= f;
                        }   
                    }
                    break;
                case int intVal:
                    int i;
                    var intParsed = int.TryParse(Value, out i);
                    if (intParsed)
                    {
                        switch (Conditional)
                        {
                            case Conditionals.Equals:
                                return intVal == i;
                            case Conditionals.NotEquals:
                                return intVal != i;
                            case Conditionals.LessThan:
                                return intVal < i;
                            case Conditionals.GreaterThan:
                                return intVal > i;
                            case Conditionals.LessThanOrEquals:
                                return intVal <= i;
                            case Conditionals.GreaterThanOrEquals:
                                return intVal >= i;
                        }
                    }
                    break;
            }

            //We're just not gonna handle any other type. (Float and Int might even be a bit over zealous)
            //it's however still possible to react to value changes. 
            return false;
        }

        private void ExecuteTrigger()
        {
            if (Disabled) return;
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode(storedValue);

            }
#endif

            var positionWithOffset = transform.position + localPositionOffset;

            if (useForwardForPositionOffset)
            {
                var qrt = Quaternion.LookRotation(transform.forward, transform.up);
                positionWithOffset = transform.position + qrt * localPositionOffset;
            }

            var positionDelta = new PositionalData(positionWithOffset, Quaternion.Euler(forwardRotationOffset) * transform.forward) { Origin = gameObject };
            
            //When a custom event triggers, Other is the origin of the event.
            for (var i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i], storedValue);
#endif
                
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], positionDelta);
            }
        }
        
    }
}