using System;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class SetupGameFeelDescriptionsWindow : EditorWindow
    {
        private string selectedTag;
        private string selectedName;
        private Component selectedComponent;
        private SerializedProperty selectedComponentObject;

        //TODO: Set description when created, hide the Create buttons, and show some options for the description, also make a "done/create another" button!
        private GameFeelDescription createdDescription;
        
        [MenuItem("GameFeelDescriptions/Setup")]
        public static void ShowWindow() => GetWindow<SetupGameFeelDescriptionsWindow>("Setup Descriptions");

        // private void OnEnable()
        // {
        //     //setup Repainting, while the window is not selected, if needed!
        //     Selection.selectionChanged += Repaint;
        // }
        //
        // private void OnDisable()
        // {
        //     Selection.selectionChanged -= Repaint;
        // }

        private void OnGUI()
        {
            //NOTE consider using DisableScope,  for something
            // var someBOol = true;
            // using (new EditorGUI.DisabledScope(someBOol))
            // {
            //     //Anything in here is disabled if someBOol is true!
            // }

            GUILayout.Label("Create Description!", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    //Attach to tag!
                    GUILayout.Label("Attach to GameObjects with Tag", EditorStyles.boldLabel);
                    using (new GUILayout.HorizontalScope())
                    {
                        selectedTag = EditorGUILayout.TagField(GUIContent.none, selectedTag);
                        if (GUILayout.Button("Create"))
                        {
                            var parent = FindOrCreateParent();
                            var desc = CreateGameFeelDescription(selectedTag, parent);
                            desc.Name = name;
                            desc.Description = "Description of all effects triggered by game objects with the tag [" + selectedTag + "]";
                            desc.AttachToTag = selectedTag;
                        }
                    }
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    //Attach to Component type
                    GUILayout.Label("Attach to GameObjects with Component Type", EditorStyles.boldLabel);
                    using (new GUILayout.HorizontalScope())
                    {
                        //TODO: make sure this looks like the ComponentPropertyDrawer! 04/06/2020
                        selectedComponent = (Component) EditorGUILayout.ObjectField(GUIContent.none, selectedComponent,
                            typeof(Component));

                        if (GUILayout.Button("Create"))
                        {
                            var parent = FindOrCreateParent();
                            var desc = CreateGameFeelDescription(selectedComponent.GetType().Name, parent);
                            desc.Name = name;
                            desc.Description = "Description of all effects triggered by game objects with the tag [" +
                                               selectedComponent.GetType().Name + "]";
                            desc.AttachToComponentType = selectedComponent;
                        }
                    }
                }

                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    //Attach to Named object???
                    GUILayout.Label("Attach to First GameObject with Name", EditorStyles.boldLabel);
                    using (new GUILayout.HorizontalScope())
                    {
                        selectedName = EditorGUILayout.TextField(GUIContent.none, selectedName);
                        if (GUILayout.Button("Create"))
                        {
                            var parent = FindOrCreateParent();
                            var desc = CreateGameFeelDescription(selectedName, parent);
                            desc.Name = name;
                            desc.Description = "Description of all effects triggered by game objects with the tag [" +
                                               selectedName + "]";
                            desc.AttachToTag = selectedName;
                            //TODO: make this work!!!
                        }
                    }
                }
            }
        }
        
        private GameObject FindOrCreateParent()
        {
            //Find or make Description parent GameObject
            var parent = GameObject.Find("GameFeelDescriptions");
            if(!parent) parent = new GameObject("GameFeelDescriptions");

            return parent;
        }
        
        private static GameFeelDescription CreateGameFeelDescription(string baseName, GameObject parent)
        {
            //Generate Descriptions for each selected tag and Attach GameFeelDescription.
            var name = baseName + "Effects";
            var description = new GameObject(name, typeof(GameFeelDescription));
            description.transform.parent = parent.transform;
            var posOffset = Vector3.down * 0.5f;
            var position = parent.transform.position;
            
            if (parent.transform.childCount > 0)
            {
                var child = parent.transform.GetChild(parent.transform.childCount - 1);
                position = child.position + posOffset;
            }
            
            EditorHelpers.SetIconForObject(description.gameObject, parent.transform.childCount % 8);
            description.transform.position = position;

            var desc = description.GetComponent<GameFeelDescription>();
            return desc;
        }
    }
}