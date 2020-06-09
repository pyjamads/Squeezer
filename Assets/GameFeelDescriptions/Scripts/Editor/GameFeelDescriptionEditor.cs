
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeelDescriptions
{
    //TODO: custom previewer to show effects ! 09/06/2020
    /* 
    [UnityEditor.CustomEditor(typeof(BaseItemDataContainer), true)]
    public class MyScriptEditor : UnityEditor.Editor
    {
        public override bool HasPreviewGUI() { return true; }
        private PreviewRenderUtility _previewRenderUtility;
        Editor gameObjectEditor;
   
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            GameObject obj = (target as BaseItemDataContainer).m_Item != null ? (target as BaseItemDataContainer).m_Item.gameObject : (target as BaseItemDataContainer).m_ItemGameObject;
            if (obj)
            {
                if (gameObjectEditor == null)
                    gameObjectEditor = Editor.CreateEditor(obj);
                gameObjectEditor.OnPreviewGUI(r, background);
            }
     
        }
    }
    
    
    //Alternative!
    GameObject gameObject;
    Editor gameObjectEditor;
    Texture2D previewBackgroundTexture;
    void OnGUI ()
    {
        EditorGUI.BeginChangeCheck();
   
        gameObject = (GameObject) EditorGUILayout.ObjectField(gameObject, typeof(GameObject), true);
   
        if(EditorGUI.EndChangeCheck())
        {
            if(gameObjectEditor != null) DestroyImmediate(gameObjectEditor);
        }
   
        GUIStyle bgColor = new GUIStyle();
   
   
        bgColor.normal.background = previewBackgroundTexture;
   
        if (gameObject != null)
        {
            if (gameObjectEditor == null)
       
            gameObjectEditor = Editor.CreateEditor(gameObject);
            gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect (200,200),bgColor);
        }
    }
    
    also this: https://timaksu.com/post/126337219047/spruce-up-your-custom-unity-inspectors-with-a
    
    */
    
    [CustomEditor(typeof(GameFeelDescription))]
    public class GameFeelDescriptionEditor : Editor
    {
        private GameFeelTriggerType selectedTriggerType;
        private int selectedEffectIndex;
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
        
        
        public static Dictionary<int, List<bool>> ExpandedDescriptionNames = new Dictionary<int, List<bool>>();

        //TODO: Consider making these static, to cache them between instances.
        //[SerializeField] // ?!? does this make sense here?
        private static List<Type> gameFeelEffects;
        private static string[] gameFeelEffectNames;

        private float lastSaveTime;

        private void OnEnable()
        {
            Init();
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
        
        #region Preview stuff

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

        public override void OnInspectorGUI()
        {   
            EditorGUI.BeginChangeCheck();
            
            #region draw inspector
            var gameFeelDescription = (GameFeelDescription) target;

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
                //----------------------------//
                DrawDefaultInspector();
                //----------------------------//    
            }
            #endregion

            GUILayout.Space(20);
            
            showBottomButtons = EditorGUILayout.Foldout(showBottomButtons, "Additional helpers");

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
                                    recipeAsset,
                                    typeof(TextAsset), false);
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
                            refs[attachToIndex] is TrailEffect trail &&
                            GUILayout.Button("Add Custom Fade Effect"))
                        {
                            if (gameFeelEffects == null || gameFeelEffects.Count == 0)
                            {
                                Init();
                            }

                            var effectType = gameFeelEffects[selectedEffectIndex];
                            var instance = (GameFeelEffect) Activator.CreateInstance(effectType);

                            Undo.RecordObject(gameFeelDescription, "Add " + effectType.Name);
                            trail.CustomFadeEffects.Add(instance);

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
                        
                    list.Add(indentString + (index) + ". " + effect.GetType().Name);
                    objs.Add(effect);
                
                    for (int i = 0; i < effect.ExecuteAfterCompletion.Count; i++)
                    {
                        if (effect.ExecuteAfterCompletion[i] == null) continue;
                        
                        var subList = GenerateLabelObjectLists(effect.ExecuteAfterCompletion[i], type, i, indent+1, prefix);
                        list.AddRange(subList.names);
                        objs.AddRange(subList.refs);   
                    }
                
                    if (effect is TrailEffect trail)
                    {
                        for (int i = 0; i < trail.CustomFadeEffects.Count; i++)
                        {
                            if (trail.CustomFadeEffects[i] == null) continue;
                            var subList = GenerateLabelObjectLists(trail.CustomFadeEffects[i], type, i, indent+1,"CustomFade: ");
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
                        var triggerLabel = indentString + (index) + ". " + trigger.GetType().Name;

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
        
        private void GenerateSimpleInterface(object container, ref int index, int indent = 0, string parentProperty = null, int dataIndex = 0)
        {
            var propertyPath = "";
            if (parentProperty != null)
            {
                propertyPath += parentProperty+".Array.data[" + dataIndex + "]";
            }

            //var canPaste = false;
            
            var highlightColor = new Color(1f, 1f, 0f, 1f);
            
            if (ExpandedDescriptionNames[target.GetInstanceID()].Count <= index)
            {
                ExpandedDescriptionNames[target.GetInstanceID()].Add(false);
            }
            
            EditorGUI.indentLevel = indent++;
            
            switch (container)
            {
                case GameFeelDescription desc:
                {
                    showDescriptionSettings = EditorGUILayout.Foldout(showDescriptionSettings, "Description Settings");
                    
                    if (showDescriptionSettings)
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));

                            GUILayout.Label("Choose how to attach the description to objects:", EditorStyles.boldLabel);
                            EditorGUI.indentLevel += 1;
                            showAttach = EditorGUILayout.Foldout(showAttach, "AttachTo");
                            EditorGUI.indentLevel -= 1;

                            if (showAttach)
                            {
                                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                                {
                                    EditorGUI.indentLevel += 1;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DynamicReattachRate"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToTag"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToObjects"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AttachToComponentType"));
                                    EditorGUI.indentLevel -= 1;
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
                        }
                    }
                    
                    GUILayout.Space(20);
                    
                    GUILayout.Label("Effect Tree (Click elements to edit, or right click for options):", EditorStyles.boldLabel);

//                    desc.StepThroughMode = EditorGUILayout.Toggle("Step Through Mode", desc.StepThroughMode);
//                    
//                    if (!string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer) && 
//                        JsonUtility.FromJson<GameFeelTrigger>(EditorGUIUtility.systemCopyBuffer) != null)
//                    {
//                        canPaste = true;
//                    }

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
                        var seperator = !string.IsNullOrEmpty(desc.AttachToTag) && desc.AttachToComponentType != null
                            ? " and "
                            : "";

                        var andList = desc.AttachToObjects?.Length > 0
                            ? (string.IsNullOrEmpty(seperator) ? "list" : " and list")
                            : "";

                        EditorGUI.LabelField(clickArea, "Trigger List [Attaching "
                                                        + desc.TriggerList.Count + " trigger(s) to " +
                                                        desc.AttachToTag +
                                                        seperator +
                                                        desc.AttachToComponentType +
                                                        andList + "]");

                        for (var i = 0; i < desc.TriggerList.Count; i++)
                        {
                            if (desc.TriggerList[i] == null) continue;

                            GenerateSimpleInterface(desc.TriggerList[i], ref index, indent,
                                "TriggerList", i);
                            index++;
                        }
                    }

                    break;
            }
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
                    
                    var highlightTime = 2.5f;
                    var doHighlight = EditorHelpers.HighlightedTriggerIndex == dataIndex && EditorApplication.timeSinceStartup - EditorHelpers.LastHighlightTime < highlightTime;
                    var alpha = (float)(EditorApplication.timeSinceStartup - EditorHelpers.LastHighlightTime) / highlightTime;

//                    if (!string.IsNullOrWhiteSpace(EditorGUIUtility.systemCopyBuffer) && 
//                        JsonUtility.FromJson<GameFeelEffectGroup>(EditorGUIUtility.systemCopyBuffer) != null)
//                    {
//                        canPaste = true;
//                    }
                    
                    var clickArea = ClickAreaWithContextMenu(trigger);
                    if (doHighlight)
                    {
                        //DO COLORING, if highlight is enabled, for highlightTime seconds!
                        EditorHelpers.DrawColoredRect(clickArea, highlightColor.withA(alpha));
                    }
                
                    ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(clickArea, ExpandedDescriptionNames[target.GetInstanceID()][index], triggerLabel);    
                
                    if (ExpandedDescriptionNames[target.GetInstanceID()][index])
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

                    using (new EditorGUI.DisabledScope(group.Disabled))
                    {
                        ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(clickArea, ExpandedDescriptionNames[target.GetInstanceID()][index], groupLabel);    
                    }

                    group.Disabled = !EditorGUI.ToggleLeft(new Rect(clickArea.x - 28f, clickArea.y, clickArea.width, clickArea.height), GUIContent.none, !group.Disabled);
                    
                    if(!group.Disabled)
                    {
                        if (ExpandedDescriptionNames[target.GetInstanceID()][index])
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

                    EditorGUILayout.BeginHorizontal();

                    using (new EditorGUI.DisabledScope(effect.Disabled))
                    {
                        ExpandedDescriptionNames[target.GetInstanceID()][index] = EditorGUI.Foldout(clickArea,
                            ExpandedDescriptionNames[target.GetInstanceID()][index], effectLabel, false);
                    }

                    effect.Disabled = !EditorGUI.ToggleLeft(new Rect(clickArea.x - 28f, clickArea.y, clickArea.width, clickArea.height), GUIContent.none, !effect.Disabled);
                    
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

                    //TODO: cache this menu, so we don't need to rebuild it all the time. 12/05/2020
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
                        Undo.RecordObject(gameFeelDescription, isPaste ? "Paste" : ("Add " + effectInstance.GetType().Name));
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
