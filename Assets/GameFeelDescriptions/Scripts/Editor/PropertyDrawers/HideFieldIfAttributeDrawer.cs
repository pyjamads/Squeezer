using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(HideFieldIfAttribute))]
    public class HideFieldIfAttributeDrawer : PropertyDrawer
    {
        

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hideFieldIf = attribute as HideFieldIfAttribute;

            var otherProp = EditorHelpers.ActualFindPropertyRelative(property, hideFieldIf?.property);

            if (otherProp == null) return EditorGUI.GetPropertyHeight(property, true);

            bool shouldShow;
            if (otherProp.isArray)
            {
                //Do List.Any() logic here or Count() == 0:
                shouldShow = hideFieldIf.negate
                    ? otherProp.arraySize == 0
                    : otherProp.arraySize > 0;
            }
            else
            {
                var obj = GetValue(otherProp);
                shouldShow = hideFieldIf.negate
                    ? (hideFieldIf.value != obj && !obj.Equals(hideFieldIf.value))
                    : (hideFieldIf.value == obj || obj.Equals(hideFieldIf.value));
            }

            if (!shouldShow)
            {
                return EditorGUI.GetPropertyHeight(property, true);
            }

            return -EditorGUIUtility.standardVerticalSpacing;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // First get the attribute since it contains the range for the slider
            var hideFieldIf = attribute as HideFieldIfAttribute;

            var otherProp = EditorHelpers.ActualFindPropertyRelative(property, hideFieldIf?.property);

            //TODO: figure out a better way to "show" property, if it has more attributes!!! 2020-09-22
            if (otherProp == null)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            bool shouldHide;
            if (otherProp.isArray)
            {
                //Do List.Any() logic here or Count() == 0:
                shouldHide = hideFieldIf.negate
                    ? otherProp.arraySize == 0
                    : otherProp.arraySize > 0;
            }
            else
            {
                var obj = GetValue(otherProp);

                shouldHide = hideFieldIf.negate
                    ? (hideFieldIf.value != obj && !obj.Equals(hideFieldIf.value))
                    : (hideFieldIf.value == obj || obj.Equals(hideFieldIf.value));
            }

            if (!shouldHide)
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public static object GetValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return prop;
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                case SerializedPropertyType.LayerMask:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                case SerializedPropertyType.Character:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                case SerializedPropertyType.Gradient:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue;
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue;
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize;
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue;
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue;
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue;
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue;
                case SerializedPropertyType.ManagedReference:
                    //return prop.managedReferenceValue;
                    Debug.LogError("We can't get ManagedReference values easily...");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Anything that falls through gets a null value.
            return null;
        }
    }
}