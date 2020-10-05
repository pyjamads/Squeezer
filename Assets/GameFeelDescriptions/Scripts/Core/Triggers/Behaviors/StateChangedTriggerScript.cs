using System;
using System.Reflection;
using UnityEngine;
using Conditionals = GameFeelDescriptions.OnStateChangeTrigger.Conditionals;


namespace GameFeelDescriptions
{
    public class StateChangedTriggerScript : GameFeelBehaviorBase
    {

        [Header("The component containing the field or property.")]
        public string ComponentName;
        [Header("The field or property to track.")]
        public string StateField;

        
        [Header("Conditional operator to apply, when comparing values.")]
        public Conditionals Conditional;
        
        [Header("The value to use for the comparison.")]
        public string Value;
        
        [Header("Should the trigger react to a specific condition.")]
        public bool ReactToSpecificValue;
        
        [Header("Should the trigger active when achieving or loosing the specified condition.")]
        public bool ReactOnObtainingValue = true;

        private Component component;
        private MemberInfo memberInfo;
        private object storedValue;

        private void Start()
        {
            SetupInitialTargets();

            component = GetComponent(ComponentName);
            if (component == null)
            {
                Debug.LogError("Failed to find "+ComponentName+" on "+gameObject.name);
                return;
            }
            
            var type = component.GetType();
            //TODO: might want to also get Properties... 2020-09-30
            var memberInfos = type.GetMember(StateField, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (memberInfos.Length == 0)
            {
                Debug.LogError("Failed to find "+StateField+" on "+ComponentName);
                return;
            }
            
            //Get the member info.
            memberInfo = memberInfos[0];
            //Check that the member is a field or property.
            if (memberInfo.MemberType == MemberTypes.Field || memberInfo.MemberType == MemberTypes.Property)
            {
                //Get the current value.
                storedValue = memberInfo.GetValue(component);   
            }
            else
            {
                Debug.LogError(StateField+" on "+ComponentName+ " is neither a Property or Field.");
            }
        }

        void Update()
        {
            if (component != null && memberInfo != null)
            {
                var currentValue = memberInfo.GetValue(component);
                
                //TODO: We may want to be able to determine the value that triggers the event,
                //TODO: for booleans this could be when it's flipped to true,
                //TODO: for enums, it might be a switch to a specific flag 
                //TODO: and for delegates, it might switching to a specific method, such as "DoJump" or "DoShoot" 2020-09-30
                
                //If the stored value is different to the current value!
                if (storedValue.Equals(currentValue) == false)
                {
                    //Check the value change more thoroughly.
                    if (ReactToSpecificValue)
                    {
                        var before = CheckValue(storedValue);
                        var after = CheckValue(currentValue);

                        //React to the value changing to the specified value 
                        if (ReactOnObtainingValue && !before && after)
                        {
                            //Trigger the effects!
                            ExecuteTrigger();
                        }
                        //React to the value changing away from the specified value
                        else if(before && !after)
                        {
                            //Trigger the effects!
                            ExecuteTrigger();
                        }
                    }
                    else
                    {
                        //If a value is defined
                        if (!string.IsNullOrWhiteSpace(Value))
                        {
                            //check it with the conditional 
                            if (CheckValue(currentValue))
                            {
                                //if it's fulfilled, execute trigger
                                ExecuteTrigger();
                            }
                        }
                        else
                        {
                            //Trigger the effects!
                            ExecuteTrigger();    
                        }
                    }
                    
                    //Lastly update the storedValue
                    storedValue = currentValue;
                }
            }
        }

        private bool CheckValue(object val)
        {
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
            
            //When a custom event triggers, Other is the origin of the event.
            for (var i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i], storedValue);
#endif
                
                //TODO: maybe we want to pass the value of the state change... but for now, let's just react to the change. 2020-09-30
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], new GameFeelTriggerData{Origin = gameObject});
            }
        }
        
    }
}