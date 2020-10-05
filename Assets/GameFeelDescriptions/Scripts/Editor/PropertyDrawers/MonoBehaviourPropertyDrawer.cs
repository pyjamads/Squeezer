using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(MonoBehaviour))]
    public class MonoBehaviourPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
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

                var currentComp = (Component) property.objectReferenceValue;
                if (currentComp == null)
                {
                    menu.AddDisabledItem(new GUIContent("No Game Object Selected!"));

                    //Find all objects in the scene, and allow the user to select one from that!
                    var components = Object.FindObjectsOfType<GameObject>().Where(item => item.GetComponent<MonoBehaviour>());
                    foreach (var component in components.OrderBy(item => item.name))
                    {
                        menu.AddItem(new GUIContent(component.name), false, () =>
                        {
                            property.objectReferenceValue = component;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }
                else
                {
                    var components = currentComp.GetComponents<MonoBehaviour>();
                    var componentNames = components.Select((m) => m.GetType().Name)
                        .ToList();

                    if (components.Length == 0)
                    {
                        menu.AddDisabledItem(new GUIContent("No MonoBehaviours on this object!"));
                    }

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
                }

                menu.ShowAsContext();
            }
        }
    }
}