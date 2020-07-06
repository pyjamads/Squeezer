#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class StepThroughModeWindow : EditorWindow
    {
        //public GameFeelBehaviorBase trigger;

        public GameFeelTriggerType TriggerType;
        public GameFeelDescription Description;
        public int TriggerIndex;
        
        public List<GameFeelEffectGroup> EffectGroups
        {
            get
            {
                //If the trigger matches us, return the effect groups.
                if(Description.TriggerList.Count >= TriggerIndex && 
                   Description.TriggerList[TriggerIndex].TriggerType == TriggerType)
                {
                    return Description.TriggerList[TriggerIndex].EffectGroups;
                }
                
                return new List<GameFeelEffectGroup>();
            }
        }
        
        public static List<bool> ExpandedDescriptionNames = new List<bool>();
        
        public object[] context;
        public Vector3 triggerPosition;
        public Quaternion triggerRotation;

        public GameFeelEffectGroup effectGroup;
        public string message;
        private SerializedObject serializedObject;

        private const bool showRecipeDebug = false;

        private bool hasAssisted = false;
        
        private Vector2 scrollPosition;


        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
        }

        private void Update()
        {
            if (EditorApplication.timeSinceStartup > EditorHelpers.HighlightUntil)
            {
                Repaint();
            }
        }

        private void EditorApplicationOnplayModeStateChanged(PlayModeStateChange mode)
        {
            Clear();
        }
        
        //TODO: add TOOLTIP: "opens automatically when triggered." 11/06/2020
        [MenuItem("GameFeelDescriptions/Step Through Mode")]
        public static void ShowWindow()
        {
            ShowWindow(null, null, null);
        }
        
        public static void ShowWindow(string message, GameFeelBehaviorBase trigger, GameFeelEffectGroup effectGroup, params object[] context)
        {
            var window = GetWindow<StepThroughModeWindow>("Step Through Mode");
            window.message = message;
            window.effectGroup = effectGroup;
            window.context = context;
            
            //TODO: set window position to not be in front of sceneview, when popping up the first time! -Mads 2020-07-06
            
            if (trigger != null)
            {
                window.triggerPosition = trigger.transform.position;
                window.triggerRotation = trigger.transform.rotation;

                window.Description = trigger.Description;
                window.TriggerIndex = trigger.TriggerIndex;
                window.TriggerType = trigger.TriggerType;
                
                EditorHelpers.HighlightedTriggerIndex = trigger.TriggerIndex;
                EditorHelpers.HighlightUntil = EditorApplication.timeSinceStartup + EditorHelpers.HighlightTime;

                if (effectGroup != null)
                {
                    EditorHelpers.HighlightedEffectGroupIndex = trigger.EffectGroups.IndexOf(effectGroup);
                }
                else
                {
                    EditorHelpers.HighlightedEffectGroupIndex = -1;
                }
            }

            window.Show(); //Might want to ShowModel here!
        }

        private void Clear()
        {
            ExpandedDescriptionNames = new List<bool>();
            triggerPosition = Vector3.zero;
            triggerRotation = Quaternion.identity;
            Description = null;
            TriggerIndex = -1;
            context = null;
            effectGroup = null;
            
            //TODO: consider resetting camera to (0,0,0)
            // SceneView.lastActiveSceneView.FixNegativeSize();
            // SceneView.lastActiveSceneView.LookAt(triggerPosition + Vector3.forward, triggerRotation, 2f);
        }

        private void OnGUI()
        {
            if (Description == null)
            {
                GUILayout.Label("Waiting for Step Through Mode to trigger!");
                return;
            }
            //Get possible effects from trigger type
            var effects = GameFeelBehaviorBase.GetGameFeelEffects(TriggerType, context);
            //Generate a recipe of up to 5 effects, from those effects.
            var recipe = GameFeelBehaviorBase.MakeRecipe(effects);
            
            EditorGUILayout.HelpBox(new GUIContent(message), true);
            
            serializedObject = new SerializedObject(Description);
            
            //Then show the Effect tree, from the trigger
            using (var scroll = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scroll.scrollPosition;
                
                var index = 0;
                GenerateSimpleInterface(Description.TriggerList[TriggerIndex], ref index, 0, "TriggerList", TriggerIndex);
                
                GUILayout.Space(20);
            }

            //TODO: make this automatic for half the people, for the userStudy ... 
            //Assist Me / Accept button!
            if (!hasAssisted && GUILayout.Button("Assist Me!"))
            {
                var desc = (GameFeelDescription)serializedObject.targetObject;
                Undo.RecordObject(desc, "Generate feedback effects!");
                    
                for (int i = 0; i < recipe.Count; i++)
                {
                    if (effectGroup != null)
                    {
                        effectGroup.EffectsToExecute.Add(recipe[i]);
                    }
                    else
                    {
                        if (desc.TriggerList[TriggerIndex].EffectGroups.Count == 0)
                        {
                            desc.TriggerList[TriggerIndex].EffectGroups.Add(new GameFeelEffectGroup
                                {EffectsToExecute = new List<GameFeelEffect> {recipe[i]}});
                        }
                        else
                        {
                            desc.TriggerList[TriggerIndex].EffectGroups[0].EffectsToExecute.Add(recipe[i]);
                                
                        }
                    }    
                }
                serializedObject.ApplyModifiedProperties();

                if (GameFeelDescription.saveDataForUserStudy)
                {
                    var name = DateTime.Now.ToString("s").Replace(':', '.')+
                               "_"+ Description.name + ".txt";

                    GameFeelDescription.SaveToFile(Description, name, GameFeelDescription.userStudyPath);
                }
                    
                //Unpause!
                //EditorApplication.isPaused = false;
                hasAssisted = true;
                return;
            }

            //dialogResult values 2: alt, 1: cancel, 0: ok 
            if (!hasAssisted && GUILayout.Button("Edit in the Inspector")) 
            {
                //TODO: Maybe context + trigger type to setup trigger automatically?!? (Needs to also create the additional !tag trigger, see AttachTriggers in GameFeelDescription)
                
                //Select the Description GameObject in the Scene, Ping it, and highlight the relevant parts in the inspector.
                EditorGUIUtility.PingObject(Description.gameObject);
                Selection.activeGameObject = Description.gameObject;
                EditorHelpers.HighlightUntil = EditorApplication.timeSinceStartup + EditorHelpers.HighlightTime;
                
                //Clear the StepThroughMode Window.
                Clear();
                
                //Also close the window!
                Close();
                return;
            }
            
            //Skip logic!
            if(!hasAssisted && GUILayout.Button("Stop reacting to this!"))
            {
                if (effectGroup != null && effectGroup.StepThroughMode)
                {
                    effectGroup.StepThroughMode = false;
                }
                else if (Description.StepThroughMode && (EffectGroups.Count == 0 || EffectGroups.Any(item => item.EffectsToExecute.Count == 0)))
                {
                    switch (TriggerType)
                    {
                        case GameFeelTriggerType.OnCollision:
                        {
                            var col = (OnCollisionTrigger) Description.TriggerList[TriggerIndex];
                            var other = (GameObject) context[1];
                            var reactTo = other.tag.Equals("Untagged") ? other.name : other.tag;
                            
                            col.ReactTo.Remove("*");
                            col.ReactTo.Add("!" + reactTo);
                            serializedObject.ApplyModifiedProperties();

                            break;
                        }
                        default:
                            //For all the other trigger types, just remove them.
                        {
                            if (EffectGroups.Count == 0)
                            {
                                var sp = serializedObject.FindProperty("TriggerList");
                                sp.DeleteArrayElementAtIndex(TriggerIndex);
                                serializedObject.ApplyModifiedProperties();

                                Destroy(this);    
                            }
                            break;
                        }
                    }
                
                    Clear();
                }
                
                EditorApplication.isPaused = false;
                Close();
                return;
            }

            if (GUILayout.Button("Save & Continue"))
            {
                
                //Clear the StepThroughMode Window.
                Clear();
                
                //Also close the window!
                EditorApplication.isPaused = false;
                Close();
                return;
            }
            
            if (hasAssisted && GUILayout.Button("Discard Generated Effects!"))
            {
                hasAssisted = false;
                Undo.PerformRedo();
            }
        }
        
        private void GenerateSimpleInterface(object container, ref int index, int indent = 0, string parentProperty = null, int dataIndex = 0)
        {
            var propertyPath = "";
            if (parentProperty != null)
            {
                propertyPath += parentProperty+".Array.data[" + dataIndex + "]";
            }

            //var canPaste = false;
            
            var highlightColor = new Color(1f, 1f, 0f, 1f);
            
            var doHighlight = EditorHelpers.HighlightedTriggerIndex == TriggerIndex && EditorApplication.timeSinceStartup < EditorHelpers.HighlightUntil;
            var alpha = (float)((EditorHelpers.HighlightUntil - EditorApplication.timeSinceStartup) / EditorHelpers.HighlightTime);

            var updateScrollPos = false;
            
            if (ExpandedDescriptionNames.Count <= index)
            {
                ExpandedDescriptionNames.Add(false);

                if (container is GameFeelTrigger && EditorHelpers.HighlightedEffectGroupIndex == -1)
                {
                    ExpandedDescriptionNames[index] |= doHighlight;
                }

                updateScrollPos = true;
            }

            EditorGUI.indentLevel = indent++;
            
            
            
            switch (container)
            {
                case GameFeelTrigger trigger:
                {
                    var triggerLabel = trigger.GetType().Name;

                    if (trigger is OnCollisionTrigger col)
                    {
                        triggerLabel += " [reacting to ("+String.Join(", ", col.ReactTo)
                                                         +") "+col.type.GetName()+"]";
                    }
                    else if (trigger is OnMoveTrigger mov)
                    {
                        triggerLabel += " [executing "+mov.type.GetName()+"]";
                    }
                    else if (trigger is OnCustomEventTrigger custom)
                    {
                        triggerLabel += " [reacting to " + custom.EventName + " from "+
                                        (custom.AllowFrom == OnCustomEventTrigger.EventTriggerSources.Sources ? 
                                            "("+string.Join(",", custom.Sources)+")" : 
                                            custom.AllowFrom.GetName())+ "]";
                    }
                    
//                    if (!string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer) && 
//                        JsonUtility.FromJson<GameFeelEffectGroup>(EditorGUIUtility.systemCopyBuffer) != null)
//                    {
//                        canPaste = true;
//                    }
                    
                    var clickArea = ClickAreaWithContextMenu(trigger);
                    
                    if (doHighlight && EditorHelpers.HighlightedEffectGroupIndex == -1)
                    {
                        //DO COLORING, if highlight is enabled, for highlightTime seconds!
                        EditorHelpers.DrawColoredRect(clickArea, highlightColor.withA(alpha));

                        if (updateScrollPos)
                        {
                            scrollPosition = new Vector2(0, index * EditorGUIUtility.singleLineHeight);
                        }
                    }
                    
                    ExpandedDescriptionNames[index] = EditorGUI.Foldout(clickArea, ExpandedDescriptionNames[index], triggerLabel);    
                
                    if (ExpandedDescriptionNames[index])
                    {
                        DrawPropertyWithColor(propertyPath, highlightColor, doHighlight, alpha);
                    }
                    // else
                    // {
                        for (var i = 0; i < trigger.EffectGroups.Count; i++)
                        {
                            if(trigger.EffectGroups[i] == null) continue;
                        
                            index++;
                            GenerateSimpleInterface(trigger.EffectGroups[i], ref index, indent+1, 
                                propertyPath+".EffectGroups", i); 
                        }    
                    // }
                    
                    break;
                }
                case GameFeelEffectGroup group:
                {
//                    if (!string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer) && 
//                        JsonUtility.FromJson<GameFeelEffect>(EditorGUIUtility.systemCopyBuffer) != null)
//                    {
//                        canPaste = true;
//                    }
                    
                    var clickArea = ClickAreaWithContextMenu(group);
                    
                    var prefix = group.Disabled ? "[DISABLED] " : "";
                    var groupLabel = prefix+"EffectGroup "+(string.IsNullOrWhiteSpace(group.GroupName) ? "" : "'"+group.GroupName+"'") 
                                     +" [Applies to "+ (group.ExecuteOnTargetCopy ? "a copy of " : "")
                                     + group.AppliesTo.GetName() + "]";

                    if (doHighlight && EditorHelpers.HighlightedEffectGroupIndex == dataIndex)
                    {
                        //DO COLORING, if highlight is enabled, for highlightTime seconds!
                        EditorHelpers.DrawColoredRect(clickArea, highlightColor.withA(alpha));

                        if (updateScrollPos)
                        {
                            scrollPosition = new Vector2(0, index * EditorGUIUtility.singleLineHeight);
                        }
                    }
                    
                    using (new EditorGUI.DisabledScope(group.Disabled))
                    {
                        ExpandedDescriptionNames[index] = EditorGUI.Foldout(clickArea, ExpandedDescriptionNames[index], groupLabel);    
                    }

                    group.Disabled = !EditorGUI.ToggleLeft(new Rect(clickArea.x - 28f, clickArea.y, clickArea.width, clickArea.height), GUIContent.none, !group.Disabled);
                    
                    if(!group.Disabled)
                    {
                        if (ExpandedDescriptionNames[index])
                        {
                            DrawPropertyWithColor(propertyPath, highlightColor);
                        }
                        // else
                        // {   
                            for (var i = 0; i < group.EffectsToExecute.Count; i++)
                            {
                                if (group.EffectsToExecute[i] == null) continue;

                                index++;
                                GenerateSimpleInterface(group.EffectsToExecute[i], ref index, indent + 1,
                                    propertyPath + ".EffectsToExecute", i);
                            }
                        // }
                    }

                    break;
                }
                case GameFeelEffect effect:
                {
                                        
//                    if (!string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer) && 
//                        JsonUtility.FromJson<GameFeelEffect>(EditorGUIUtility.systemCopyBuffer) != null)
//                    {
//                        canPaste = true;
//                    }
                    
                    var clickArea = ClickAreaWithContextMenu(effect);
                    
                    var prefix = effect.Disabled ? "[DISABLED] " : "";
                    var effectLabel = prefix + effect.GetType().Name;

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(effect.Disabled))
                        {
                            ExpandedDescriptionNames[index] = EditorGUI.Foldout(clickArea,
                                ExpandedDescriptionNames[index], effectLabel, false);
                        }

                        effect.Disabled = !EditorGUI.ToggleLeft(new Rect(clickArea.x - 28f, clickArea.y, clickArea.width, clickArea.height), GUIContent.none, !effect.Disabled);
                    }
                    
                    if(!effect.Disabled)
                    {
                        if (ExpandedDescriptionNames[index])
                        {
                            DrawPropertyWithColor(propertyPath, highlightColor);
                        }
                        // else
                        // {
                            for (var i = 0; i < effect.ExecuteAfterCompletion.Count; i++)
                            {
                                if (effect.ExecuteAfterCompletion[i] == null) continue;

                                index++;
                                GenerateSimpleInterface(effect.ExecuteAfterCompletion[i], ref index, indent + 1,
                                    propertyPath + ".ExecuteAfterCompletion", i);
                            }

                            if (effect is TrailEffect trail)
                            {
                                if (trail.CustomFadeEffects.Count > 0)
                                {
                                    EditorGUI.indentLevel = indent + 1;
                                    var subClickArea = ClickAreaWithContextMenu(trail, false);

                                    EditorGUI.LabelField(subClickArea, "Custom Trail Effect:");

                                    for (var i = 0; i < trail.CustomFadeEffects.Count; i++)
                                    {
                                        if (trail.CustomFadeEffects[i] == null) continue;
                                        index++;
                                        GenerateSimpleInterface(trail.CustomFadeEffects[i], ref index, indent + 2,
                                            propertyPath + ".CustomFadeEffects", i);
                                    }
                                }
                            }
                        // }
                    }
                    break;
                }
            }
            
            #region local functions

            void DrawPropertyWithColor(string path, Color separatorColor, bool highlight = false, float highlightAlpha = 1f)
            {
                EditorGUI.indentLevel = 0;
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    // var rect = EditorGUILayout.GetControlRect();
                    // EditorHelpers.DrawColoredRect(rect, separatorColor.withA(0.9f));

                    serializedObject.FindProperty(path).isExpanded = true;

                    EditorGUI.indentLevel = 1;
                    var rect = EditorGUILayout.GetControlRect(true,
                        EditorGUI.GetPropertyHeight(serializedObject.FindProperty(path), true) +
                        EditorGUIUtility.standardVerticalSpacing);
                    if (highlight)
                    {
                        EditorHelpers.DrawColoredRect(rect, separatorColor.withA(highlightAlpha));
                    }

                    EditorGUI.PropertyField(rect, serializedObject.FindProperty(path), true);
                    serializedObject.ApplyModifiedProperties();


                    EditorGUI.indentLevel = 0;
                    // rect = EditorGUILayout.GetControlRect();
                    // EditorHelpers.DrawColoredRect(rect, separatorColor.withA(0.9f));
                }
            }

            Rect ClickAreaWithContextMenu(object context, bool removeItem = true)
            {
                var clickArea = EditorGUILayout.GetControlRect();
                var current = Event.current;

                if (clickArea.Contains(current.mousePosition) && current.type == EventType.ContextClick)
                {
                    //Do a thing, in this case a drop down menu
                    var menu = new GenericMenu();
                    
                    //Add items from context.
                    switch (context)
                    {
                        case GameFeelDescription desc:
                            {
//                                if (canPaste)
//                                {
//                                    var data = new addCallbackStruct
//                                    {
//                                        isPaste = true,
//                                        context = desc,
//                                        instance = () => JsonUtility.FromJson<GameFeelTrigger>(EditorGUIUtility.systemCopyBuffer),
//                                    };
//                                    menu.AddItem(new GUIContent("Paste Trigger"), false, 
//                                        AddPropertyCallback, data);
//                                }
                                
                                for (var i = 0; i < Enum.GetNames(typeof(GameFeelTriggerType)).Length; i++)
                                {
                                    var type = (GameFeelTriggerType)i;
                                    var typeName = type.GetName();
                                    var data = new addCallbackStruct
                                    {
                                        context = desc,
                                        instance = () =>
                                        {
                                            var trigger = GameFeelTrigger.CreateTrigger(type);
                                            //Add a default Effect Group!
                                            var group = new GameFeelEffectGroup();
                                            //group.GroupName = "List of effects applied to Self "+typeName;
                                            trigger.EffectGroups.Add(group);
                                            
                                            return trigger;
                                        },
                                    };
                                    menu.AddItem(new GUIContent("Add Trigger/" + typeName), false, 
                                        AddPropertyCallback, data);
                                }
                            }
                            break;
                        case GameFeelTrigger trigger:
                            {
//                                menu.AddItem(new GUIContent("Copy Trigger"), false, CopyPropertyCallback, trigger);
//                                if (canPaste)
//                                {
//                                    var pasteData = new addCallbackStruct
//                                    {
//                                        isPaste = true,
//                                        context = trigger,
//                                        instance = () => JsonUtility.FromJson<GameFeelEffectGroup>(EditorGUIUtility.systemCopyBuffer),
//                                    };
//                                    menu.AddItem(new GUIContent("Paste EffectGroup"), false, 
//                                        AddPropertyCallback, pasteData);
//                                }
                                
                                var data = new addCallbackStruct
                                {
                                    context = trigger,
                                    instance = () => new GameFeelEffectGroup()
                                };
                                menu.AddItem(new GUIContent("Add EffectGroup"), false, 
                                    AddPropertyCallback, data);
                            }
                            break;
                        
                        case GameFeelEffectGroup group:
                            {
//                                menu.AddItem(new GUIContent("Copy EffectGroup"), false, CopyPropertyCallback, group);
//                                if (canPaste)
//                                {
//                                    var pasteData = new addCallbackStruct
//                                    {
//                                        isPaste = true,
//                                        context = group.EffectsToExecute,
//                                        instance = () => JsonUtility.FromJson<GameFeelEffect>(EditorGUIUtility.systemCopyBuffer),
//                                    };
//                                    menu.AddItem(new GUIContent("Paste Effect"), false, 
//                                        AddPropertyCallback, pasteData);
//                                }
                                
                                var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));                   
                                foreach (var type in types)
                                { 
                                    // Skip abstract classes because they should not be instantiated
                                    if(type.IsAbstract) 
                                        continue;    
                                    
                                    var data = new addCallbackStruct
                                    {
                                        context = group.EffectsToExecute,
                                        instance = () => Activator.CreateInstance(type)
                                    };
                                    menu.AddItem(new GUIContent("Add Effect/"+ type.Name), false, 
                                        AddPropertyCallback, data);
                                }
                            }
                            break;
                        case GameFeelEffect effect:
                            {
//                                menu.AddItem(new GUIContent("Copy Effect"), false, CopyPropertyCallback, effect);
//                                if (canPaste)
//                                {
//                                    //Regular paste block
//                                    {
//                                        var pasteData = new addCallbackStruct
//                                        {
//                                            isPaste = true,
//                                            context = effect.ExecuteAfterCompletion,
//                                            instance = () => JsonUtility.FromJson<GameFeelEffect>(EditorGUIUtility.systemCopyBuffer),
//                                        };
//                                        menu.AddItem(new GUIContent("Paste as OnComplete Effect"), false, 
//                                            AddPropertyCallback, pasteData);    
//                                    }
//
//                                    if (effect is TrailEffect trail)
//                                    {
//                                        var pasteData = new addCallbackStruct
//                                        {
//                                            isPaste = true,
//                                            context = trail.CustomFadeEffects,
//                                            instance = () => JsonUtility.FromJson<GameFeelEffect>(EditorGUIUtility.systemCopyBuffer),
//                                        };
//                                        menu.AddItem(new GUIContent("Paste as CustomFade Effect"), false, 
//                                            AddPropertyCallback, pasteData);
//                                    }
//                                }
                                
                                var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));                   
                                foreach (var type in types)
                                { 
                                    // Skip abstract classes because they should not be instantiated
                                    if(type.IsAbstract) 
                                        continue;

                                    //Block to separate variable names
                                    {
                                        var data = new addCallbackStruct
                                        {
                                            context = effect.ExecuteAfterCompletion,
                                            instance = () => Activator.CreateInstance(type)
                                        };
                                        menu.AddItem(new GUIContent("Add OnComplete Effect/" + type.Name), false,
                                            AddPropertyCallback, data);
                                    }
                                    
                                    if (effect is TrailEffect trail)
                                    {
                                        var data = new addCallbackStruct
                                        {
                                            context = trail.CustomFadeEffects,
                                            instance = () => Activator.CreateInstance(type)
                                        };
                                        menu.AddItem(new GUIContent("Add CustomFade Effect/"+ type.Name), false, 
                                            AddPropertyCallback, data);
                                    }
                                }
                            }
                            break;
                    }
                    
                    if(removeItem)
                    {
                        menu.AddItem(new GUIContent("Remove Item"), false, RemovePropertyCallback);
                    }
                    menu.ShowAsContext();

                    current.Use();
                }

                return clickArea;
            }

            

            void AddPropertyCallback(object input)
            {
                var isPaste = ((addCallbackStruct) input).isPaste;
                var context = ((addCallbackStruct) input).context;
                var instance = ((addCallbackStruct) input).instance;

                switch (context)
                {    
                    case GameFeelDescription desc:
                    {
                        var triggerInstance = (GameFeelTrigger) instance.Invoke();
                        Undo.RecordObject(Description, isPaste ? "Paste" : ("Add " + triggerInstance.TriggerType.GetName()));
                        Description.TriggerList.Add(triggerInstance);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    case GameFeelTrigger trigger:
                    {
                        Undo.RecordObject(Description, "Add EffectGroup");
                        trigger.EffectGroups.Add((GameFeelEffectGroup) instance.Invoke());
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    //This case handles EffectGroups, ExecuteOnCompletion, and CustomTrailEffects.
                    case List<GameFeelEffect> list:
                    {
                        var effectInstance = (GameFeelEffect) instance.Invoke();
                        Undo.RecordObject(Description, isPaste ? "Paste" : ("Add " + effectInstance.GetType().Name));
                        list.Add(effectInstance);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                }                
            }
            
            void RemovePropertyCallback()
            {
                var sp = serializedObject.FindProperty(parentProperty);
                sp.DeleteArrayElementAtIndex(dataIndex);
                serializedObject.ApplyModifiedProperties();
            }

//            void CopyPropertyCallback(object instance)
//            {
//                var prefix = "";
//                switch (instance)
//                {
//                   case GameFeelDescription desc:
//                       prefix = "0";
//                       break;
//                   case GameFeelTrigger trigger:
//                       prefix = "1";
//                       break;
//                   case GameFeelEffectGroup group:
//                       prefix = "2";
//                       break;
//                   case GameFeelEffect effect:
//                       prefix = "3";
//                       break;
//                }
//                
//                EditorGUIUtility.systemCopyBuffer = prefix+JsonUtility.ToJson(instance); 
//            }

            #endregion
        }
        
        private struct addCallbackStruct
        {
            public bool isPaste;
            public object context;
            public Func<object> instance;
        }
    }
}

#endif