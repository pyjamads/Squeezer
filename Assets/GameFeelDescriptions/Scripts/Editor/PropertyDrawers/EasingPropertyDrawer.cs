using UnityEngine;

namespace GameFeelDescriptions
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(EasingHelper.EaseType))]
    public class EasingPropertyDrawer : PropertyDrawer
    {
        private AnimationCurve animationCurve;
        
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {   
            if (property.propertyType != SerializedPropertyType.Enum && 
                property.propertyType != SerializedPropertyType.Integer) return;
            
            EasingHelper.EaseType ease;
            
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                ease = (EasingHelper.EaseType)property.intValue;
            }
            else
            {
                ease = (EasingHelper.EaseType)property.enumValueIndex;    
            }
            
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var enumRect = new Rect(position.x, position.y, position.width, 20);
            var curveRect = new Rect(position.x, position.y + 20, position.width, position.height - 20);

            EditorGUI.BeginChangeCheck();
            // Draw fields - pass GUIContent.none to each so they are drawn without labels
            ease = (EasingHelper.EaseType)EditorGUI.EnumPopup(enumRect, GUIContent.none, ease);
            if(ease != EasingHelper.EaseType.Curve && (animationCurve == null || EditorGUI.EndChangeCheck()))
            {
                animationCurve = EasingHelper.Ease2Curve(ease);
            }

            var path = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf("."));
            var curveProp = property.serializedObject.FindProperty(path+".curve");
            
            if (ease == EasingHelper.EaseType.Curve)
            {
                if (curveProp != null && curveProp.propertyType == SerializedPropertyType.AnimationCurve)
                {
                    curveProp.animationCurveValue = EditorGUI.CurveField(curveRect, curveProp.animationCurveValue);
                    animationCurve = null;
                }
                else
                {
                    EditorGUI.HelpBox(curveRect, "Field [AnimationCurve curve] not available here!", MessageType.Error);
                }
            }
            else
            {   
                EditorGUI.BeginChangeCheck();
                animationCurve = EditorGUI.CurveField(curveRect, animationCurve);

                if (EditorGUI.EndChangeCheck())
                {
                    if (curveProp != null && curveProp.propertyType == SerializedPropertyType.AnimationCurve)
                    {
                        ease = EasingHelper.EaseType.Curve;
                        curveProp.animationCurveValue = animationCurve;
                    }
                }
            }
                        
            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = (int) ease;
            }
            else
            {
                property.enumValueIndex = (int) ease;
            }

            property.serializedObject.ApplyModifiedProperties();
            
            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + 50;
        }
    }
}