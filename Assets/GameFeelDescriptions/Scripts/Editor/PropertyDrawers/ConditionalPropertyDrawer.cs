using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(OnStateChangeTrigger.Conditionals))]
    public class ConditionalPropertyDrawer : PropertyDrawer
    {
        private static readonly string[] displayOptions = {"==", "!=", "<", ">", "<=", ">="};

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //if (Event.current.type == EventType.Repaint) return;
            
            if (property.propertyType != SerializedPropertyType.Enum &&
                property.propertyType != SerializedPropertyType.Integer)
            {
                return;
            }
            
            int conditional;
            
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                conditional = property.intValue;
            }
            else
            {
                conditional = property.enumValueIndex;    
            }
            
            conditional = EditorGUI.Popup(position, "Conditional", conditional, displayOptions);
            
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = conditional;
            }
            else
            {
                property.enumValueIndex = conditional;
            }

            property.serializedObject.ApplyModifiedProperties();
        }

    }
}