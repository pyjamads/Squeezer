using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(Component))]
    public class ComponentPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            var pos = new Rect(position);
            pos.width = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            pos.x = position.x + position.width - pos.width;
            
            position.width = position.width - pos.width;

            EditorGUI.PropertyField(position, property);

            var pressed = EditorGUI.DropdownButton(pos, GUIContent.none, FocusType.Keyboard);

            if (pressed)
            {
                var menu = new GenericMenu();
                menu.allowDuplicateNames = true;
                
                var currentComp = (Component)property.objectReferenceValue;
                if (currentComp == null)
                {
                    menu.AddDisabledItem(new GUIContent("No Game Object Selected!"));
                    menu.ShowAsContext();  
                    EditorGUILayout.EndHorizontal();
                    return;
                }
                
                var components = currentComp.GetComponents<Component>();
                var componentNames = components.Select((m) => m.GetType().Name)
                    .ToList();
                
                for (var index = 0; index < components.Length; index++)
                {
                    var name = componentNames[index];
                    var comp = components[index];
                    
                    menu.AddItem(new GUIContent(name), false, () =>
                    {
                        property.objectReferenceValue = comp;
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }

                menu.ShowAsContext();
            }
            
            EditorGUILayout.EndHorizontal();
        }
    }
}