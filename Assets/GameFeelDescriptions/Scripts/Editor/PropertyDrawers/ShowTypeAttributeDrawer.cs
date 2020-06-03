using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(ShowTypeAttribute))]
    public class ShowTypeAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //Very dumb custom handling, because the ShowTypeAttribute attribute
            //overrides the standard PropertyTweenEffect custom property drawer.
            if (string.Equals(property.type, "managedReference<PropertyTweenEffect>"))
            {
                return EditorGUIUtility.singleLineHeight;
            }
            
            return EditorGUI.GetPropertyHeight(property, true);
        }
 
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //Right now we don't handle things that are not generic or managed.
            if (property.propertyType != SerializedPropertyType.ManagedReference && 
                property.propertyType != SerializedPropertyType.Generic)
            {
                EditorGUI.PropertyField(position, property, GUIContent.none, true);
                return;
            }
            
            EditorGUI.BeginProperty(position, label, property);

            var typeLabelPosition = position;
            typeLabelPosition.x += EditorGUIUtility.labelWidth + 1 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.width = position.width - EditorGUIUtility.labelWidth - 1 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.height = EditorGUIUtility.singleLineHeight;
            
            var storedIndent = EditorGUI.indentLevel;
           
            var standardLabelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(standardLabelPosition, label);    
           
            EditorGUI.indentLevel = 0;

            var className = property.type;
            
            if (property.propertyType == SerializedPropertyType.ManagedReference)
            {
                if(string.IsNullOrWhiteSpace(property.managedReferenceFullTypename)) return;
                
                var typeClassName = property.managedReferenceFullTypename.Split(char.Parse(" "))[1];
                var typeDomainSplit = typeClassName.Split('.');
   
                className = typeDomainSplit[typeDomainSplit.Length - 1];

                className = string.IsNullOrEmpty(className) ? "Null (Assign)" : className;
            }
            
            GUI.Label(typeLabelPosition, new GUIContent(className, className));
        
            EditorGUI.indentLevel = storedIndent;

            //Very dumb custom handling, because the attribute overrides the standard custom property drawer.
            if (string.Equals(property.type, "managedReference<PropertyTweenEffect>"))
            {
                PropertyTweenEffectDrawer.DrawPropertyTweenEffect(property);
            }
            else
            {
                EditorGUI.PropertyField(position, property, GUIContent.none, true);    
            }
        
            EditorGUI.EndProperty(); 
        } 
    }
}