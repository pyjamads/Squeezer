using System.Reflection;
using GameFeelDescriptions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

using UnityEngine.SceneManagement;

public class PreviewCameraWindow : EditorWindow
{
    RenderTexture renderTexture;
    public static Camera camera;

    void OnEnable()
    {
        int w = (int)position.width;
        int h = (int)position.height;

        renderTexture = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32);
        if(camera == null) 
        {
            camera = Camera.main;
        }
    }

    
    
    void OnInspectorUpdate()
    {
        this.Repaint();
    }

    void OnGUI()
    {
        // if (GUILayout.Button("Close"))
        // {
        //     //camera.orthographic = false;
        //     this.Close();
        // }
        //if (GUILayout.Button("Close!")) this.Close();
        
        if (renderTexture != null)
        {
            float w = position.width;
            float h = position.height;
            GUI.DrawTexture(new Rect(0, 0, w, h), renderTexture);
        }
    }

    void OnFocus()
    {
        //Selection.activeTransform = camera.transform;
        //camera.orthographic = true;
    }

    void Update()
    {
        if (camera != null)
        {
            camera.targetTexture = renderTexture;
            camera.Render();
            camera.targetTexture = null;
        }

        int w = (int)position.width;
        int h = (int)position.height;
        if (renderTexture.width != w || renderTexture.height != h)
        {
            renderTexture = new RenderTexture(w, h, 32, RenderTextureFormat.ARGB32);
        }
    }

    // void OnLostFocus()
    // {
    //     camera.orthographic = false;
    // }
}


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
    private static PreviewCameraWindow previewCameraWindow;
    
    //UI options!
    private bool captureImages;
    private float captureFPS;
    
    // TODO: Add Reset preview button, toggle visibility, and zoom in/out buttons
    // TODO: Add Screen Capture interface (might be temporary if it doesn't seem useful beyond analysis purposes)
    // TODO: Consider adding optional movement to the object, when the effect is triggered. 
    // TODO: Add select effect group interface
    // TODO: Add generator interface
    
    // TODO: Add interactive evolution interface (same as the interactive evolution scene)
    // TODO: Improve algorithm, by generating way more than 8 individuals for the next generation, and selecting novel ones.
    // TODO: Improve algorithm, by adding a guiding "direction" which can be used to determine fitness of the next generation. 
    
    // TODO: Make the preview camera in the corner the target of camera effects... 2021-03-22
    
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
        PreviewWindow window = GetWindow<PreviewWindow>(typeof(SceneView));
        //Important so we don't break the Window layout, when re-opening the project
        window.name = "PreviewWindow";
        PreviewCameraWindow.camera = window.camera;
        
        if (Camera.main != null)
        {
            window.camera.backgroundColor = Camera.main.backgroundColor;
            window.orthographic = Camera.main.orthographic;
        }
        else
        {
            window.orthographic = false;
        }

        //TODO: This doesn't work, so how do we make sure the camera targeting effects actually target this camera? 
        window.camera.transform.tag = "MainCamera";
        Camera.SetupCurrent(window.camera);

        //TODO: Make Preview camera better, by fixing camera effects, !
        // previewCameraWindow = CreateInstance<PreviewCameraWindow>();
        // var previewPos = new Rect(previewCameraWindow.position);
        //
        // var screenPos = window.ScreenPosition();
        //
        // previewPos.position = new Vector2(screenPos.x - 5, screenPos.y +10) + (window.position.size - window.position.size / 4f);
        // previewPos.size = window.position.size / 4f;
        // previewCameraWindow.minSize = previewPos.size;
        // previewCameraWindow.maxSize = previewPos.size;
        // previewCameraWindow.position = previewPos;
        //
        // //TODO: Deal with moving editor window!!! 2021-03-22
        // previewCameraWindow.ShowPopup();

        // Get the object you're selecting in the Unity Editor
        //window.titleContent = window.GetName();
 
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
        //TODO: This is messing with us! [EditorWindowTitle(title = "Scene", useTypeNameAsIconName = true)]
        // var editorWindowTitleAttribute = TypeDescriptor.GetAttributes(typeof(PreviewWindow))[0];
        //
        // editorWindowTitleAttribute.GetType().GetMember("icon")[0].SetValue(editorWindowTitleAttribute, "preAudioAutoPlayOff");
        // //Console.WriteLine(ca.GetType().GetValue(ca)); // <=== nice
        // TypeDescriptor.AddAttributes(typeof(PreviewWindow), editorWindowTitleAttribute);
        // ca = TypeDescriptor.GetAttributes(typeof(Foo))
        //     .OfType<CategoryAttribute>().FirstOrDefault();
        // Console.WriteLine(ca.Category); // <=== naughty

        // var typ = this.GetType();
        // var att = this.GetType().GetCustomAttributes(this)[0];
        // att.GetType().GetMember("title", BindingFlags.Public | BindingFlags.Instance)[0].SetValue(att, "Preview...");
        
        //NOTE: might not be worth it, seems like the previewSceneStage in 2020, is probably the right choice for this... 
        
        Selection.selectionChanged += OnSelectionChanged;
        
        base.OnEnable();
        
        titleContent = GetName();
        name = "PreviewWindow";
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        previewCameraWindow?.Close();
    }
 
    private new void OnDestroy()
    {
        base.OnDestroy();
        Selection.selectionChanged -= OnSelectionChanged;
        
        GameFeelEffectExecutor.DestroyInstance();
        previewCameraWindow?.Close();
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
                //If preview mode is not preview window, close the window.
                if (desc.PreviewMode != 2)
                {
                    Close();
                    return;
                }

                //If selection changed back to the same description, we don't need to reset the view!
                if (selectedDescription != desc)
                {
                    selectedDescription = desc;

                    //Refresh the preview scene!
                    SetupScene();    
                }
            }
            else
            {
                //Ignore selecting internal objects, but otherwise close the preview!
                if (go.scene != customScene)
                {
                    Close();
                }
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
        lightingObj.transform.position = Vector3.up*100; 
        lightingObj.AddComponent<Light>().type = LightType.Directional;

        //Move lighting obj into scene
        EditorSceneManager.MoveGameObjectToScene(lightingObj, customScene);
        
        // Create the object we're selecting
        var attachedObjects = selectedDescription.FindGameObjectsToAttach();
        if (attachedObjects.Count == 0)
        {
            //TODO: consider searching for the object in prefabs or something... 2021-02-15
            //First round: a primitive cuboid with different scales.
            var gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gameObject.transform.localScale = new Vector3(0.5f, 1f, 0.2f);
            
            target = gameObject;
        }
        else
        {
            target = Instantiate(attachedObjects[0]);
        }
        
        // Move the objects to the preview scene
        EditorSceneManager.MoveGameObjectToScene(target, customScene);
        
        // Zoom the scene view into the new object
        //TODO: FrameSelection better! 2020-06-01
        var biggerBounds = target.transform.lossyScale;
        biggerBounds *= 2f;

        biggerBounds.x += Mathf.Abs(selectedDescription.PreviewPositionOffset.x);
        biggerBounds.y += Mathf.Abs(selectedDescription.PreviewPositionOffset.y);
        biggerBounds.z += Mathf.Abs(selectedDescription.PreviewPositionOffset.z);
        
        Frame(new Bounds(target.transform.position + selectedDescription.PreviewPositionOffset, biggerBounds));
        camera.transform.position = camera.transform.position + Vector3.back * 10; 
        Camera.SetupCurrent(camera);
        
        // Create camera
        // previewCamera = camera;
        // previewCamera.transform.tag = "MainCamera";
        // Camera.SetupCurrent(previewCamera);
        // // var cameraObj = Instantiate(camera.gameObject);
        // // EditorSceneManager.MoveGameObjectToScene(cameraObj, customScene);
        // // cameraObj.transform.position = cameraObj.transform.position + Vector3.back * 10; 
        // //
        // // const float cameraScale = 4f;
        // renderTarget = new RenderTexture(Mathf.FloorToInt(position.width), Mathf.FloorToInt(position.height), 32, RenderTextureFormat.ARGB32);
        // //
        // // // previewCamera = cameraObj.GetComponent<Camera>();
        // previewCamera.targetTexture = renderTarget;

        //Selection.activeObject = tempSelection;

        //Selection.selectionChanged += SetupScene;
    }
    
    private GUIContent GetName()
    {
        // Setup the title GUI Content (Image, Text, Tooltip options) for the window
         GUIContent titleContent = new GUIContent("Effect Preview");
        
         //EditorGUIUtility.Load("Assets/TestFolder/TestIcon.png") as Texture2D;
         //TODO: maybe find/make better icon 2021-02-08
         titleContent.image = EditorGUIUtility.IconContent("preAudioAutoPlayOff").image;
         //titleContent.image = EditorGUIUtility.IconContent("Motion Icon").image;
         //titleContent.image = EditorGUIUtility.IconContent("Particle Effect").image;
         //titleContent.image = EditorGUIUtility.IconContent("d_CustomSorting").image;
        
         return titleContent;
    }
    
    // void OnInspectorUpdate()
    // {
    //     this.Repaint();
    //     
    //     
    //     
    // }
    
    protected override void OnGUI()
    {
        base.OnGUI();

        // previewCamera.Render();
        //
        // var size = new Vector2(renderTarget.width, renderTarget.height);
        // var rect = new Rect(position.max - size, size);
        // GUI.DrawTexture(rect, renderTarget);
    }

    public Rect ScreenPosition()
    {
        //var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
        var parentInfo = GetType().GetField("m_Parent", BindingFlags.NonPublic | BindingFlags.Instance);
        var parent = parentInfo.GetValue(this);
        var screenPosInfo = parent.GetType().GetMember("screenPosition")[0];
        var screenPos = screenPosInfo.GetValue(parent);
        return (Rect)screenPos;
    }

}
