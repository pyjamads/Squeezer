using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using UnityEditor;
#endif

namespace GameFeelDescriptions
{
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    public class GameFeelEffectExecutor : MonoBehaviour
    {
        //Debug info for the Inspector
        [UsedImplicitly]
        public int CurrentlyExecuting;
        
        //TODO: Check how slow this is, and maybe make a type registry+array or a single list with filtering
        //NOTE: we don't need serialized reference here, but it can be good for debugging 19/03/2020
        public List<GameFeelEffect> activeEffects;
        private List<GameFeelEffect> tempEffects;


        //NOTE: this might be redundant, now that we don't have infinitely looping effects...
        [MenuItem("GameFeelDescriptions/ClearLingeringEffects")]
        public static void ClearLingeringEffects()
        {
            DestroyInstance();
        }
        
        [MenuItem("GameFeelDescriptions/ClearLingeringEffects", true)]
        public static bool AllowClearLingeringEffects()
        {
            if (instance == null)
            {
                // check if there is an instance available already
                instance = FindObjectOfType(typeof(GameFeelEffectExecutor)) as GameFeelEffectExecutor;    
            }
            
            return instance != null;
        }
        
        /// <summary>
        /// disallow spawning new instances when application is shutting down.
        /// </summary>
        public static bool applicationIsQuitting;

        /// <summary>
        /// Determines whether to clear the current active tweens when a new level is loaded.
        /// </summary>
        public bool shouldRemoveTweensOnLevelLoad;
        
        private static GameFeelEffectExecutor instance;
        /// <summary>
        /// The static Instance of the GameFeelTween runner. 
        /// </summary>
        public static GameFeelEffectExecutor Instance
        {
            get
            {
                if (instance || applicationIsQuitting) return instance;
                
                // check if there is an instance available already
                instance = FindObjectOfType(typeof(GameFeelEffectExecutor)) as GameFeelEffectExecutor;

                if (instance != null)
                {
                    if (instance.activeEffects == null)
                    {
                        instance.activeEffects = new List<GameFeelEffect>();
                        instance.tempEffects = new List<GameFeelEffect>();
                    }
                }
                else
                {
                    var obj = new GameObject("GameFeelEffectExecution");
                    instance = obj.AddComponent<GameFeelEffectExecutor>();
                }
                
#if UNITY_EDITOR
                if(EditorApplication.isPlayingOrWillChangePlaymode)
#endif
                {
                    DontDestroyOnLoad(instance);
                }
#if UNITY_EDITOR
                else
                {
                    //TODO: maybe use this cleaner solution in GameFeelDescription 2021-02-04:
                    //https://forum.unity.com/threads/solved-how-to-force-update-in-edit-mode.561436/#post-5110952 
                    EditorApplication.update += instance.Update;

                    if (previewScene.IsValid())
                    {
                        SceneManager.MoveGameObjectToScene(instance.gameObject, previewScene);
                    }
                }
#endif
                return instance;
            }
        }
        
        public static void DestroyInstance()
        {
            if (instance == null) return;
            
            EditorApplication.update -= instance.Update;
            
            DestroyImmediate(instance.gameObject);

            instance = null;
        }
        
        #region MonoBehaviour

        public void Awake()
        {
            if (instance == null)
            {
                instance = this;
                instance.activeEffects = new List<GameFeelEffect>();
                instance.tempEffects = new List<GameFeelEffect>();
            }

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= OnSceneWasLoaded;
            SceneManager.sceneLoaded += OnSceneWasLoaded;
#endif
        }

        public void OnApplicationQuit()
        {
            instance = null;
            Destroy(gameObject);
            applicationIsQuitting = true;
        }

        public void OnDestroy()
        {
            OnCustomEventTriggered = null;

            EditorApplication.update -= Update;
        }


#if UNITY_5_4_OR_NEWER
        public void OnSceneWasLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (shouldRemoveTweensOnLevelLoad && loadSceneMode == LoadSceneMode.Single)
            {
                activeEffects.Clear();
            }
        }
#else
		public void OnLevelWasLoaded(int level)
		{
			if(shouldRemoveTweensOnLevelLoad)
			{
                activeTweens.Clear();
            }
		}
#endif

// #if UNITY_EDITOR
//         void OnRenderObject()
//         {
//             if (!EditorApplication.isPlayingOrWillChangePlaymode)
//             {
//                 Update();
//             }
//         }
// #endif

        private double lastUpdateTime;

        private float GetUnscaledDeltaTime()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                var deltaTime = EditorApplication.timeSinceStartup - lastUpdateTime;
                
                //Hack to avoid instant execution in EditMode.
                if (deltaTime > 0.1f)
                {
                    deltaTime = 0.016f;
                }
                
                lastUpdateTime = EditorApplication.timeSinceStartup;
                return (float)deltaTime;
            }
#endif
            return Time.unscaledDeltaTime;
        }

        private double lastQueuedRenderUpdate;
        private float idleTime;

#if UNITY_EDITOR
        private void OnApplicationPause(bool pauseStatus)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                DestroyInstance();
            }
        }
        
#endif
        
        private void QueueEditorRenderUpdate()
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (EditorApplication.timeSinceStartup - lastQueuedRenderUpdate > 0.016f) //Update at ~60FPS
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                    lastQueuedRenderUpdate = EditorApplication.timeSinceStartup;
                    idleTime += 0.016f;
                }

                
                //TODO: Stop relying on idle time, kill it, when selection changes! 2021-02-04
                //This is probably the best way to clean up after ourselves...
                if (activeEffects == null || activeEffects.Count == 0)
                {
                    //After idling for a bit, remove the executor.
                    if (idleTime > 5f)
                    {
                        EditorApplication.update -= Update;
                        DestroyImmediate(gameObject);
                    }
                }
                else
                {
                    //Reset idle time, whenever there are active effects.
                    idleTime = 0;
                }
            }
