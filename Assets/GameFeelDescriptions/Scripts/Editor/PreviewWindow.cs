using System;
using GameFeelDescriptions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Preview window, that opens a new preview scene and instantiates objects in there.
/// Reference: https://forum.unity.com/threads/what-is-previewscenestage.762734/#post-6346353
///
/// Adding selection changed subscription does allow us to change the object being shown in the preview window,
/// but doesn't work super well, once you want to take a look around the scene in the preview window. 
/// </summary>
public class PreviewWindow : SceneView
{
    public GameFeelDescription selectedDescription;
    public Scene sceneLoaded;

    public static GameObject target;
    
    [MenuItem("GameFeelDescriptions/EffectPreview", true)]
    public static bool ValidateShowWindow()
    {
        // Return false if a wrong number of transforms is selected.
        if (Selection.transforms.Length != 1) return false;

        if (Selection.activeGameObject.GetComponent<GameFeelDescription>() == null) return false;

        return true;
    }
    
    [MenuItem("GameFeelDescriptions/EffectPreview")]
    public static void ShowWindow()
    {
        // Create the window
        PreviewWindow window = CreateWindow<PreviewWindow>();
 
        // Get the object you're selecting in the Unity Editor
        window.titleContent = window.GetName();
 
        // Load a new preview scene
        scene = EditorSceneManager.NewPreviewScene();
        
     
        window.sceneLoaded = scene;
        window.sceneLoaded.name = window.name;
        window.customScene = window.sceneLoaded;
 
        window.drawGizmos = false;
        
        //This runs setup scene!
        window.OnSelectionChanged();
 
        window.Repaint();

        GameFeelEffectExecutor.previewScene = window.customScene;
    }
 
    private static Scene scene;
 
    public override void OnEnable()
    {
        base.OnEnable();

        // Set title name
        titleContent = GetName();

        Selection.selectionChanged += OnSelectionChanged;
    }

    // private void OnFocus()
    // {
    //     SetupScene();
    // }

    public override void OnDisable()
    {
        base.OnDisable();
    }
 
    private new void OnDestroy()
    {
        base.OnDestroy();
        Selection.selectionChanged -= OnSelectionChanged;
        
        GameFeelEffectExecutor.DestroyInstance();
        EditorSceneManager.ClosePreviewScene(customScene);
    }

    void OnSelectionChanged()
    {
        if (Selection.objects.Length > 1)
        {
            //Debug.LogError("Your selection must include a single object.");
            return;
        }
        
        if (Selection.objects.Length <= 0)
        {
            //Debug.LogError("No object selected to preview.");
            return;
        }
 
        if (Selection.activeGameObject == null)
        {
            //Debug.LogError("No Game Objects selected, only GameObjects/Prefabs are supported now");
            return;
        }
        
        if (Selection.activeObject is GameObject go)
        {
            var desc = go.GetComponent<GameFeelDescription>();
            if (desc != null)
            {
                selectedDescription = desc;

                //Refresh the preview scene!
                SetupScene();
            }
        }
    }
    
    void SetupScene()
    {
        if (selectedDescription == null) return;
        
        //NOTE: Selection change might not be the greatest idea, unless we can check if the object exists in the preview scene or not!
        //if(customScene.IsValid() &&  Selection.activeObject)
        
        //Selection.selectionChanged -= SetupScene;
        GameFeelEffectExecutor.DestroyInstance();
        EditorSceneManager.ClosePreviewScene(customScene);

        //selectedObj = Selection.activeObject;
        customScene = EditorSceneManager.NewPreviewScene();
        GameFeelEffectExecutor.previewScene = customScene;
            
        // Create lighting
        GameObject lightingObj = new GameObject("Lighting");
        lightingObj.transform.eulerAngles = new Vector3(50, -30, 0);
        
        //Move lighting obj into scene
        EditorSceneManager.MoveGameObjectToScene(lightingObj, customScene);

        // Create the object we're selecting
        var attachedObjects = selectedDescription.FindGameObjectsToAttach();
        if (attachedObjects.Count == 0)
        {
            //TODO: make a primitive or search of the object in prefabs or something... 2021-02-08
            return;
        }
        
        target = Instantiate(attachedObjects[0]);
 
        // Move the objects to the preview scene
        EditorSceneManager.MoveGameObjectToScene(target, customScene);
        
        //Ignore this selection change!
        //var tempSelection = Selection.activeObject;
        //Selection.activeObject = obj;
        
        //titleContent = GetName();

        // Zoom the scene view into the new object
        //TODO: FrameSelection better!
        var biggerBounds = target.transform.lossyScale;
        biggerBounds *= 2f;
        Frame(new Bounds(target.transform.position, biggerBounds));


        //Selection.activeObject = tempSelection;

        //Selection.selectionChanged += SetupScene;
    }
    
    private GUIContent GetName()
    {
        // Setup the title GUI Content (Image, Text, Tooltip options) for the window
         GUIContent titleContent = new GUIContent("Effect Preview");
        
         //TODO: maybe find/make better icon 2021-02-08
         titleContent.image = EditorGUIUtility.IconContent("preAudioAutoPlayOff").image;
         //titleContent.image = EditorGUIUtility.IconContent("Motion Icon").image;
         //titleContent.image = EditorGUIUtility.IconContent("Particle Effect").image;
         //titleContent.image = EditorGUIUtility.IconContent("d_CustomSorting").image;
        
         return titleContent;
    }
 
    new void OnGUI()
    {
        base.OnGUI();
    }
}
