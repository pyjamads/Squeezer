using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(EnableFieldIfAttribute))]
    public class EnableFieldIfAttributeDrawer : PropertyDrawer
    {
        private float heightOfSelf;
        private float heightOfOthers;
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            heightOfOthers = 0;
            
            var enableFieldIf = attribute as EnableFieldIfAttribute;
            if (enableFieldIf != null && enableFieldIf.allEnableFieldIfAttributes == null)
            {
                enableFieldIf.allEnableFieldIfAttributes = fieldInfo.GetCustomAttributes(typeof(EnableFieldIfAttribute), false)
                    .Cast<EnableFieldIfAttribute>().OrderBy(s => s.order).ToList();
            }

            var obj = GetValue(property);

            if (enableFieldIf.allEnableFieldIfAttributes != null)
            {
                foreach (var propertyValuePair in enableFieldIf.allEnableFieldIfAttributes)
                {
                    var shouldShow = propertyValuePair.negate
                        ? !obj.Equals(propertyValuePair.value)
                        : obj.Equals(propertyValuePair.value);
                    if (shouldShow)
                    {
                        var otherProp = EditorHelpers.ActualFindPropertyRelative(property, propertyValuePair?.property);
                        
                        if (otherProp != null)
                        {
                            heightOfOthers += EditorGUI.GetPropertyHeight(otherProp, true);

                            if (otherProp.hasChildren && otherProp.isExpanded)
                            {
                                //Add space for size label.
                                heightOfOthers += EditorGUIUtility.singleLineHeight;
                                
                                //Add space for elements.
                                if (otherProp.isArray && otherProp.arraySize > 0)
                                {
                                    var childProp = otherProp.GetArrayElementAtIndex(0);
                                    var height = base.GetPropertyHeight(childProp, GUIContent.none);

                                    heightOfOthers += height * otherProp.arraySize;
                                }
                                else if (otherProp.isFixedBuffer && otherProp.fixedBufferSize > 0)
                                {
                                    var childProp = otherProp.GetFixedBufferElementAtIndex(0);
                                    var height = base.GetPropertyHeight(childProp, GUIContent.none);

                                    heightOfOthers += height * otherProp.fixedBufferSize;
                                }
                            }
                        }
                    }
                }
            }

            heightOfSelf = base.GetPropertyHeight(property, label);
            return heightOfSelf + heightOfOthers;
        }


        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {   
            // First get the attribute since it contains the range for the slider
            var enableFieldIf = attribute as EnableFieldIfAttribute;

            var obj = GetValue(property);
            
            EditorGUI.BeginProperty(position, label, property); 
            
            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, heightOfSelf), property, label, true);

            var positionY = position.y + heightOfSelf;
            
            foreach (var propertyValuePair in enableFieldIf.allEnableFieldIfAttributes)
            {
                var shouldShow = propertyValuePair.negate
                    ? (obj != propertyValuePair.value && !obj.Equals(propertyValuePair.value))
                    : (obj == propertyValuePair.value || obj.Equals(propertyValuePair.value));
                
                if (shouldShow)
                {
                    var otherProp = EditorHelpers.ActualFindPropertyRelative(property, propertyValuePair?.property);

                    if (otherProp != null)
                    {
                        if (otherProp.propertyType == SerializedPropertyType.ArraySize || 
                            otherProp.propertyType == SerializedPropertyType.FixedBufferSize ||
                            otherProp.propertyType == SerializedPropertyType.Generic)
                        {
                            otherProp.isExpanded = EditorGUI.Foldout(new Rect(position.x - 1, positionY, position.width, EditorGUIUtility.singleLineHeight), otherProp.isExpanded, otherProp.displayName);
                            
                            if (otherProp.isExpanded)
                            {
                                var oldIndent = EditorGUI.indentLevel++;
                                
                                //Add space for elements.
                                if (otherProp.isArray)
                                {
                                    //Add size label.
                                    positionY += EditorGUIUtility.singleLineHeight;
                                    otherProp.arraySize = EditorGUI.IntField(new Rect(position.x, positionY, position.width, EditorGUIUtility.singleLineHeight),"Size", otherProp.arraySize);
                                    property.serializedObject.ApplyModifiedProperties();
                                    
                                    //Add child objects
                                    for (int i = 0; i < otherProp.arraySize; i++)
                                    {
                                        positionY += EditorGUIUtility.singleLineHeight;
                                        var element = otherProp.GetArrayElementAtIndex(i);
                                        var height = base.GetPropertyHeight(element, GUIContent.none);
                                        EditorGUI.PropertyField(new Rect(position.x, positionY, position.width, height), element, true);
                                    }
                                }
                                else if (otherProp.isFixedBuffer)
                                {
                                    //Add size label.
                                    positionY += EditorGUIUtility.singleLineHeight;
                                    EditorGUI.LabelField(new Rect(position.x, positionY, position.width, EditorGUIUtility.singleLineHeight),"Size", otherProp.fixedBufferSize.ToString());
                                    property.serializedObject.ApplyModifiedProperties();
                                    
                                    //Add child objects
                                    for (int i = 0; i < otherProp.fixedBufferSize; i++)
                                    {
                                        positionY += EditorGUIUtility.singleLineHeight;
                                        var element = otherProp.GetFixedBufferElementAtIndex(i);
                                        var height = base.GetPropertyHeight(element, GUIContent.none);
                                        EditorGUI.PropertyField(new Rect(position.x, positionY, position.width, height), element, true);
                                    }
                                }

                                EditorGUI.indentLevel = oldIndent;
                            }
                        }
                       else
                        {
                            var height = EditorGUI.GetPropertyHeight(otherProp, true);
                            EditorGUI.PropertyField(new Rect(position.x, positionY, position.width, height), otherProp, true);
                            positionY += height;
                        }
                    }
                }    
            }
            
            EditorGUI.EndProperty();

            property.serializedObject.ApplyModifiedProperties();
        }
        
        public static object GetValue(SerializedProperty prop)
        {
            switch (prop.propertyType)
            {
                case SerializedPropertyType.Generic:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.Integer:
                    return prop.intValue;
                    break;
                case SerializedPropertyType.Boolean:
                    return prop.boolValue;
                    break;
                case SerializedPropertyType.Float:
                    return prop.floatValue;
                    break;
                case SerializedPropertyType.String:
                    return prop.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    return prop.colorValue;
                    break;
                case SerializedPropertyType.ObjectReference:
                    return prop.objectReferenceValue;
                    break;
                case SerializedPropertyType.LayerMask:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.Enum:
                    return prop.enumValueIndex;
                    break;
                case SerializedPropertyType.Vector2:
                    return prop.vector2Value;
                    break;
                case SerializedPropertyType.Vector3:
                    return prop.vector3Value;
                    break;
                case SerializedPropertyType.Vector4:
                    return prop.vector4Value;
                    break;
                case SerializedPropertyType.Rect:
                    return prop.rectValue;
                    break;
                case SerializedPropertyType.ArraySize:
                    return prop.arraySize;
                    break;
                case SerializedPropertyType.Character:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.AnimationCurve:
                    return prop.animationCurveValue;
                    break;
                case SerializedPropertyType.Bounds:
                    return prop.boundsValue;
                    break;
                case SerializedPropertyType.Gradient:
                    Debug.Log("Is this happening? THen figure out how to get the value.");
                    break;
                case SerializedPropertyType.Quaternion:
                    return prop.quaternionValue;
                    break;
                case SerializedPropertyType.ExposedReference:
                    return prop.exposedReferenceValue;
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    return prop.fixedBufferSize;
                    break;
                case SerializedPropertyType.Vector2Int:
                    return prop.vector2IntValue;
                    break;
                case SerializedPropertyType.Vector3Int:
                    return prop.vector3IntValue;
                    break;
                case SerializedPropertyType.RectInt:
                    return prop.rectIntValue;
                    break;
                case SerializedPropertyType.BoundsInt:
                    return prop.boundsIntValue;
                    break;
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