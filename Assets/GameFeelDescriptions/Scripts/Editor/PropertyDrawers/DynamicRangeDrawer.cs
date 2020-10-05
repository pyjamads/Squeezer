using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof (DynamicRangeAttribute))]
    public class DynamicRangeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!GUI.enabled) return;
            
            var attribute = (DynamicRangeAttribute) this.attribute;

            var min = 0f;
            var max = 1f;
            if (attribute.minValue is int minInt)
            {
                min = minInt;
            }
            else if (attribute.minValue is float minFloat)
            {
                min = minFloat;
            }
            else if (attribute.minValue is string minStr)
            {
                var otherProp = EditorHelpers.ActualFindPropertyRelative(property, minStr);

                if (otherProp != null)
                {
                    if (otherProp.propertyType is SerializedPropertyType.Integer)
                    {
                        min = otherProp.intValue;
                    }
                    else if (otherProp.propertyType is SerializedPropertyType.Float)
                    {
                        min = otherProp.floatValue;
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label.text, "DynamicRange Min only supports float or int references.");
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "DynamicRange Min couldn't load reference.");
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "DynamicRange Min only supports string, float or int values.");
            }

            if (attribute.maxValue is int maxInt)
            {
                max = maxInt;
            }
            else if (attribute.maxValue is float maxFloat)
            {
                max = maxFloat;
            }
            else if (attribute.maxValue is string maxStr)
            {
                var otherProp = EditorHelpers.ActualFindPropertyRelative(property, maxStr);

                if (otherProp != null)
                {
                    if (otherProp.propertyType is SerializedPropertyType.Integer)
                    {
                        max = otherProp.intValue;
                    }
                    else if (otherProp.propertyType is SerializedPropertyType.Float)
                    {
                        max = otherProp.floatValue;
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label.text, "DynamicRange Max only supports float or int references.");
                    }
                }
                else
                {
                    EditorGUI.LabelField(position, label.text, "DynamicRange Max couldn't load reference.");
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "DynamicRange Max only supports string, float or int values.");
            }
            
            
            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = Mathf.Clamp(property.floatValue, min, max);
                    EditorGUI.Slider(position, property, min, max, label);
                    break;
                case SerializedPropertyType.Integer:
                    property.intValue = Mathf.Clamp(property.intValue, (int) min, (int) max);
                    EditorGUI.IntSlider(position, property, (int) min, (int) max, label);
                    break;
                default:
                    EditorGUI.LabelField(position, label.text, "Only use DynamicRange with float or int fields.");
                    break;
            }
            
        }
    }
}