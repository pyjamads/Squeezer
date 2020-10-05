using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(MemberInfoSelectorAttribute))]
    public class MemberInfoSelectorAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //This only works on strings!
            if (property.propertyType != SerializedPropertyType.String)
            {
                base.OnGUI(position, property, label);
                return;
            }
            
            var memberInfoAttr = attribute as MemberInfoSelectorAttribute;

            var menu = new GenericMenu();
            menu.allowDuplicateNames = true;
            var type = memberInfoAttr?.type;
            if (type == null)
            {
                var otherProp = EditorHelpers.ActualFindPropertyRelative(property, memberInfoAttr?.getTypeRelative);

                if (otherProp == null) return;
                
                if (otherProp.propertyType != SerializedPropertyType.ObjectReference)
                {
                    menu.AddDisabledItem(new GUIContent("Relative Property "+memberInfoAttr?.getTypeRelative+" not supported!"));
                    EditorGUILayout.EndHorizontal();
                    menu.ShowAsContext();
                    return;
                }

                if (otherProp.objectReferenceValue != null)
                {
                    type = otherProp.objectReferenceValue.GetType();
                }
            }

            if (type != null)
            {
                // var properties = type.GetProperties(BindingFlags.Public | 
                //                                     BindingFlags.NonPublic 
                //                                     // |
                //                                     // BindingFlags.Instance 
                //                                     // |
                //                                     // BindingFlags.DeclaredOnly
                //     );

                var fields = type.GetFields(BindingFlags.Public |
                                            BindingFlags.NonPublic |
                                            BindingFlags.Instance 
                    // |
                    // BindingFlags.DeclaredOnly
                );


                var list = fields.Select(m => m.Name).ToList();//.Concat(properties.Select(m => m.Name)).ToList();
                var typelist = fields.Select(m => m.FieldType); //.Concat(properties.Select(m => m.PropertyType));

                var pos = new Rect(position);
                pos.width = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                pos.x = position.x + position.width - pos.width;
            
                position.width = position.width - pos.width;
            
                var selectedIndex = list.FindIndex(item => item.Equals(property.stringValue));
                
                if(selectedIndex >= 0)
                {
                    var selectedType = typelist.ElementAt(selectedIndex);

                    //Make type names recognizable for beginners.
                    var typeName = selectedType.Name;
                    if (typeName.Equals("Single")) typeName = "float";
                    else if (typeName.Equals("Int32")) typeName = "int";

                    EditorGUI.LabelField(position, label, new GUIContent(typeName + " " + property.stringValue));
                }
                else
                {
                    if (list.Count > 0)
                    {
                        EditorGUI.LabelField(position, label, new GUIContent("Select a Property!"));    
                    }
                    else
                    {
                        EditorGUI.LabelField(position, label, new GUIContent("No fields available!"));
                        menu.AddDisabledItem(new GUIContent("No fields available!"));
                    }
                    
                }
            
                var pressed = EditorGUI.DropdownButton(pos, GUIContent.none, FocusType.Keyboard);

                if (pressed)
                {
                    for (var index = 0; index < list.Count; index++)
                    {
                        var i = index;

                        var ltype = typelist.ElementAt(i).Name;
                        if (ltype.Equals("Single")) ltype = "float";
                        else if (ltype.Equals("Int32")) ltype = "int";
                        
                        menu.AddItem(new GUIContent(ltype + " " + list[i]), false, () =>
                        {
                            property.stringValue = list[i];
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.ShowAsContext();
                }
                
                //EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.LabelField(position, label, new GUIContent("No Component Selected!"));
            }
        }
    }
}