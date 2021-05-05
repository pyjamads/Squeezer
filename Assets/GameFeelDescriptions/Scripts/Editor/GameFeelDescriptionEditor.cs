
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomEditor(typeof(GameFeelDescription))]
    public class GameFeelDescriptionEditor : Editor
    {
        private GameFeelTriggerType selectedTriggerType;
        private int selectedEffectIndex;
        // private StepThroughModeWindow.EffectGeneratorCategories selectedCategory;
        // private int selectedIntensity = 1;
        
        private int attachToIndex = -1;
        
        private int recipeIndex = -1;
        private bool recipeOnly = false;
        private string recipeDescription = "";
        private TextAsset recipeAsset;
        
        private TextAsset descriptionAsset;
        
        private bool simplifiedView = true;
        private bool showAttach = false;
        private bool showDescriptionSettings = false;

        private bool showBottomButtons = false;

        private bool canPaste = false;

        private static object copiedObject;
        
        public static Dictionary<int, List<bool>> ExpandedDescriptionNames = new Dictionary<int, List<bool>>();
        public bool showGenerators;

        //TODO: Consider making these static, to cache them between instances.
        //[SerializeField] // ?!? does this make sense here?
        private static List<Type> gameFeelEffects;
        private static string[] gameFeelEffectNames;

        private float lastSaveTime;
        private double editorTimeAtLastUpdate;

        private GameObject previewObject;
        private float LastPreviewTime = 0;
        
        //private Scene theScene;

        private void OnEnable()
        {
            Init();
            
            Selection.selectionChanged += ClearLingeringPreview;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDestroy()
        {
            Selection.selectionChanged -= ClearLingeringPreview;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
               ClearLingeringPreview();
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                GameFeelEffectExecutor.DestroyInstance();
                GameFeelEffectExecutor.applicationIsQuitting = false;
            }
        }
        
        private void ClearLingeringPreview()
        {
            if ((target as GameFeelDescription)?.PreviewMode == 1)
            {
                GameFeelEffectExecutor.DestroyInstance();
            }
        }
        
        public override bool RequiresConstantRepaint()
        {
            return EditorApplication.timeSinceStartup > EditorHelpers.HighlightUntil || base.RequiresConstantRepaint();
        }

        void Init()
        {
            var instanceID = target.GetInstanceID();
            if (!ExpandedDescriptionNames.ContainsKey(instanceID))
            {
                ExpandedDescriptionNames[target.GetInstanceID()] = new List<bool>();
            }
            
            var type = typeof(GameFeelEffect);
            var assembly = type.Assembly; 
            var types = assembly.GetTypes();
            var subtypes = types.Where(item => item.IsSubclassOf(type) && !item.IsAbstract);
            gameFeelEffects = new List<Type>();
            gameFeelEffects.AddRange(subtypes);
            gameFeelEffectNames = new string[gameFeelEffects.Count];
            for (var index = 0; index < gameFeelEffects.Count; index++)
            {
                gameFeelEffectNames[index] = gameFeelEffects[index].Name;
            }
        }
        
        #region tiny Preview stuff

        //TODO: make custom preview renderer! 09/06/2020
        // public override bool HasPreviewGUI() { return showAttach; }
        //
        // //private PreviewRenderUtility _previewRenderUtility;
        // Editor gameObjectEditor;
        // private List<GameObject> attachedObjects;
        //
        // public override void OnPreviewGUI(Rect r, GUIStyle background)
        // {
        //     if (showDescriptionSettings && showAttach)
        //     {
        //         if (gameObjectEditor == null)
        //         {
        //             attachedObjects = (target as GameFeelDescription).FindGameObjectsToAttach();
        //             gameObjectEditor = Editor.CreateEditor(attachedObjects.ToArray());
        //         }
        //
        //         // var stage = PrefabStageUtility.GetPrefabStage(attachedObject);
        //         //  stage.scene.
        //
        //         //Setup constant repaint for showing things...
        //         // ((GameObject)gameObjectEditor.target).transform.localScale = TweenHelper.Interpolate(originalSize,
        //         //     (float)EditorApplication.timeSinceStartup % 2f, Vector3.one,
        //         //     EasingHelper.Ease(EasingHelper.EaseType.BackOut));
        //         //
        //     
        //         // EditorWindow editorWindow = EditorWindow.GetWindow(typeof(CameraEditor));
        //         //
        //         // //editorWindow.autoRepaintOnSceneChange = true;
        //         // editorWindow.Show();
        //     
        //         gameObjectEditor.OnPreviewGUI(r, background);
        //     }
        //     else
        //     {
        //         gameObjectEditor = null;
        //         attachedObjects = null;
        //     }
        // }
        
        #endregion

        private Vector2 scrollPosition = Vector2.zero;

        public override void OnInspectorGUI()
        {   
            EditorGUI.BeginChangeCheck();
            
            #region draw inspector
            var gameFeelDescription = (GameFeelDescription) target;
            
            //note the scene!
            //theScene = gameFeelDescription.gameObject.scene;

            var style = new GUIStyle();
            style.border = new RectOffset(5,5,5,5);
            style.padding = new RectOffset(50,0,0,0);
            style.alignment = TextAnchor.MiddleRight;

            if (!ExpandedDescriptionNames.ContainsKey(target.GetInstanceID()))
            {
                Init();
            }
            
            EditorGUILayout.HelpBox(new GUIContent("Game Feel Description contains all the effects triggered with various triggers on objects with the tag defined in AttachTo. " +
                                                   "\nA Descriptions contain a list of Triggers, which determine when their EffectGroups get executed." +
                                                   "\nAn EffectGroup is a collections of Effects applied to some target."));
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            //Uncomment to Right align
//            rect.width = 120; 
//            rect.x = EditorGUIUtility.currentViewWidth - rect.width - 15; // 15 is for the scroll view offset
            simplifiedView = EditorGUI.ToggleLeft(rect, "Simplified View", simplifiedView);

            
            //GUILayout.Space(20);
            
            if (simplifiedView)
            {
                var indent = EditorGUI.indentLevel;
                var index = 0;
                GenerateSimpleInterface(gameFeelDescription, ref index, indent);
                EditorGUI.indentLevel = indent;
            }
            else
            {
                
                using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
                {
                    //----------------------------//
                    DrawDefaultInspector();
                    //----------------------------//
                    
                    scrollPosition = scrollViewScope.scrollPosition;
                }
            }
                
            #endregion

            GUILayout.Space(20);
            
            showBottomButtons = EditorGUILayout.Foldout(showBottomButtons, "Additional helpers", true);

            if (showBottomButtons)
            {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    #region add triggers

                    //Create trigger block
                    {
                        EditorGUILayout.HelpBox(new GUIContent(
                            "Add Triggers to determine when effects are executed. \nTriggers determine when EffectGroups get executed."));

                        selectedTriggerType =
                            (GameFeelTriggerType) EditorGUILayout.EnumPopup("Trigger", selectedTriggerType);

                        if (GUILayout.Button("Add Trigger"))
                        {
                            Undo.RecordObject(gameFeelDescription, "Add " + selectedTriggerType.GetName());
                            gameFeelDescription.TriggerList.Add(GameFeelTrigger.CreateTrigger(selectedTriggerType));
                            serializedObject.ApplyModifiedProperties();
                        }
                    }

                    #endregion

                    #region add effect recipes

                    if (gameFeelDescription.TriggerList.Count != 0)
                    {
                        GUILayout.Space(20);
                        
                        EditorGUILayout.HelpBox(new GUIContent("Effect groups can be saved and loaded as recipes!"));
                        var (groupNames, groupRefs) =
                            GenerateLabelObjectLists(gameFeelDescription, typeof(GameFeelEffectGroup));

                        //Make sure we're not selecting something out of bounds, because the user removed a value.
                        if (recipeIndex >= groupNames.Count)
                        {
                            recipeIndex = -1;
                        }

                        recipeIndex = EditorGUILayout.Popup("EffectGroup", recipeIndex, groupNames.ToArray());

                        if (recipeIndex >= 0)
                        {
                            var gameFeelEffectGroup = ((GameFeelEffectGroup) groupRefs[recipeIndex]);

                            if (gameFeelEffectGroup != null)
                            {
                                recipeOnly = EditorGUILayout.Toggle("Only save effect list", recipeOnly);

                                if (recipeOnly)
                                {
                                    EditorGUILayout.LabelField("Description added to the effect list:");
                                    recipeDescription = EditorGUILayout.TextArea(recipeDescription);

                                    if (GUILayout.Button("Save As Recipe"))
                                    {
                                        var groupName = gameFeelEffectGroup.GroupName;
                                        if (string.IsNullOrWhiteSpace(groupName))
                                        {
                                            groupName = gameFeelDescription.Name + "EffectGroup" + recipeIndex;
                                        }

                                        gameFeelEffectGroup.SaveToRecipe(true, recipeDescription,
                                            groupName + "Recipe.txt");
                                    }
                                }
                                else
                                {
                                    if (GUILayout.Button("Save As EffectGroup"))
                                    {
                                        var groupName = gameFeelEffectGroup.GroupName;
                                        if (string.IsNullOrWhiteSpace(groupName))
                                        {
                                            groupName = gameFeelDescription.Name + "EffectGroup" + recipeIndex;
                                        }

                                        gameFeelEffectGroup.SaveToEffectGroup(true, groupName + "Recipe.txt");
                                    }
                                }

                                recipeAsset = (TextAsset) EditorGUILayout.ObjectField("Load from Recipe File",
                                    recipeAsset, typeof(TextAsset), false);
                                if (recipeAsset != null)
                                {
                                    var loadedRecipe = GameFeelEffectGroup.LoadRecipeFromJson(recipeAsset.text);
                                    if (loadedRecipe.Name != null)
                                    {
                                        if (GUILayout.Button("Add effects from Recipe"))
                                        {
                                            gameFeelEffectGroup.AddEffectsFromRecipe(loadedRecipe);
                                        }

                                        if (GUILayout.Button("Replace with Recipe"))
                                        {
                                            gameFeelEffectGroup.ReplaceEffectsWithRecipe(loadedRecipe);
                                        }
                                    }
                                    else
                                    {
                                        var loadedGroup = GameFeelEffectGroup.LoadEffectGroupFromJson(recipeAsset.text);

                                        if (loadedGroup.GroupName == null)
                                        {
                                            Debug.LogError(
                                                "json file was neither GameFeelRecipe or GameFeelEffectGroup");
                                        }
                                        else
                                        {
                                            if (GUILayout.Button("Add effects from EffectGroup"))
                                            {
                                                gameFeelEffectGroup.AddEffectsFromRecipe(loadedGroup);
                                            }

                                            if (GUILayout.Button("Replace with EffectGroup"))
                                            {
                                                gameFeelEffectGroup.ReplaceEffectsWithRecipe(loadedGroup);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region add effects

                    if (gameFeelDescription.TriggerList.Count != 0)
                    {
                        GUILayout.Space(20);
                        EditorGUILayout.HelpBox(
                            new GUIContent("Add Effects to the list of Effects this Game Feel Description executes."));

                        selectedEffectIndex = EditorGUILayout.Popup("Effect", selectedEffectIndex, gameFeelEffectNames);

                        var (names, refs) = GenerateLabelObjectLists(gameFeelDescription);

                        //Make sure we're not selecting something out of bounds, because the user removed a value.
                        if (attachToIndex >= names.Count)
                        {
                            attachToIndex = -1;
                        }

                        attachToIndex = EditorGUILayout.Popup("Attach to", attachToIndex, names.ToArray());

                        if (GUILayout.Button("Add Effect"))
                        {
                            if (refs[attachToIndex] == null) return;

                            if (gameFeelEffects == null || gameFeelEffects.Count == 0)
                            {
                                Init();
                            }

                            var effectType = gameFeelEffects[selectedEffectIndex];

                            var instance = (GameFeelEffect) Activator.CreateInstance(effectType);

                            Undo.RecordObject(gameFeelDescription, "Add " + effectType.Name);
                            switch (refs[attachToIndex])
                            {
                                case GameFeelEffect effect:
                                    effect.ExecuteAfterCompletion.Add(instance);
                                    break;
                                case GameFeelEffectGroup group:
                                    @group.EffectsToExecute.Add(instance);
                                    break;
                                case GameFeelTrigger trigger:
                                    if (trigger.EffectGroups.Count == 0)
                                    {
                                        trigger.EffectGroups.Add(new GameFeelEffectGroup
                                            {EffectsToExecute = new List<GameFeelEffect> {instance}});
                                    }
                                    else
                                    {
                                        trigger.EffectGroups[0].EffectsToExecute.Add(instance);
                                    }

                                    break;
                            }

                            serializedObject.ApplyModifiedProperties();
                        }

                        //Extra button for CustomFadeEffect on trails.
                        if (0 < attachToIndex && attachToIndex < refs.Count &&
                            refs[attachToIndex] is SpawningGameFeelEffect spawner &&
                            GUILayout.Button("Add Offspring Effect"))
                        {
                            if (gameFeelEffects == null || gameFeelEffects.Count == 0)
                            {
                                Init();
                            }

                            var effectType = gameFeelEffects[selectedEffectIndex];
                            var instance = (GameFeelEffect) Activator.CreateInstance(effectType);

                            Undo.RecordObject(gameFeelDescription, "Add " + effectType.Name);
                            spawner.ExecuteOnOffspring.Add(instance);

                            serializedObject.ApplyModifiedProperties();
                        }
                    }

                    #endregion

                    #region save/load description

                    GUILayout.Space(20);
                    EditorGUILayout.HelpBox(new GUIContent("Save and Load entire Descriptions!"));

                    if (gameFeelDescription.TriggerList.Count > 0)
                    {
                        if (GUILayout.Button("Save Description"))
                        {
                            GameFeelDescription.SaveToFile(gameFeelDescription, gameFeelDescription.Name + ".txt");
                        }
                    }

                    descriptionAsset = (TextAsset) EditorGUILayout.ObjectField("Load from Description File",
                        descriptionAsset, typeof(TextAsset), false);

                    if (GUILayout.Button("Load Description"))
                    {
                        var data = GameFeelDescription.LoadDescriptionFromJson(descriptionAsset.text);

                        Undo.RecordObject(gameFeelDescription, "Loaded " + descriptionAsset.name + ".");
                        gameFeelDescription.OverrideDescriptionData(data);
                        serializedObject.ApplyModifiedProperties();
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }

                    #endregion
                }
            }

            var changed = EditorGUI.EndChangeCheck();
            
            //Save every 5 seconds while we're doing the user study.
            if (GameFeelDescription.saveDataForUserStudy && changed)//&& EditorApplication.timeSinceStartup - lastSaveTime > 5) 
            {
                GameFeelDescription.SaveToFile(gameFeelDescription, 
                    DateTime.Now.ToString("s").Replace(':', '.')+
                    (EditorApplication.isPlaying? "_P" : "") +
                    "_"+ gameFeelDescription.name + ".txt", 
                    path: GameFeelDescription.userStudyPath);
            }
        }

        private (List<string> names, List<object> refs) GenerateLabelObjectLists(object container, Type type = null, int index = 0, int indent = 0, string prefix = null)
        {
            var objs = new List<object>();
            var list = new List<string>();

            var indentString = "";
            
            for (int j = 1; j < indent; j++)
            {
                indentString += " |";
            }

            if (indent > 0)
            {
                indentString += " ↳";
            }
            
            if (!string.IsNullOrEmpty(prefix))
            {
                indentString += prefix;
            }
            
            switch (container)
            {
                case GameFeelDescription desc:
                {   
                    for (int i = 0; i < desc.TriggerList.Count; i++)
                    {
                        if (desc.TriggerList[i] == null) continue;

                        var (names, refs) = GenerateLabelObjectLists(desc.TriggerList[i], type, i, indent);
                        
                        list.AddRange(names);
                        objs.AddRange(refs);
                    }

                    break;
                }
                case GameFeelEffect effect:
                {
                    //Skip all game feel effects and sub effects!
                    if (type != null && type != typeof(GameFeelEffect)) break;
                        
                    list.Add(indentString + (index) + ". " + ObjectNames.NicifyVariableName(effect.GetType().Name));
                    objs.Add(effect);
                
                    for (int i = 0; i < effect.ExecuteAfterCompletion.Count; i++)
                    {
                        if (effect.ExecuteAfterCompletion[i] == null) continue;
                        
                        var subList = GenerateLabelObjectLists(effect.ExecuteAfterCompletion[i], type, i, indent+1, prefix);
                        list.AddRange(subList.names);
                        objs.AddRange(subList.refs);   
                    }
                
                    if (effect is SpawningGameFeelEffect spawner)
                    {
                        for (int i = 0; i < spawner.ExecuteOnOffspring.Count; i++)
                        {
                            if (spawner.ExecuteOnOffspring[i] == null) continue;
                            var subList = GenerateLabelObjectLists(spawner.ExecuteOnOffspring[i], type, i, indent+1,"OnOffspring: ");
                            list.AddRange(subList.names);
                            objs.AddRange(subList.refs);  
                        }
                    }
                    break;
                }
                case GameFeelEffectGroup group:
                {
                    //Skip adding groups!
                    if (type == null || type == typeof(GameFeelEffectGroup))
                    {
                        list.Add(indentString + (index) + ". " + "EffectGroup [AppliesTo: " +group.AppliesTo.GetName()+"]");
                        objs.Add(group);
                    }
                    
                    for (int i = 0; i < group.EffectsToExecute.Count; i++)
                    {
                        if (group.EffectsToExecute[i] == null) continue;

                        var effectList = GenerateLabelObjectLists(group.EffectsToExecute[i], type, i, indent + 1);
                        list.AddRange(effectList.names);
                        objs.AddRange(effectList.refs);
                    }

                    break;
                }
                case GameFeelTrigger trigger:
                {
                    //Skip adding triggers!
                    if (type == null || type == typeof(GameFeelTrigger))
                    {
                        var triggerLabel = indentString + (index) + ". " + ObjectNames.NicifyVariableName(trigger.GetType().Name);

                        if (trigger is OnCollisionTrigger col)
                        {
                            triggerLabel += " ["+col.type.GetName()+" (" + String.Join(",", col.ReactTo) + ")]";
                        }
                        else if (trigger is OnMoveTrigger mov)
                        {
                            triggerLabel += " [" + mov.type.GetName() + "]";
                        }
                        else if (trigger is OnCustomEventTrigger custom)
                        {
                            triggerLabel += " [" + custom.EventName + " ("+custom.AllowFrom.GetName()+")]";
                        }

                        list.Add(triggerLabel);
                        objs.Add(trigger);
                    }

                    for (int i = 0; i < trigger.EffectGroups.Count; i++)
                    {
                        if(trigger.EffectGroups[i] == null) continue;
                            
                        var (names, refs) = GenerateLabelObjectLists(trigger.EffectGroups[i], type, i, indent+1); //, trigger.GetType().Name+ ": "
                        list.AddRange(names);
                        objs.AddRange(refs); 
                    }
                    break;
                }
            }

            return (list, objs);
        }
        
        private void GenerateSimpleInterface(object container, ref int index, int indent = 0, string parentProperty = null, int dataIndex = 0, bool doHighlight = false, float totalExecutionTime = 0f)
        {
            var propertyPath = "";
            if (parentProperty != null)
            {
                propertyPath += parentProperty+".Array.data[" + dataIndex + "]";
            }

            var toggleOnLabel = false;
            
            #if UNITY_2019_4 //_OR_NEWER ...TODO: test this with different versions 2021-02-15
                toggleOnLabel = true;
            #endif
            
            //var canPaste = false;
            
            var highlightColor = new Color(1f, 1f, 0f, 1f);
            var alpha = (float)((EditorHelpers.HighlightUntil - EditorApplication.timeSinceStartup) / EditorHelpers.HighlightTime);
            
            if (ExpandedDescriptionNames[target.GetInstanceID()].Count <= index)
            {
                ExpandedDescriptionNames[target.GetInstanceID()].Add(false);
            }
            
            EditorGUI.indentLevel = indent++;
            
            switch (container)
            {
                case GameFeelDescription desc:
                {
                    showDescriptionSettings = EditorGUILayout.Foldout(showDescriptionSettings, "Description Settings", toggleOnLabel);
                    
                    if (showDescriptionSettings)
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));

                            GUILayout.Label("Choose how to attach the description to objects:", EditorStyles.boldLabel);
                            //EditorGUI.indentLevel += 1;
                            
                            //showAttach = EditorGUILayout.Foldout(showAttach, "AttachTo");
                            //EditorGUI.indentLevel -= 1;

                            //if (showAttach) //Don't do foldout inside foldout here!
                            {
                                using (new GUILayout.VerticalScope( "AttachTo", EditorStyles.helpBox))
                                {
                                    EditorGUI.indentLevel += 1;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DontDestroyOnLoad"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DynamicReattachRate"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToTag"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToObjects"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToComponentType"));
                                    EditorGUI.indentLevel -= 1;
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                    
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("StepThroughMode"));
                    if (desc.StepThroughMode)
                    {
                        EditorGUILayout.HelpBox(new GUIContent(
                            "StepThroughMode let's you add effects as collisions between objects occur.\n" +
                            "In addition when StepThroughMode is enabled, \n" +
                            "PlayMode changes to the description will persist in EditMode."));
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                    
                    GUILayout.Space(20);

                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        var previewString = "Preview settings:";

                        if (!desc.PreviewExpanded)
                        {
                            previewString = "Preview [";

                            if (desc.PreviewTriggerInterval > 0 && desc.PreviewMode > 0)
                            {
                                if (desc.PreviewMode == 1)
                                {
                                    previewString += "SCENE";
                                }
                                else
                                {
                                    previewString += "WINDOW";
                                }

                                previewString += "] Trigger Interval [" + desc.PreviewTriggerInterval.ToString("F1") +
                                                 "s]";
                            }
                            else
                            {
                                //NOTE: Maybe it's okay to leave "OFF" as "Manual", but this forces you to make a choice!
                                if (desc.PreviewMode > 0)
                                {
                                    previewString += "MANUAL]";
                                }
                                else
                                {
                                    previewString += "OFF]";
                                }
                            }

                            previewString += " (expand for more settings):";
                        }



                        desc.PreviewExpanded = EditorGUILayout.Foldout(desc.PreviewExpanded,
                            previewString, true, EditorStyles.foldoutHeader);
                        //GUILayout.Label("Preview settings:", EditorStyles.boldLabel);

                        if (desc.PreviewExpanded)
                        {
                            desc.PreviewTriggerInterval = EditorGUILayout.FloatField(
                                "Trigger Interval" + ((desc.PreviewTriggerInterval <= 0)
                                    ? " (Automatic for values > 0)"
                                    : " (Manual for values <= 0)"), desc.PreviewTriggerInterval);
                            desc.PreviewDirection = EditorGUILayout.Vector3Field("Direction", desc.PreviewDirection);
                            desc.PreviewPositionOffset =
                                EditorGUILayout.Vector3Field("Position Offset", desc.PreviewPositionOffset);

                            if (desc.PreviewMode == 1 && previewObject != null && GUILayout.Button("Reset Preview Object!"))
                            {
                                //Hard reset the scene target object!
                                DestroyImmediate(previewObject);
                                previewObject = null;
                                GameFeelEffectExecutor.DestroyInstance();
                            }
                            else if (desc.PreviewMode == 2 && GUILayout.Button("Reset Preview!"))
                            {
                                //Hard reset the window!
                                if (EditorWindow.HasOpenInstances<PreviewWindow>())
                                {
                                    EditorWindow.GetWindow<PreviewWindow>().Close();
                                }
                            }

                            if (GUILayout.Button("Toggle object visibility"))
                            {
                                if (EditorWindow.HasOpenInstances<PreviewWindow>() && PreviewWindow.target != null)
                                {
                                    //TODO: make this better, maybe as a preview setting, so it just doesn't copy it over. 2021-02-08
                                    //NOTE: very confusing the way this makes it scale down, and fade out, instead of just disappearing. (seems to only happen when we are running preview)
                                    PreviewWindow.target.SetActive(!PreviewWindow.target.activeSelf);
                                }
                            }
                            
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.HelpBox(new GUIContent(
                                    "Scene Mode previews the effect directly in the scene. " +
                                    "\nThis will alter the scene itself, which can lead to lingering " +
                                    "\nObject Transformations, Color Changes and Components."));

                                EditorGUILayout.HelpBox(new GUIContent("Preview Window Mode opens a preview scene " +
                                                                       "\nwith a copy og the target object. Known Issues: " +
                                                                       "\nCamera Shake and targeting Tags / Component type in Effect Groups."));
                            }
                        }

                        EditorGUI.BeginChangeCheck();
                        var previewModeTemp = desc.PreviewMode;
                        desc.PreviewMode = GUILayout.Toolbar(desc.PreviewMode,
                            new[] {"Off", "Scene Mode", "Preview Window"});

                        if (EditorGUI.EndChangeCheck())
                        {
                            //If it changed and came from Scene Mode, clear lingering effects.
                            if (previewModeTemp == 1)
                            {
                                GameFeelEffectExecutor.ClearLingeringEffects();
                            }
                            //If it changed and came from Preview window, clear/close that window.
                            else if (previewModeTemp == 2)
                            {
                                if (EditorWindow.HasOpenInstances<PreviewWindow>())
                                {
                                    var window = EditorWindow.GetWindow<PreviewWindow>();
                                    window.Close();
                                }
                            }

                            if (desc.PreviewMode > 0)
                            {
                                EditorApplication.QueuePlayerLoopUpdate();
                            }
                        }

                        //Always do this check, in order to open the window, when switching to a different description.
                        if (desc.PreviewMode == 2 && !EditorWindow.HasOpenInstances<PreviewWindow>())
                        {
                            //var window = EditorWindow.GetWindow<PreviewWindow>();
                            PreviewWindow.ShowWindow();
                        }


                        if (desc.PreviewTriggerInterval > 0 && desc.PreviewMode > 0 &&
                            Time.realtimeSinceStartup - LastPreviewTime > desc.PreviewTriggerInterval)
                        {
                            LastPreviewTime = Time.realtimeSinceStartup;

                            var targetInAList = new List<GameObject>();
                            if (desc.PreviewMode == 2 && PreviewWindow.target != null)
                            {
                                targetInAList.Add(PreviewWindow.target);
                            }
                            else if(desc.PreviewMode == 1 && previewObject != null)
                            {
                                targetInAList.Add(previewObject);
                            }

                            //Grab the first available attachTarget.
                            if (targetInAList.Count == 0)
                            {
                                //Grab the first available attachTarget.
                                var attachTargets = desc.FindGameObjectsToAttach();

                                if (attachTargets.Count > 0)
                                {
                                    targetInAList = attachTargets.GetRange(0, 1);

                                    targetInAList[0] = Instantiate(targetInAList[0],
                                        GameFeelEffectExecutor.Instance.transform, true);

                                    //In Scene Mode
                                    if (desc.PreviewMode == 1)
                                    {
                                        //Uniformly Scale the object up a tiny bit, so it "hides" the original object.  
                                        targetInAList[0].transform.localScale *= 1.001f;
                                        previewObject = targetInAList[0];
                                    }
                                }
                                else
                                {
                                    //TODO: consider searching for the object in prefabs or something... 2021-02-15
                                    //First round: a primitive cuboid with different scales.
                                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    gameObject.transform.localScale = new Vector3(0.5f, 1f, 0.2f);
                                    gameObject.transform.parent = GameFeelEffectExecutor.Instance.transform;
                                    targetInAList.Add(gameObject);
                                    previewObject = gameObject;
                                }
                            }

                            foreach (var trigger in desc.TriggerList)
                            {
                                foreach (var effectGroup in trigger.EffectGroups)
                                {
                                    var position = Vector3.zero;
                                    if (targetInAList.Count > 0 && targetInAList[0] != null)
                                    {
                                        position = targetInAList[0].transform.position;
                                    }

                                    effectGroup.InitializeAndQueueEffects(null, targetInAList,
                                        new PositionalData(
                                            position + desc.PreviewPositionOffset,
                                            desc.PreviewDirection));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Preview Disabled during PlayMode!");
                    }
                    GUILayout.Space(20);
                    

                    GUILayout.Label("Effect Tree (Click elements to edit, or right click for options):", EditorStyles.boldLabel);
                    
//                    desc.StepThroughMode = EditorGUILayout.Toggle("Step Through Mode", desc.StepThroughMode);
//                    
                    if (copiedObject is GameFeelTrigger)
                    {
                        canPaste = true;
                    }
                    else
                    {
                        canPaste = false;
                    }

                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
                    {
                        var clickArea = ClickAreaWithContextMenu(desc, false);
                        if (desc.TriggerList.Count == 0)
                        {
                            if (desc.StepThroughMode)
                            {
                                EditorGUI.LabelField(clickArea,
                                    "Trigger List is Empty, hit play to begin StepThroughMode!");
                            }
                            else
                            {
                                EditorGUI.LabelField(clickArea,
                                    "Trigger List is Empty, right click to add a Trigger or enable StepThroughMode!");
                            }
                        }
                        else
                        {
                            var seperator = !string.IsNullOrEmpty(desc.AttachToTag) && !string.IsNullOrEmpty(desc.AttachToComponentType)
                                ? " and "
                                : "";

                            var andList = desc.AttachToObjects?.Length > 0
                                ? (string.IsNullOrEmpty(seperator) ? "list" : " and list")
                                : "";

                            EditorGUI.LabelField(new Rect(clickArea.x, clickArea.y, clickArea.width - 50f, clickArea.height), "Trigger List [Attaching "
                                                                                                                              + desc.TriggerList.Count + " trigger(s) to " +
                                                                                                                              desc.AttachToTag +
                                                                                                                              seperator +
                                                                                                                              desc.AttachToComponentType +
                                                                                                                              andList + "]");
                            
                            if (GUI.Button(new Rect(clickArea.x + clickArea.width - 50f, clickArea.y, 50f, clickArea.height),"+"))
                            {
                                PlusMenuDropdown(desc);
                            }
                            
                            indent++;
                            
                            for (var i = 0; i < desc.TriggerList.Count; i++)
                            {
                                if (desc.TriggerList[i] == null) continue;

                                GenerateSimpleInterface(desc.TriggerList[i], ref index, indent,
                                    "TriggerList", i);
                                index++;
                            }
                            
                            indent--;
                        }
                        
                        scrollPosition = scrollViewScope.scrollPosition;
                    }
                    

                    break;
            }
                case GameFeelTrigger trigger:
                {
                    var prefix = trigger.Disabled ? "[DISABLED] " : "";
                    var triggerLabel = prefix + ObjectNames.NicifyVariableName(trigger.GetType().Name);

                    if (trigger is OnCollisionTrigger col)
                    {
                        triggerLabel += " [reacting to ("+String.Join(", ", col.ReactTo)
                                                         +") "+ObjectNames.NicifyVariableName(col.type.GetName())+"]";
                    }
                    else if (trigger is OnMoveTrigger mov)
                    {
                        triggerLabel += " [executing "+ObjectNames.NicifyVariableName(mov.type.GetName())+"]";
                    }
                    else if (trigger is OnCustomEventTrigger custom)
                    {
                        triggerLabel += " [reacting to " + custom.EventName + " from "+
                                        (custom.AllowFrom == OnCustomEventTrigger.EventTriggerSources.Sources ? 
                                            "("+string.Join(",", custom.Sources)+")" : 
                                            custom.AllowFrom.GetName())+ "]";
                    }

                    doHighlight = EditorHelpers.HighlightedTriggerIndex == dataIndex && EditorApplication.timeSinceStartup < EditorHelpers.HighlightUntil;

                    if (copiedObject is GameFeelEffectGroup)
                    {
                        canPaste = true;
                    }
                    else
                    {
                        canPaste = false;
                    }
                    
                    var clickArea = ClickAreaWithContextMenu(trigger);
                    var indented = EditorGUI.IndentedRect(clickArea);
                    
                    if (doHighlight)
                    {
                        //DO COLORING, if highlight is enabled, for highlightTime seconds!
                        EditorHelpers.DrawColoredRect(clickArea, highlightColor.withA(alpha));
                        
                        if (EditorHelpers.HighlightedEffectGroupIndex == -1)
                        {
                            ExpandedDescriptionNames[target.GetInstanceID()][index] = true;    
                        }
                    }

                    trigger.Disabled =
                        !EditorGUI.ToggleLeft(
                            new Rect(clickArea.x - 28f, clickArea.y, clickArea.width - indented.width + 15f,
                                clickArea.height),
                            GUIContent.none, !trigger.Disabled);
                    
                    using (new EditorGUI.DisabledScope(trigger.Disabled))
                    {
                        ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(
                            new Rect(clickArea.x, clickArea.y, indented.width - 100f - EditorGUIUtility.standardVerticalSpacing, clickArea.height),
                            ExpandedDescriptionNames[target.GetInstanceID()][index], triggerLabel, toggleOnLabel);
                    }

                    if (!trigger.Disabled)
                    {
                        if (ExpandedDescriptionNames[target.GetInstanceID()][index])
                        {
                            // EditorGUI.indentLevel += 1;
                            // ExpandedDescriptionNames[target.GetInstanceID()][index][1] = EditorGUILayout.Foldout(ExpandedDescriptionNames[target.GetInstanceID()][index][1], "Trigger Properties");
                            // EditorGUI.indentLevel -= 1;
                            // if (ExpandedDescriptionNames[target.GetInstanceID()][index][1])
                            // {
                            DrawPropertyWithColor(propertyPath, highlightColor, doHighlight, alpha);
                            // }
                        }
                        // else
                        // {

                        // if (GUILayout.Button("x"))
                        // {
                        //     RemovePropertyCallback();
                        // }
                        
                        var desc = (GameFeelDescription)target;
                        
                        if (desc.PreviewMode > 0 && GUI.Button(new Rect(indented.x + indented.width - 100f - EditorGUIUtility.standardVerticalSpacing, indented.y, 50f, indented.height),">"))
                        {
                            //If there's no lingering effects
                            if (GameFeelEffectExecutor.Instance.activeEffects.Count == 0)
                            {
                                GameFeelEffectExecutor.DestroyInstance();
                            }
                            
                            //Ignore resetting the state for now! TODO: actually reset the state or use a preview window!

                            var targetInAList = new List<GameObject>();
                            if (EditorWindow.HasOpenInstances<PreviewWindow>() && PreviewWindow.target != null)
                            {
                                targetInAList.Add(PreviewWindow.target);
                            }
                            
                            if(targetInAList.Count == 0)
                            {
                                //Grab the first available attachTarget.
                                var attachTargets = desc.FindGameObjectsToAttach();
                            
                                if (attachTargets.Count > 0)
                                {
                                    targetInAList = attachTargets.GetRange(0, 1);

                                    targetInAList[0] = Instantiate(targetInAList[0], GameFeelEffectExecutor.Instance.transform, true);
                                    
                                    //In Scene Mode
                                    if (desc.PreviewMode == 1)
                                    {
                                        //Uniformly Scale the object up a tiny bit, so it "hides" the original object.  
                                        targetInAList[0].transform.localScale *= 1.001f;    
                                    }
                                    
                                    // Undo.RecordObject(attachTargets[0], "preview effect");
                                    // //TODO: maybe hide the original object, and show it again after the preview is done!
                                    // previewObject = attachTargets[0];
                                    // previewObject.SetActive(false);
                                }
                                else
                                {
                                    //TODO: consider searching for the object in prefabs or something... 2021-02-15
                                    //First round: a primitive cuboid with different scales.
                                    var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                    gameObject.transform.localScale = new Vector3(0.5f, 1f, 0.2f);
                                    gameObject.transform.parent = GameFeelEffectExecutor.Instance.transform;
                                    targetInAList.Add(gameObject);
                                    previewObject = gameObject;
                                }
                            }
                            
                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                var previewTargetPosition = Vector3.zero;
                                if (targetInAList.Count > 0 && targetInAList[0] != null)
                                {
                                    previewTargetPosition = targetInAList[0].transform.position;
                                }

                                effectGroup.InitializeAndQueueEffects(null, targetInAList,
                                        new PositionalData(
                                            previewTargetPosition + desc.PreviewPositionOffset,
                                            desc.PreviewDirection));
                            }
                        }
                        
                        
                        if (GUI.Button(new Rect(indented.x + indented.width - 50f, indented.y, 50f, indented.height),
                            "+"))
                        {
                            PlusMenuDropdown(trigger);
                        }

                        for (var i = 0; i < trigger.EffectGroups.Count; i++)
                        {
                            if (trigger.EffectGroups[i] == null) continue;

                            index++;
                            GenerateSimpleInterface(trigger.EffectGroups[i], ref index, indent + 1,
                                propertyPath + ".EffectGroups", i, doHighlight);
                        }
                        
                        // }
                    }

                    break;
                }
                case GameFeelEffectGroup group:
                {
                    if (copiedObject is GameFeelEffect)
                    {
                        canPaste = true;
                    }
                    else
                    {
                        canPaste = false;
                    }

                    
                    var clickArea = ClickAreaWithContextMenu(group);
                    var indented = EditorGUI.IndentedRect(clickArea);

                    // if (GUI.Button(
                    //     new Rect(indented.x - 60f, indented.y, indented.width - indented.width + 25f,
                    //         indented.height),
                    //     EditorGUIUtility.IconContent("RotateTool")))
                    // {
                    //     Undo.PerformUndo();
                    // }
                    
                    var prefix = group.Disabled ? "[DISABLED] " : "";
                    var groupLabel = prefix + "Effect Group " + (string.IsNullOrWhiteSpace(group.GroupName)
                                         ? ""
                                         : "'" + group.GroupName + "'")
                                     + " [Applies to "
                                     + group.AppliesTo.GetName()
                                     + (group.AppliesTo == GameFeelTarget.Tag ? " ("+group.TargetTag+")" : "")  
                                     + "]";

                    if (doHighlight && EditorHelpers.HighlightedEffectGroupIndex == dataIndex)
                    {
                        //DO COLORING, if highlight is enabled, for highlightTime seconds!
                        EditorHelpers.DrawColoredRect(clickArea, highlightColor.withA(alpha));

                        // ExpandedDescriptionNames[target.GetInstanceID()][index] = true;
                    }

                    group.Disabled =
                        !EditorGUI.ToggleLeft(
                            new Rect(clickArea.x - 28f, clickArea.y, clickArea.width - indented.width + 15f,
                                clickArea.height),
                            GUIContent.none, !group.Disabled);
                    
                    using (new EditorGUI.DisabledScope(group.Disabled))
                    {
                        ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(
                            new Rect(clickArea.x, clickArea.y, indented.width - 210f, clickArea.height),
                            ExpandedDescriptionNames[target.GetInstanceID()][index], groupLabel, toggleOnLabel);
                    }
                    
                    if (!group.Disabled)
                    {
                        // else
                        // {   

                        if (group.EffectsToExecute.Count == 0)
                        {
                            //GUILayout.Label("Select what type of reaction you'd like here:", EditorStyles.largeLabel);
                            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox)) 
                            {
                                using (new EditorGUILayout.VerticalScope())
                                {
                                    group.selectedCategory =
                                        (StepThroughModeWindow.EffectGeneratorCategories) EditorGUILayout.EnumPopup(
                                            "Category", group.selectedCategory);
                                    group.selectedIntensity =
                                        EditorGUILayout.IntSlider("Intensity", group.selectedIntensity, 1, 10);
                                }

                                using (new EditorGUILayout.VerticalScope())
                                {
                                    if (GUILayout.Button("Generate!"))
                                    {
                                        Undo.RecordObject(target, "Generate " + group.selectedCategory.GetName());
                                        var recipe =
                                            StepThroughModeWindow.GenerateRecipe(group.selectedCategory,
                                                group.selectedIntensity);

                                        group.EffectsToExecute.AddRange(recipe);

                                        //Take the handcrafted tree, and mutate it!
                                        InteractiveEvolution.MutateGroup(group, 0.25f, 0.25f, 0.10f);

                                        serializedObject.ApplyModifiedProperties();
                                    }

                                    if (GUILayout.Button("Handcrafted!"))
                                    {
                                        Undo.RecordObject(target, "Handcrafted " + group.selectedCategory.GetName());
                                        var recipe =
                                            StepThroughModeWindow.GenerateRecipe(group.selectedCategory,
                                                group.selectedIntensity);
                                    
                                        group.EffectsToExecute.AddRange(recipe);
                                        serializedObject.ApplyModifiedProperties();
                                    }
                                }
                            }
                        }
                        else
                        {
                            showGenerators =
                                EditorGUI.Foldout(new Rect(indented.x + indented.width - 220f - EditorGUIUtility.standardVerticalSpacing, 
                                                                    indented.y, 110f, indented.height), 
                                    showGenerators, "Generator", true);
                            if (showGenerators)
                            {
                                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                                {
                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        group.selectedCategory =
                                            (StepThroughModeWindow.EffectGeneratorCategories) EditorGUILayout.EnumPopup(
                                                "Category", group.selectedCategory);
                                        group.selectedIntensity =
                                            EditorGUILayout.IntSlider("Intensity", group.selectedIntensity, 1, 10);
                                    }

                                    using (new EditorGUILayout.VerticalScope())
                                    {
                                        if (GUILayout.Button("Mutate"))
                                        {
                                            Undo.RecordObject(target,
                                                "Mutate " + (string.IsNullOrEmpty(group.GroupName)
                                                    ? "EffectGroup"
                                                    : group.GroupName));

                                            InteractiveEvolution.MutateGroup(group);
                                        }

                                        if (GUILayout.Button("Regenerate"))
                                        {
                                            Undo.RecordObject(target, "Regenerate " + group.selectedCategory.GetName());
                                            //group.EffectsToExecute.Clear();
                                            group.EffectsToExecute.RemoveAll(item => item.Lock == false);
                                            var recipe = StepThroughModeWindow.GenerateRecipe(group.selectedCategory,
                                                group.selectedIntensity, group.EffectsToExecute);

                                            group.EffectsToExecute.AddRange(recipe);

                                            //Take the handcrafted tree, and mutate it!
                                            InteractiveEvolution.MutateGroup(group, 0.25f, 0.25f, 0.10f);

                                            serializedObject.ApplyModifiedProperties();
                                        }
                                    }
                                }
                            }
                            

                            // using (new EditorGUILayout.HorizontalScope())
                            // {
                            //     if (GUILayout.Button("Regenerate using handcrafted tree"))
                            //     {
                            //         Undo.RecordObject(target, "Re-Handcraft "+selectedCategory.GetName());
                            //         group.EffectsToExecute.RemoveAll(item => item.Lock == false);
                            //         var recipe = StepThroughModeWindow.GenerateRecipe(selectedCategory, selectedIntensity, group.EffectsToExecute);
                            //         
                            //         group.EffectsToExecute.AddRange(recipe);
                            //         
                            //         serializedObject.ApplyModifiedProperties();
                            //     }
                            // }
                        }
                        
                        if (ExpandedDescriptionNames[target.GetInstanceID()][index])
                        {
                            // ExpandedDescriptionNames[target.GetInstanceID()][index][1] =
                            //     EditorGUILayout.Foldout(ExpandedDescriptionNames[target.GetInstanceID()][index][1], "Group Properties");
                            //
                            // if (ExpandedDescriptionNames[target.GetInstanceID()][index][1])
                            // {
                            DrawPropertyWithColor(propertyPath, highlightColor);
                            // }
                        }

                        if (GUI.Button(
                            new Rect(indented.x + indented.width - 50f, indented.y, 50f, indented.height),
                            "+"))
                        {
                            PlusMenuDropdown(group);
                        }

                        
                        if (GUI.Button(new Rect(indented.x + indented.width - 100f - EditorGUIUtility.standardVerticalSpacing, indented.y, 50f, indented.height),"Mutate"))
                        {
                            Undo.RecordObject(target, "Mutate " + (string.IsNullOrEmpty(group.GroupName) ? "EffectGroup" : group.GroupName));

                            InteractiveEvolution.MutateGroup(group);
                        }

                        for (var i = 0; i < group.EffectsToExecute.Count; i++)
                        {
                            if (group.EffectsToExecute[i] == null) continue;

                            index++;
                            GenerateSimpleInterface(group.EffectsToExecute[i], ref index, indent + 1,
                                propertyPath + ".EffectsToExecute", i);
                        }
                    }


                    break;
                }
                case GameFeelEffect effect:
                {
                    if (copiedObject is GameFeelEffect)
                    {
                        canPaste = true;
                    }
                    else
                    {
                        canPaste = false;
                    }
                    
                    var clickArea = ClickAreaWithContextMenu(effect);
                    
                    var indented = EditorGUI.IndentedRect(clickArea);
                    
                    var prefix = effect.Disabled ? "[DISABLED] " : "";
                    
                    var remainingTime = effect.GetRemainingTime(true);
                    var suffix = " [" + remainingTime.ToString("F") + (remainingTime == float.PositiveInfinity ? "" : "s") +"]";

                    var minTime = remainingTime;

                    if (effect.RandomizeDelay)
                    {
                        minTime -= effect.Delay;
                    }

                    if (effect is DurationalGameFeelEffect durational && durational.RandomizeDuration)
                    {
                        minTime -= durational.Duration - durational.DurationMin;
                    }
                    
                    //if (effect.RandomizeDelay || effect is DurationalGameFeelEffect durational && durational.RandomizeDuration)
                    if (Math.Abs(minTime - remainingTime) > float.Epsilon * 10f)
                    {
                        suffix = " ["+(minTime.ToString("F"))+"-" + remainingTime.ToString("F") + (remainingTime == float.PositiveInfinity ? "" : "s") + "]";
                    }
                    
                    
                    totalExecutionTime += remainingTime;
                    if (totalExecutionTime != remainingTime)
                    {
                        suffix += " (Total: "+totalExecutionTime.ToString("F") + (totalExecutionTime == float.PositiveInfinity ? "" : "s")+")";
                    }

                    var effectVars = "";
                    
                    if(effect is ParticlePuffEffect puff)
                    {
                        effectVars += " {particles: " + puff.AmountOfParticles + "}";
                    }
                    else if(effect is ParticleScatterEffect scatter)
                    {
                        effectVars += " {particles: " + scatter.AmountOfParticles + "}";
                    }
                    
                    var effectLabel = prefix + ObjectNames.NicifyVariableName(effect.GetType().Name) + effectVars + suffix;

                    EditorGUILayout.BeginHorizontal();

                    effect.Disabled =
                        !EditorGUI.ToggleLeft(
                            new Rect(clickArea.x - 28f, clickArea.y, clickArea.width - indented.width + 15f, clickArea.height),
                            GUIContent.none, !effect.Disabled);
                    
                    EditorGUI.BeginChangeCheck();
                    effect.Lock =
                        EditorGUI.Toggle(
                            new Rect(clickArea.x - 46f, clickArea.y, clickArea.width - indented.width + 15f, clickArea.height),
                            GUIContent.none, effect.Lock, GUI.skin.FindStyle("IN LockButton"));
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        LockChildrenRecursive(effect);

                        void LockChildrenRecursive(GameFeelEffect outerEffect)
                        {
                            //Apply the lock/unlock to children.
                            foreach (var innerEffect in outerEffect.ExecuteAfterCompletion)
                            {
                                innerEffect.Lock = outerEffect.Lock;
                                LockChildrenRecursive(innerEffect);
                            }

                            if (outerEffect is SpawningGameFeelEffect spawnerEffect)
                            {
                                //Apply the lock/unlock to children.
                                foreach (var innerEffect in spawnerEffect.ExecuteOnOffspring)
                                {
                                    innerEffect.Lock = outerEffect.Lock;
                                    LockChildrenRecursive(innerEffect);
                                }
                            }
                        }
                    }

                    if (!effect.Disabled)
                    {
                        if (GUI.Button(new Rect(indented.x + indented.width - 50f, indented.y, 50f, indented.height),"+"))
                        {
                            PlusMenuDropdown(effect);
                        }
                            
                        if (GUI.Button(new Rect(indented.x + indented.width - 100f - EditorGUIUtility.standardVerticalSpacing, indented.y, 50f, indented.height),"Mutate"))
                        {
                            Undo.RecordObject(target, "Mutate " + (string.IsNullOrEmpty(ObjectNames.NicifyVariableName(effect.GetType().Name))));

                            InteractiveEvolution.MutateEffectsRecursive(GameFeelBehaviorBase<GameFeelTrigger>.GetGameFeelEffects(), effect);
                        }
                    }
                    
                    using (new EditorGUI.DisabledScope(effect.Disabled))
                    {
                        // if (effect is TrailEffect trail)
                        // {
                        //     showAttach =
                        //         EditorGUI.Foldout(new Rect(clickArea.x + 100f, clickArea.y, clickArea.width - 100f, clickArea.height),
                        //             showAttach, "Custom Fade Effect");
                        // }
                        ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(
                            new Rect(clickArea.x, clickArea.y, indented.width - 100f - EditorGUIUtility.standardVerticalSpacing, clickArea.height),
                            ExpandedDescriptionNames[target.GetInstanceID()][index], effectLabel, toggleOnLabel);
                        
                        //EditorGUI.indentLevel = 1;
                        
                        
                        if (effect is ColorChangeEffect colorChangeEffect)
                        {
                            var rect = new Rect(EditorGUI.IndentedRect(clickArea).x, clickArea.y + clickArea.height - EditorGUIUtility.standardVerticalSpacing,
                                indented.width - 105f - EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.standardVerticalSpacing * 3);
                            // rect.height = EditorGUIUtility.standardVerticalSpacing;
                            // rect.y = clickArea.y + clickArea.height - EditorGUIUtility.standardVerticalSpacing;

                            var gradient = new Gradient();
                            gradient.mode = GradientMode.Blend;
                            if (colorChangeEffect.setFromValue)
                            {
                                gradient.colorKeys = new[]{new GradientColorKey(colorChangeEffect.@from,0), new GradientColorKey(colorChangeEffect.to, 1)};
                                //gradient.alphaKeys = new[]{new GradientAlphaKey(colorChangeEffect.@from.a,0), new GradientAlphaKey(colorChangeEffect.to.a, 1)};
                            }
                            else
                            {
                                gradient.colorKeys = new[]{new GradientColorKey(colorChangeEffect.to, 0), new GradientColorKey(colorChangeEffect.to, 1)};
                                //gradient.alphaKeys = new[]{new GradientAlphaKey(1,0), new GradientAlphaKey(colorChangeEffect.to.a, 1)};
                            }
                            
                            EditorGUI.GradientField(rect, gradient);
                            //EditorHelpers.DrawColoredRect(rect, colorChangeEffect.to);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                    
                    if(!effect.Disabled)
                    {
                        if (ExpandedDescriptionNames[target.GetInstanceID()][index])
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
                                    propertyPath + ".ExecuteAfterCompletion", i,false, totalExecutionTime);
                            }
                           
                            if (effect is SpawningGameFeelEffect spawner)
                            {
                                if (spawner.ExecuteOnOffspring.Count > 0)
                                {
                                    EditorGUI.indentLevel = indent - 1;
                                    
                                    var subClickArea = ClickAreaWithContextMenu(spawner, false);
                                
                                    EditorGUI.LabelField(subClickArea, "Executed on "+ObjectNames.NicifyVariableName(spawner.GetType().Name)+" Offspring:", EditorStyles.miniBoldLabel);
                                
                                    for (var i = 0; i < spawner.ExecuteOnOffspring.Count; i++)
                                    {
                                        if (spawner.ExecuteOnOffspring[i] == null) continue;
                                        index++;
                                        GenerateSimpleInterface(spawner.ExecuteOnOffspring[i], ref index, indent + 1,
                                            propertyPath + ".ExecuteOnOffspring", i, false, totalExecutionTime);
                                    }   
                                    
                                    EditorGUI.indentLevel = indent;
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
                
                if (clickArea.Contains(current.mousePosition))
                {
                    if (current.type == EventType.Repaint)
                    {
                        if (DragAndDrop.visualMode != DragAndDropVisualMode.None &&
                            DragAndDrop.visualMode != DragAndDropVisualMode.Rejected)
                        {
                            EditorGUI.DrawRect(clickArea, Color.grey);
                        }
                    }
                    else if (current.type == EventType.MouseDrag)
                    {
                        // Clear out drag data
                        DragAndDrop.PrepareStartDrag();
                        
                        // Set up what we want to drag
                        DragAndDrop.SetGenericData("context", context);
                        DragAndDrop.SetGenericData("parentProperty", parentProperty);
                        DragAndDrop.SetGenericData("dataIndex", dataIndex);

                        // Start the actual drag
                        DragAndDrop.StartDrag(context.GetType().Name);
                        
                        //NOTE: this bit of code makes it extremely hard to click the foldouts! So don't do that!
                        // Make sure no one uses the event after us
                        //current.Use();
                    }
                    else if (current.type == EventType.DragUpdated)
                    {
                        switch (context)
                        {
                            case GameFeelTrigger trigger:
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffectGroup ||
                                    DragAndDrop.GetGenericData("context") is GameFeelTrigger)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                }
                                else
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                                }
                                break;
                            
                            case GameFeelEffectGroup group:
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffect ||
                                    DragAndDrop.GetGenericData("context") is GameFeelEffectGroup)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                }
                                else
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                                }
                                break;
                            
                            case GameFeelEffect effect:
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffect)
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Move;
                                }
                                else
                                {
                                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                                }
                                break;
                        }
                        
                        if (DragAndDrop.GetGenericData("context") == context)
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        }
                        
                        current.Use();
                        
                    }
                    else if (current.type == EventType.DragPerform) //IE. DROP !!
                    {
                        DragAndDrop.AcceptDrag();
                        
                        if (DragAndDrop.GetGenericData("context") == context)
                        {
                            return clickArea;
                        }
                        
                        //FOR EFFECTS, WE WANT TO BE ABLE TO EITHER MOVE OR NEST!
                        //Do a thing, in this case a drop down menu
                        var menu = new GenericMenu();
                        if (!string.IsNullOrEmpty(parentProperty) && parentProperty.Equals(DragAndDrop.GetGenericData("parentProperty")))
                        {
                            menu.AddItem(new GUIContent("Reorder"), false, () =>
                            {
                                var sp = serializedObject.FindProperty(parentProperty);
                                var dragDataIndex = (int) DragAndDrop.GetGenericData("dataIndex");

                                //TODO: fix bug where sometimes nothing happens when you try to do this.
                                if (dragDataIndex > dataIndex) // came from 3, going to 0
                                {
                                    for (int i = dragDataIndex-1; i > dataIndex-1; i--)
                                    {
                                        sp.MoveArrayElement(i,i+1);
                                    }  
                                }
                                else // came from 0, going to 3
                                {
                                    for (int i = dragDataIndex; i < dataIndex; i++)
                                    {
                                        sp.MoveArrayElement(i,i+1);
                                    }
                                }
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        
                        switch (context)
                        {
                            case GameFeelTrigger trigger:
                            {
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffectGroup group)
                                {
                                    //NOTE: Deletion does quite work, so instead we just copy, and let the user do the deletion.
                                    // if (!propertyPath.Equals(DragAndDrop.GetGenericData("parentProperty")))
                                    // {
                                    //     menu.AddItem(new GUIContent("Move"), false, () =>
                                    //     {
                                    //         //serializedObject.Update();
                                    //         //Add it to the new location
                                    //         trigger.EffectGroups.Add((GameFeelEffectGroup)JsonUtility.FromJson(JsonUtility.ToJson(group), group.GetType()));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //         
                                    //         //Remove it from it's old location.
                                    //         var sp = serializedObject.FindProperty(
                                    //             (string) DragAndDrop.GetGenericData("parentProperty"));
                                    //         sp.DeleteArrayElementAtIndex((int) DragAndDrop.GetGenericData("dataIndex"));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //     });
                                    // }
                                    
                                    menu.AddItem(new GUIContent("Copy"), false, () =>
                                    {
                                        //Copy it to the new location
                                        trigger.EffectGroups.Add((GameFeelEffectGroup)JsonUtility.FromJson(JsonUtility.ToJson(group), group.GetType()));
                                    });
                                }
                                break;
                            }
                            case GameFeelEffectGroup group:
                            {
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffect effect)
                                {
                                    //NOTE: Deletion does quite work, so instead we just copy, and let the user do the deletion.
                                    // if (!propertyPath.Equals(DragAndDrop.GetGenericData("parentProperty")))
                                    // {
                                    //     menu.AddItem(new GUIContent("Move"), false, () =>
                                    //     {
                                    //         //serializedObject.Update();
                                    //         //Add it to the new location
                                    //         group.EffectsToExecute.Add(
                                    //             (GameFeelEffect) JsonUtility.FromJson(JsonUtility.ToJson(effect),
                                    //                 effect.GetType()));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //         //Remove it from it's old location.
                                    //         var sp = serializedObject.FindProperty(
                                    //             (string) DragAndDrop.GetGenericData("parentProperty"));
                                    //         sp.DeleteArrayElementAtIndex((int) DragAndDrop.GetGenericData("dataIndex"));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //     });
                                    // }

                                    menu.AddItem(new GUIContent("Copy"), false, () =>
                                    {
                                        //Copy it to the new location
                                        group.EffectsToExecute.Add(
                                            (GameFeelEffect) JsonUtility.FromJson(JsonUtility.ToJson(effect),
                                                effect.GetType()));
                                    });
                                }
                                break;
                            }
                            case GameFeelEffect effect:
                                if (DragAndDrop.GetGenericData("context") is GameFeelEffect other)
                                {
                                    //NOTE: Deletion does quite work, so instead we just copy, and let the user do the deletion.
                                    // if (!propertyPath.Equals(DragAndDrop.GetGenericData("parentProperty")))
                                    // {
                                    //     menu.AddItem(new GUIContent("Move to OnComplete"), false, () =>
                                    //     {
                                    //         //serializedObject.Update();
                                    //         //Add it to the new location
                                    //         effect.ExecuteAfterCompletion.Add(
                                    //             (GameFeelEffect) JsonUtility.FromJson(JsonUtility.ToJson(other),
                                    //                 other.GetType()));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //         //Remove it from it's old location.
                                    //         var sp = serializedObject.FindProperty(
                                    //             (string) DragAndDrop.GetGenericData("parentProperty"));
                                    //         sp.DeleteArrayElementAtIndex((int) DragAndDrop.GetGenericData("dataIndex"));
                                    //         serializedObject.ApplyModifiedProperties();
                                    //     });
                                    // }

                                    menu.AddItem(new GUIContent("Copy to OnComplete"), false, () =>
                                    {
                                        //Copy it to the new location
                                        effect.ExecuteAfterCompletion.Add(
                                            (GameFeelEffect) JsonUtility.FromJson(JsonUtility.ToJson(other),
                                                other.GetType()));
                                    });
                                    
                                    if (effect is SpawningGameFeelEffect spawner)
                                    {
                                        menu.AddItem(new GUIContent("Copy to OnOffspring"), false, () =>
                                        {
                                            //Copy it to the new location
                                            spawner.ExecuteOnOffspring.Add(
                                                (GameFeelEffect) JsonUtility.FromJson(JsonUtility.ToJson(other),
                                                    other.GetType()));
                                        });
                                    }
                                }
                                break;
                        }
                        
                        menu.AddItem(new GUIContent("Cancel"), false, DragAndDrop.PrepareStartDrag);
                        
                        menu.ShowAsContext();
                        
                        current.Use();
                    }
                    
                    if (current.type == EventType.ContextClick)
                    {
                        //Do a thing, in this case a drop down menu
                        var menu = new GenericMenu();

                        //TODO: cache this menu, so we don't need to rebuild it all the time. 12/05/2020
                        //Add items from context.
                        switch (context)
                        {
                            case GameFeelDescription desc:
                            {
                                for (var i = 0; i < Enum.GetNames(typeof(GameFeelTriggerType)).Length; i++)
                                {
                                    var type = (GameFeelTriggerType) i;
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

                                if (canPaste)
                                {
                                    var data = new addCallbackStruct
                                    {
                                        isPaste = true,
                                        context = desc,
                                        instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                            copiedObject.GetType()),
                                    };
                                    menu.AddItem(new GUIContent("Paste Trigger"), false,
                                        AddPropertyCallback, data);
                                }
                            }
                                break;
                            case GameFeelTrigger trigger:
                            {
                                var data = new addCallbackStruct
                                {
                                    context = trigger,
                                    instance = () => new GameFeelEffectGroup()
                                };
                                menu.AddItem(new GUIContent("Add EffectGroup"), false,
                                    AddPropertyCallback, data);

                                if (canPaste)
                                {
                                    var pasteData = new addCallbackStruct
                                    {
                                        isPaste = true,
                                        context = trigger,
                                        instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                            copiedObject.GetType()),
                                    };
                                    menu.AddItem(new GUIContent("Paste EffectGroup"), false,
                                        AddPropertyCallback, pasteData);
                                }

                                menu.AddItem(new GUIContent("Copy Trigger"), false, CopyPropertyCallback, trigger);
                                if (removeItem)
                                {
                                    menu.AddItem(new GUIContent("Remove Trigger"), false, RemovePropertyCallback);
                                }
                            }
                                break;

                            case GameFeelEffectGroup group:
                            {
                                var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
                                foreach (var type in types)
                                {
                                    // Skip abstract classes because they should not be instantiated
                                    if (type.IsAbstract)
                                        continue;

                                    var data = new addCallbackStruct
                                    {
                                        context = group.EffectsToExecute,
                                        instance = () => Activator.CreateInstance(type)
                                    };
                                    menu.AddItem(new GUIContent("Add Effect/" + type.Name), false,
                                        AddPropertyCallback, data);
                                }

                                if (canPaste)
                                {
                                    var pasteData = new addCallbackStruct
                                    {
                                        isPaste = true,
                                        context = group.EffectsToExecute,
                                        instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                            copiedObject.GetType()),
                                    };
                                    menu.AddItem(new GUIContent("Paste Effect"), false,
                                        AddPropertyCallback, pasteData);
                                }

                                menu.AddItem(new GUIContent("Copy EffectGroup"), false, CopyPropertyCallback, group);
                                if (removeItem)
                                {
                                    menu.AddItem(new GUIContent("Remove EffectGroup"), false, RemovePropertyCallback);
                                }
                            }
                                break;
                            case GameFeelEffect effect:
                            {
                                var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
                                foreach (var type in types)
                                {
                                    // Skip abstract classes because they should not be instantiated
                                    if (type.IsAbstract)
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

                                    if (effect is SpawningGameFeelEffect spawner)
                                    {
                                        var data = new addCallbackStruct
                                        {
                                            context = spawner.ExecuteOnOffspring,
                                            instance = () => Activator.CreateInstance(type)
                                        };
                                        menu.AddItem(new GUIContent("Add OnOffspring Effect/" + type.Name), false,
                                            AddPropertyCallback, data);
                                    }
                                }

                                if (canPaste)
                                {
                                    //Regular paste block
                                    {
                                        var pasteData = new addCallbackStruct
                                        {
                                            isPaste = true,
                                            context = effect.ExecuteAfterCompletion,
                                            instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                                copiedObject.GetType()),
                                        };
                                        menu.AddItem(new GUIContent("Paste as OnComplete Effect"), false,
                                            AddPropertyCallback, pasteData);
                                    }

                                    if (effect is SpawningGameFeelEffect spawner)
                                    {
                                        var pasteData = new addCallbackStruct
                                        {
                                            isPaste = true,
                                            context = spawner.ExecuteOnOffspring,
                                            instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                                copiedObject.GetType()),
                                        };
                                        menu.AddItem(new GUIContent("Paste as OnOffspring Effect"), false,
                                            AddPropertyCallback, pasteData);
                                    }
                                }

                                menu.AddItem(new GUIContent("Copy Effect"), false, CopyPropertyCallback, effect);

                                if (removeItem)
                                {
                                    menu.AddItem(new GUIContent("Remove Effect"), false, RemovePropertyCallback);
                                }
                            }
                                break;
                        }

                        menu.ShowAsContext();

                        current.Use();
                    }
                }
                
                // if (current.type == EventType.MouseUp)
                // {
                //     DragAndDrop.PrepareStartDrag();
                // }

                return clickArea;
            }

            void PlusMenuDropdown(object context)
            {
                //Do a thing, in this case a drop down menu
                var menu = new GenericMenu();

                //TODO: cache this menu, so we don't need to rebuild it all the time. 12/05/2020
                //Add items from context.
                switch (context)
                {
                    case GameFeelDescription desc:
                    {
                        for (var i = 0; i < Enum.GetNames(typeof(GameFeelTriggerType)).Length; i++)
                        {
                            var type = (GameFeelTriggerType) i;
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
                            menu.AddItem(new GUIContent("Add " + ObjectNames.NicifyVariableName(typeName)), false,
                                AddPropertyCallback, data);
                        }

                        if (canPaste)
                        {
                            var data = new addCallbackStruct
                            {
                                isPaste = true,
                                context = desc,
                                instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                    copiedObject.GetType()),
                            };
                            menu.AddItem(new GUIContent("Paste Trigger"), false,
                                AddPropertyCallback, data);
                        }
                    }
                        break;
                    case GameFeelTrigger trigger:
                    {
                        var data = new addCallbackStruct
                        {
                            context = trigger,
                            instance = () => new GameFeelEffectGroup()
                        };
                        menu.AddItem(new GUIContent("Add EffectGroup"), false,
                            AddPropertyCallback, data);

                        if (canPaste)
                        {
                            var pasteData = new addCallbackStruct
                            {
                                isPaste = true,
                                context = trigger,
                                instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                    copiedObject.GetType()),
                            };
                            menu.AddItem(new GUIContent("Paste EffectGroup"), false,
                                AddPropertyCallback, pasteData);
                        }

                        // menu.AddItem(new GUIContent("Copy Trigger"), false, CopyPropertyCallback, trigger);
                        // if (removeItem)
                        // {
                        //     menu.AddItem(new GUIContent("Remove Trigger"), false, RemovePropertyCallback);
                        // }
                    }
                        break;

                    case GameFeelEffectGroup group:
                    {
                        var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
                        Type baseType = null;
                        foreach (var type in types.OrderBy(t => t.BaseType.IsAbstract ? t.BaseType.FullName : t.BaseType.BaseType.FullName))
                        {
                            if (type.BaseType != baseType && type.BaseType.IsAbstract)
                            {
                                if (baseType != null && !baseType.IsGenericType)
                                {
                                    menu.AddSeparator("Add Effect/");
                                }
                                
                                menu.AddDisabledItem(new GUIContent("Add Effect/"+ObjectNames.NicifyVariableName(type.BaseType.Name)), false);
                                baseType = type.BaseType;
                            }
                            // Skip abstract classes because they should not be instantiated
                            if (type.IsAbstract)
                                continue;

                            var data = new addCallbackStruct
                            {
                                context = group.EffectsToExecute,
                                instance = () => Activator.CreateInstance(type)
                            };
                            menu.AddItem(new GUIContent("Add Effect/" + ObjectNames.NicifyVariableName(type.Name)), false,
                                AddPropertyCallback, data);
                        }

                        if (canPaste)
                        {
                            var pasteData = new addCallbackStruct
                            {
                                isPaste = true,
                                context = group.EffectsToExecute,
                                instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                    copiedObject.GetType()),
                            };
                            menu.AddItem(new GUIContent("Paste Effect"), false,
                                AddPropertyCallback, pasteData);
                        }

                        // menu.AddItem(new GUIContent("Copy EffectGroup"), false, CopyPropertyCallback, group);
                        // if (removeItem)
                        // {
                        //     menu.AddItem(new GUIContent("Remove EffectGroup"), false, RemovePropertyCallback);
                        // }
                    }
                        break;
                    case GameFeelEffect effect:
                    {
                        var spawner = effect as SpawningGameFeelEffect;
                        
                        var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
                        Type baseType = null;
                        foreach (var type in types.OrderBy(t => t.BaseType.IsAbstract ? t.BaseType.FullName : t.BaseType.BaseType.FullName))
                        {
                            if (type.BaseType != baseType && type.BaseType.IsAbstract)
                            {
                                if (baseType != null && !baseType.IsGenericType)
                                {
                                    menu.AddSeparator("Add OnComplete Effect/");
                                    if (spawner != null)
                                    {
                                        menu.AddSeparator("Add OnOffspring Effect/");
                                    }
                                }
                                
                                menu.AddDisabledItem(new GUIContent("Add OnComplete Effect/"+ObjectNames.NicifyVariableName(type.BaseType.Name)), false);
                                if (spawner != null)
                                {
                                    menu.AddDisabledItem(new GUIContent("Add OnOffspring Effect/"+ObjectNames.NicifyVariableName(type.BaseType.Name)), false);
                                }
                                baseType = type.BaseType;
                            }

                            // Skip abstract classes because they should not be instantiated
                            if (type.IsAbstract)
                                continue;
                            
                            //Block to separate variable names
                            {
                                var data = new addCallbackStruct
                                {
                                    context = effect.ExecuteAfterCompletion,
                                    instance = () => Activator.CreateInstance(type)
                                };
                                menu.AddItem(new GUIContent("Add OnComplete Effect/" + ObjectNames.NicifyVariableName(type.Name)), false,
                                    AddPropertyCallback, data);
                            }

                            if (spawner != null)
                            {
                                var data = new addCallbackStruct
                                {
                                    context = spawner.ExecuteOnOffspring,
                                    instance = () => Activator.CreateInstance(type)
                                };
                                menu.AddItem(new GUIContent("Add OnOffspring Effect/" + ObjectNames.NicifyVariableName(type.Name)), false,
                                    AddPropertyCallback, data);
                            }
                        }

                        if (canPaste)
                        {
                            //Regular paste block
                            {
                                var pasteData = new addCallbackStruct
                                {
                                    isPaste = true,
                                    context = effect.ExecuteAfterCompletion,
                                    instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                        copiedObject.GetType()),
                                };
                                menu.AddItem(new GUIContent("Paste as OnComplete Effect"), false,
                                    AddPropertyCallback, pasteData);
                            }

                            if (spawner != null)
                            {
                                var pasteData = new addCallbackStruct
                                {
                                    isPaste = true,
                                    context = spawner.ExecuteOnOffspring,
                                    instance = () => JsonUtility.FromJson(JsonUtility.ToJson(copiedObject),
                                        copiedObject.GetType()),
                                };
                                menu.AddItem(new GUIContent("Paste as OnOffspring Effect"), false,
                                    AddPropertyCallback, pasteData);
                            }
                        }

                        // menu.AddItem(new GUIContent("Copy Effect"), false, CopyPropertyCallback, effect);
                        //
                        // if (removeItem)
                        // {
                        //     menu.AddItem(new GUIContent("Remove Effect"), false, RemovePropertyCallback);
                        // }
                    }
                        break;
                }

                menu.ShowAsContext();
            }

            void AddPropertyCallback(object input)
            {
                var isPaste = ((addCallbackStruct) input).isPaste;
                var context = ((addCallbackStruct) input).context;
                var instance = ((addCallbackStruct) input).instance;
                
                var gameFeelDescription = (GameFeelDescription) target;
                
                switch (context)
                {    
                    case GameFeelDescription desc:
                    {
                        var triggerInstance = (GameFeelTrigger) instance.Invoke();
                        Undo.RecordObject(gameFeelDescription, isPaste ? "Paste" : ("Add " + triggerInstance.TriggerType.GetName()));
                        gameFeelDescription.TriggerList.Add(triggerInstance);
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    case GameFeelTrigger trigger:
                    {
                        Undo.RecordObject(gameFeelDescription, "Add EffectGroup");
                        trigger.EffectGroups.Add((GameFeelEffectGroup) instance.Invoke());
                        serializedObject.ApplyModifiedProperties();
                        break;
                    }
                    //This case handles EffectGroups, ExecuteOnCompletion, and CustomTrailEffects.
                    case List<GameFeelEffect> list:
                    {
                        var effectInstance = (GameFeelEffect) instance.Invoke();
                        Undo.RecordObject(gameFeelDescription, isPaste ? "Paste" : ("Add " + ObjectNames.NicifyVariableName(effectInstance.GetType().Name)));
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

            void CopyPropertyCallback(object instance)
            {
                copiedObject = instance;
            }

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
