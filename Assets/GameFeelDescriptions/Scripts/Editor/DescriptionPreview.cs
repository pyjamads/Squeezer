using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    
    /// <summary>
    /// This is for showing the small preview window in the inspector.
    /// Set <see cref="HasPreviewGUI"/> to return true, for it to show up.
    /// This code has issues with cleaning itself up properly,
    /// but the commented code can be used to show the first object a description attaches to.
    /// Maybe using this reference will help: https://timaksu.com/post/126337219047/spruce-up-your-custom-unity-inspectors-with-a
    /// </summary>
    [CustomPreview(typeof(GameFeelDescription))]
    public class DescriptionPreview : ObjectPreview
    {
        public override bool HasPreviewGUI()
        {
            return false;
        }
        
        //public ovr
        
         /*
        #region Preview GUI
        
        public override bool HasPreviewGUI() { return false; }
        private PreviewRenderUtility previewRenderUtility;
        //private Scene previewScene;
        
        GameObject previewObject;
        Vector3 centerPosition;

        private bool[] previewFoldoutUnfolded;
        private const bool ShowDescriptionStats = true;
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
        //     base.OnInteractivePreviewGUI(r, background);
        // }

        // PreviewGUI(Rect r, GUIStyle background)
        // {
            var desc = target as GameFeelDescription;
            //
            // if (previewFoldoutUnfolded == null || previewFoldoutUnfolded.Length != desc.TriggerList.Count)
            // {
            //     previewFoldoutUnfolded = new bool[desc.TriggerList.Count];
            // }

            if (previewRenderUtility == null)
            {
                previewRenderUtility = new PreviewRenderUtility();
                
                //previewScene = EditorSceneManager.NewPreviewScene();
                
                // // Create lighting
                // GameObject lightingObj = new GameObject("Lighting");
                // lightingObj.transform.eulerAngles = new Vector3(50, -30, 0);
                //
                // //Move lighting obj into scene
                // EditorSceneManager.MoveGameObjectToScene(lightingObj, previewScene);

                // // Create the object we're selecting
                // GameObject obj = Instantiate(Selection.activeObject as GameObject);
                //
                // // Move the objects to the preview scene
                // EditorSceneManager.MoveGameObjectToScene(obj, previewScene);
        
                
                
                // // Zoom the scene view into the new object
                // //TODO: FrameSelection better!
                // var biggerBounds = obj.transform.lossyScale;
                // biggerBounds *= 1.2f;
                // Frame(new Bounds(obj.transform.position, biggerBounds));
                
                
                var attachObjects = desc.FindGameObjectsToAttach();
                if (attachObjects.Count > 0)
                {
                    previewObject = Instantiate(attachObjects[0]);
                    previewRenderUtility.AddSingleGO(previewObject);
                    previewObject.transform.position = Vector3.zero;
                }
                
                //var flags = BindingFlags.Static | BindingFlags.NonPublic;
                //var propInfo = typeof(Camera).GetProperty ("PreviewCullingLayer", flags);
                //int previewLayer = (int)propInfo.GetValue (null, new object[0]);

                previewRenderUtility = new PreviewRenderUtility (true);
                previewRenderUtility.cameraFieldOfView = 30f;
                //previewRenderUtility.camera.cullingMask = 1 << previewLayer;

                // var component = (Component)target;
                // previewObject = Instantiate (attachObjects[0]);
                //previewObject.hideFlags = HideFlags.HideAndDontSave;

                //previewObject.layer = previewLayer;

                // foreach (Transform transform in previewObject.transform) {
                //     transform.gameObject.layer = previewLayer;
                // }

                Bounds bounds = new Bounds (previewObject.transform.position, Vector3.zero);

                foreach (var renderer in previewObject.GetComponentsInChildren<Renderer>()) {
                    bounds.Encapsulate (renderer.bounds);
                }

                centerPosition = bounds.center;

                //previewObject.SetActive (false);

                RotatePreviewObject (new Vector2 (-120, 20));
            }

            if (previewRenderUtility != null)
            {
                previewRenderUtility.BeginPreview (r, background);

                var drag = Vector2.zero;

                if (Event.current.type == EventType.MouseDrag) {
                    drag = Event.current.delta;
                }

                previewRenderUtility.camera.transform.position = centerPosition + Vector3.forward * -5;

                RotatePreviewObject (drag);

                //previewObject.SetActive (true);
                previewRenderUtility.Render();
                //previewRenderUtility.camera.Render ();
                //previewObject.SetActive (false);

                previewRenderUtility.EndAndDrawPreview (r);

                if (drag != Vector2.zero)
                    Repaint ();
                
                //previewRenderUtility.Render();
            }
            
            // if (ShowDescriptionStats)
            // {
            //     for (var index = 0; index < desc.TriggerList.Count; index++)
            //     {
            //         var trigger = desc.TriggerList[index];
            //         previewFoldoutUnfolded[index] = EditorGUILayout.Foldout(previewFoldoutUnfolded[index], index+". "+trigger.GetType().Name);
            //         
            //         //TODO: Do change check, and fold every other trigger, so it works like a radio button! 2021-01-07
            //         
            //         if (previewFoldoutUnfolded[index])
            //         {
            //             GUILayout.Label("Duration (Max): "+GetSpawnCount(trigger));
            //             GUILayout.Label("Spawn Count: "+GetSpawnCount(trigger));
            //
            //             var gradients = GetColorGradients(trigger);
            //
            //             foreach (var gradient in gradients)
            //             {
            //                 EditorGUILayout.GradientField(gradient);
            //             }
            //         }
            //     }
            // }
            // else
            // {
            //     //TODO: Preview actual effects being executed... 2021-01-07
            //     //TODO Note: Descriptions can have multiple triggers,
            //     //TODO might have to show a dropdown to select which trigger to execute
            //     //TODO Note: Use the Stats fold/unfoldedness to select which trigger to preview!
            //     
            //     //use the previewRenderUtility to build a scene for executing the effects. (This requires building a whole hierarchy and controlling it manually)
            // }
        }
        
        public override GUIContent GetPreviewTitle ()
        {
            return new GUIContent (target.name + " Preview");
        }

        private void RotatePreviewObject (Vector2 drag)
        {
            previewObject.transform.RotateAround (centerPosition, Vector3.up, -drag.x);
            previewObject.transform.RotateAround (centerPosition, Vector3.right, -drag.y);
        }

        private void OnDisable()
        {
            if (previewRenderUtility == null)
                return;
            previewRenderUtility.Cleanup();
        }

        private void OnDestroy()
        {
            if (previewRenderUtility == null)
                return;
            previewRenderUtility.Cleanup();
            previewRenderUtility = (PreviewRenderUtility) null;
            
            //EditorSceneManager.CloseScene(previewScene, true);
        }

        private int GetSpawnCount(GameFeelTrigger trigger)
        {
            //TODO: Recurse, and count spawned objects! 2021-01-07
            return 5;
        }
        
        private float GetMaxDuration(GameFeelTrigger trigger)
        {
            //TODO: Recurse, and find max! 2021-01-07
            //return trigger.EffectGroups[0].EffectsToExecute[0].GetRemainingTime(true);
            return 1.5f;
        }
        
        private List<Gradient> GetColorGradients(GameFeelTrigger trigger)
        {
            //TODO: Recurse, and find color tweens etc.! 2021-01-07
            //return trigger.EffectGroups[0].EffectsToExecute[0].GetRemainingTime(true);
            return new List<Gradient>{new Gradient()};
        }

        
        #endregion
        */

         private GameObject previewObject;
         private PreviewRenderUtility previewRenderUtility;
         
        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            var desc = (GameFeelDescription) target;
            
            if (previewRenderUtility == null)
            {
                previewRenderUtility = new PreviewRenderUtility(true);
                
                var attachObjects = desc.FindGameObjectsToAttach();
                if (attachObjects.Count > 0)
                {
                    previewObject = GameObject.Instantiate(attachObjects[0]);
                    previewRenderUtility.AddSingleGO(previewObject);
                    previewObject.transform.position = Vector3.zero;
                }
                
                previewRenderUtility.lights[0] = new Light();
                previewRenderUtility.lights[0].type = LightType.Directional;
                previewRenderUtility.lights[0].transform.eulerAngles = new Vector3(50, -30, 0);
            }
            else
            {
                previewRenderUtility.BeginPreview(r, background);

                previewRenderUtility.camera.transform.position = previewObject.transform.position - Vector3.back * 10;
            
                previewRenderUtility.Render();
            
                GUI.DrawTexture(r,previewRenderUtility.EndPreview());    
            }
            
            
            //GUI.Label(r, target.name + " is being previewed");
            
            
        }
    }
    
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
}