#endif
        }
        
        
        public void Update()
        {
            if (instance != this)
            {
                EditorApplication.update -= Update;
                DestroyImmediate(gameObject);
                return;
            }
            
            var unscaledDeltaTime = GetUnscaledDeltaTime();
            //If a frame took more than a second to update, ignore effects
            //NOTE: this is to avoid that first few nasty frames in the editor...
            //TODO: figure out a better way of handling this... 22/03/2020
            if (unscaledDeltaTime > 0.1f) return;

            tempEffects.Clear();
            tempEffects.AddRange(activeEffects);
    
            CurrentlyExecuting = tempEffects.Count;
            foreach (var effect in tempEffects)
            {
                if (effect.triggerData != null)
                {
                    //Tell the effect that it can use destroyImmediate.
                    effect.triggerData.InCollisionUpdate = false;
                }
                
                // update tween
                if(effect.Tick(unscaledDeltaTime))
                {
                    // tween completed
                    RemoveEffect(effect);
                    //activeEffects.Remove(effect);
                }
            }

            if (idleTime < 5f)
            {
                //While executing effects in the editor, force the editor to render at roughly 60FPS
                QueueEditorRenderUpdate();
            }
        }

        #endregion

        #region Instantiation
        
        public static Scene previewScene;

        public static GameObject Instantiate(PrimitiveType primitive)
        {
            //NOTE: Commented out, because we now simply move the executor into the preview scene.
//             var editMode = false;
//             
// #if UNITY_EDITOR
//             editMode = EditorApplication.isPlayingOrWillChangePlaymode == false;
// #endif
//       
//             //Only try to use preview, when running from the editor, and not in playmode.
//             if (editMode)
//             {
//                 var gameObject = GameObject.CreatePrimitive(primitive);
//                 gameObject.transform.parent = Instance.transform;
//                 
//                 
//                 //NOTE: Move the intantiated object to the preview scene, when previewing! 
//                 // if (previewScene.IsValid())
//                 // {
//                 //     SceneManager.MoveGameObjectToScene(gameObject, previewScene);
//                 // }
//
//                 return gameObject;
//             }
//             else
//             {
                var gameObject = GameObject.CreatePrimitive(primitive);
                gameObject.transform.parent = Instance.transform;

                return gameObject;
            // }
        }
        
        public static GameObject Instantiate(GameObject prefab)
        {
            //NOTE: Commented out, because we now simply move the executor into the preview scene.
//             var editMode = false;
//             
// #if UNITY_EDITOR
//             editMode = EditorApplication.isPlayingOrWillChangePlaymode == false;
// #endif
//       
//             //Only try to use preview, when running from the editor, and not in playmode.
//             if (editMode)
//             {
//                 var gameObject = GameObject.Instantiate(prefab, Instance.transform);
//
//                  
//                 //NOTE: Move the intantiated object to the preview scene, when previewing! 
//                 // if (previewScene.IsValid())
//                 // {
//                 //     SceneManager.MoveGameObjectToScene(gameObject, previewScene);
//                 // }
//
//                 return gameObject;
//             }
//             else
//             {
                var gameObject = GameObject.Instantiate(prefab, Instance.transform);

                return gameObject;
            // }
            
        }
        
        public static GameObject Instantiate(PrimitiveType primitive, Vector3 position)
        {
            var gameObject = Instantiate(primitive);
            gameObject.transform.position = position;
            
            return gameObject;
        }

        public static GameObject Instantiate(GameObject prefab, Vector3 position)
        {
            var gameObject = Instantiate(prefab);
            gameObject.transform.position = position;

            return gameObject;
        }
        #endregion
        
        public void QueueEffect(GameFeelEffect effect)
        {
            // if (forceQueue)
            // {
            //     activeEffects.Add(effect);
            //     return;
            // }
            
            //Destruction effects cannot occur during collision updates, so we delay it until next update
            //TODO: handle the whole DestroyImmediate issue as well (when copying objects) 2020-08-27
            //TODO: another issue is Disabling the renderer when making a copy that is following the original...2020-09-04
            
            if (effect.triggerData != null && effect.triggerData.InCollisionUpdate && (effect is DestroyEffect || effect is DisableEffect || effect is DisableRendererEffect || effect is TrailEffect || effect is SpawnCopyEffect))
            {
                activeEffects.Add(effect);
                return;
            }
            
            //If Delay is Zero, execute the Effect immediately to avoid one frame of lag.
            if (effect.Delay == 0)
            {
                //Tick it, but if it's not done, then add it to active effects.
                if (!effect.Tick(GetUnscaledDeltaTime()))
                {
                    activeEffects.Add(effect);
                }

                // //If Duration > 0, queue the "rest" of the effect.
                // if (effect is DurationalGameFeelEffect durational && durational.Duration > 0)
                // {
                //     activeEffects.Add(effect);
                // }
            }
            else
            {
                activeEffects.Add(effect);    
            }
            
        }

        public void RemoveEffect(GameFeelEffect effect)
        {
            activeEffects.Remove(effect);
        }
        
        public void TriggerCustomEvent(GameObject origin, string eventName, GameFeelTriggerData innerDataData = null) => OnCustomEventTriggered.Invoke(origin, eventName, innerDataData);

        //TODO: Maybe register events separately per "eventName", ie. Dictionary<string, Action<GameObject, string, GameFeelTriggerEvent>> ... 7/4/2020
        public event Action<GameObject, string, GameFeelTriggerData> OnCustomEventTriggered = delegate(GameObject o, string s, GameFeelTriggerData args) {  };
    }
}