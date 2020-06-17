using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class SetupGameFeelDescriptionsWindow : EditorWindow
    {
        public enum AttachTriggersTo
        {
            Tag,
            ComponentType,
            List,
        }

        private string[] triggerTypes;
        private bool[] triggerTypesSelected;

        private OnCollisionTrigger.CollisionActivationType collisionActivationType;
        private OnMoveTrigger.MovementActivationType movementActivationType;
        private string eventName;
        
        private string selectedTag;
        private string selectedComponentType;

        private Vector2 scrollPosition;

        private AttachTriggersTo attachTriggersTo;

        //TODO: Set description when created, hide the Create buttons, and show some options for the description, also make a "done/create another" button!
        private GameFeelDescription createdDescription;
        
        [MenuItem("GameFeelDescriptions/Setup")]
        public static void ShowWindow() => GetWindow<SetupGameFeelDescriptionsWindow>("Setup Descriptions");

        private void OnEnable()
        {
            //setup Repainting, while the window is not selected!
            Selection.selectionChanged += Repaint;
            ClearForm();
        }
        
        private void OnDisable()
        {
            Selection.selectionChanged -= Repaint;
        }

        private void OnGUI()
        {
            if (createdDescription == null)
            {
                GUILayout.Label("Create Description!", EditorStyles.boldLabel);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUI.BeginChangeCheck();
                    attachTriggersTo = (AttachTriggersTo) EditorGUILayout.EnumPopup(attachTriggersTo);
                    var changed = EditorGUI.EndChangeCheck();

                    if (changed)
                    {
                        ClearForm();
                    }
                    
                    switch (attachTriggersTo)
                    {
                        case AttachTriggersTo.Tag:
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                //Attach to tag!
                                GUILayout.Label("Attach to GameObjects with Tag", EditorStyles.boldLabel);
                                using (new GUILayout.HorizontalScope())
                                {
                                    selectedTag = EditorGUILayout.TagField(GUIContent.none, selectedTag);
                                }

                                var hasNotSelectedTag = string.IsNullOrEmpty(selectedTag) || selectedTag.Contains("Untagged");

                                using (new EditorGUI.DisabledScope(hasNotSelectedTag))
                                {
                                    DrawTriggerSelection();
                                    
                                    if (GUILayout.Button("Create"))
                                    {
                                        var parent = FindOrCreateParent();
                                        var desc = CreateGameFeelDescription(selectedTag, parent);
                                        desc.Description =
                                            "Description of all effects triggered by game objects with the tag [" +
                                            selectedTag + "]";
                                        desc.AttachToTag = selectedTag;
                                        
                                        AddTriggers(desc);

                                        createdDescription = desc;
                                    }
                                }
                            }
                            

                            break;
                        case AttachTriggersTo.ComponentType:
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                //Attach to Component type
                                GUILayout.Label("Attach to GameObjects with Component Type", EditorStyles.boldLabel);
                                using (new GUILayout.HorizontalScope())
                                {
                                    //TODO: make sure this looks like the ComponentPropertyDrawer! 04/06/2020
                                    selectedComponentType = EditorGUILayout.TextField(GUIContent.none, selectedComponentType);
                                }
                                
                                var hasNotSelectedAnything = string.IsNullOrEmpty(selectedComponentType);
                                
                                using (new EditorGUI.DisabledScope(hasNotSelectedAnything))
                                {
                                    DrawTriggerSelection();
                                    
                                    if (GUILayout.Button("Create"))
                                    {
                                        var parent = FindOrCreateParent();
                                        var desc = CreateGameFeelDescription(selectedComponentType, parent);
                                        desc.Description =
                                            "Description of all effects triggered by game objects with the component type [" +
                                            selectedComponentType + "]";
                                        desc.AttachToComponentType = selectedComponentType;

                                        AddTriggers(desc);

                                        createdDescription = desc;
                                    }
                                }
                            }

                            break;
                        case AttachTriggersTo.List:
                            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                //Attach to provided GameObject???
                                GUILayout.Label("Attach to Selected GameObjects", EditorStyles.boldLabel);
                                
                                //TODO subscribe to selection change events here !
                                var gameObjects = Selection.gameObjects;
                                GUILayout.Label("Selection:");

                                var anySelected = Selection.gameObjects.Length > 0;
                                
                                if (anySelected == false)
                                {
                                    GUILayout.Label("Select objects in the hierarchy/scene.", EditorStyles.centeredGreyMiniLabel);    
                                }
                                else
                                {
                                    var scrollView = new GUILayout.ScrollViewScope(scrollPosition, GUILayout.ExpandHeight(false));
                                    using (scrollView)
                                    {
                                        scrollPosition = scrollView.scrollPosition;
                                        foreach (var gameObject in gameObjects)
                                        {
                                            GUILayout.Label(gameObject.name, EditorStyles.centeredGreyMiniLabel);
                                        }    
                                    }
                                }

                                using(new EditorGUI.DisabledScope(anySelected == false))
                                {
                                    DrawTriggerSelection();
                                    
                                    if (GUILayout.Button("Create"))
                                    {
                                        var parent = FindOrCreateParent();
                                        var name = string.Join(",", gameObjects.Select(item => item.name).ToArray(), 0,
                                            Mathf.Min(2, gameObjects.Length));
                                        if (gameObjects.Length > 2) name += ",...";

                                        var desc = CreateGameFeelDescription(name, parent);
                                        desc.Description =
                                            "Description of all effects triggered by the game object with the name [" +
                                            name + "]";
                                        //TODO: Don't just attach to transform, that'll attach to everything!!
                                        desc.AttachToObjects = gameObjects;
                                        
                                        AddTriggers(desc);
                                        
                                        createdDescription = desc;
                                    }
                                }
                                    
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (createdDescription != null)
                {    
                    EditorGUIUtility.PingObject(createdDescription);
                    Selection.activeObject = createdDescription;
                }
            }
            else
            {
                if (GUILayout.Button("Click here to add another!"))
                {
                    ClearForm();
                    return;
                }
                
                GUILayout.Label("Enable Step Through Mode or", EditorStyles.boldLabel);
                GUILayout.Label("begin adding effects in the inspector window!", EditorStyles.boldLabel);
                
                createdDescription.StepThroughMode = EditorGUILayout.Toggle("Step Through Mode", createdDescription.StepThroughMode);
            }
        }

        private void AddTriggers(GameFeelDescription desc)
        {
            for (var i = 0; i < triggerTypesSelected.Length; i++)
            {
                if (triggerTypesSelected[i] == false) continue;

                var type = (GameFeelTriggerType) i;
                var typeName = triggerTypes[i];

                var trigger = GameFeelTrigger.CreateTrigger(type);
                //Add a default Effect Group!
                var effectGroup = new GameFeelEffectGroup();
                effectGroup.GroupName = "Effects triggered " + typeName;

                if (type == GameFeelTriggerType.OnCollision)
                {
                    ((OnCollisionTrigger) trigger).type = collisionActivationType;
                }
                else if (type == GameFeelTriggerType.OnMove)
                {
                    ((OnMoveTrigger) trigger).type = movementActivationType;
                }
                else if (type == GameFeelTriggerType.OnCustomEvent)
                {
                    ((OnCustomEventTrigger) trigger).EventName = eventName;
                }
                
                trigger.EffectGroups.Add(effectGroup);
                desc.TriggerList.Add(trigger);
            }
        }

        void ClearForm()
        {
            createdDescription = null;
            if (triggerTypesSelected != null)
            {
                for (int i = 0; i < triggerTypesSelected.Length; i++)
                {
                    triggerTypesSelected[i] = false;
                }
            }

            selectedTag = null;
            selectedComponentType = null;

            eventName = null;
            collisionActivationType = OnCollisionTrigger.CollisionActivationType.OnAllEnter;
            movementActivationType = OnMoveTrigger.MovementActivationType.OnAnyStateChange;
        }

        private void DrawTriggerSelection()
        {
            if (triggerTypes == null)
            {
                triggerTypes = Enum.GetNames(typeof(GameFeelTriggerType));
            }

            if (triggerTypesSelected == null || triggerTypesSelected.Length == 0)
            {
                triggerTypesSelected = new bool[triggerTypes.Length];
            }

            var half = Mathf.CeilToInt(triggerTypes.Length / 2f);
            GUILayout.Label("You can pre-select triggers you want to add here:", EditorStyles.largeLabel);
            using (new GUILayout.VerticalScope())
            {
                for (var i = 0; i < half; i++)
                {
                    var typeName = triggerTypes[i];

                    using (new GUILayout.HorizontalScope())
                    {
                        if (triggerTypesSelected[i])
                        {
                            if ((GameFeelTriggerType) i == GameFeelTriggerType.OnCollision)
                            {
                                using (new GUILayout.VerticalScope())
                                {
                                    triggerTypesSelected[i] = EditorGUILayout.ToggleLeft(typeName, triggerTypesSelected[i]);
                                    EditorGUI.indentLevel += 1;
                                
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Activation", GUILayout.Width(60));
                                        collisionActivationType = (OnCollisionTrigger.CollisionActivationType)EditorGUILayout.EnumPopup(GUIContent.none, collisionActivationType, GUILayout.Width(140));
                                        //TODO: consider also showing the "list" of names/tags to react to! 09/06/2020
                                    }
                                    
                                    EditorGUI.indentLevel -= 1;
                                }    
                            }
                            else if ((GameFeelTriggerType) i == GameFeelTriggerType.OnMove)
                            {
                                using (new GUILayout.VerticalScope())
                                {
                                    triggerTypesSelected[i] = EditorGUILayout.ToggleLeft(typeName, triggerTypesSelected[i]);
                                    EditorGUI.indentLevel += 1;
                                    
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        GUILayout.Label("Activation", GUILayout.Width(60));
                                        movementActivationType = (OnMoveTrigger.MovementActivationType)EditorGUILayout.EnumPopup(GUIContent.none, movementActivationType, GUILayout.Width(140));    
                                    }
                                    
                                    EditorGUI.indentLevel -= 1;
                                }    
                            }
                            else
                            {
                                triggerTypesSelected[i] = EditorGUILayout.ToggleLeft(typeName, triggerTypesSelected[i]);    
                            }
                        }
                        else
                        {
                            triggerTypesSelected[i] = EditorGUILayout.ToggleLeft(typeName, triggerTypesSelected[i]);
                        }
                        
                        var j = i + half;
                        if (j >= triggerTypes.Length) continue;

                        var typeName2 = triggerTypes[j];

                        if (triggerTypesSelected[j] && (GameFeelTriggerType) j == GameFeelTriggerType.OnCustomEvent)
                        {
                            using (new GUILayout.VerticalScope())
                            {
                                triggerTypesSelected[j] = EditorGUILayout.ToggleLeft(typeName2, triggerTypesSelected[j]);
                                EditorGUI.indentLevel += 1;

                                eventName = EditorGUILayout.TextField(GUIContent.none, string.IsNullOrEmpty(eventName) ? "Eg. OnJump" : eventName, GUILayout.Width(120));    
                                
                                EditorGUI.indentLevel -= 1;
                            }
                        }
                        else
                        {
                            triggerTypesSelected[j] = EditorGUILayout.ToggleLeft(typeName2, triggerTypesSelected[j]);
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
            desc.Name = name;
            return desc;
        }
    }
}