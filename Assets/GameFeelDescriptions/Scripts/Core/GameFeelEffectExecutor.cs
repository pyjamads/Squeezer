using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeelDescriptions
{   
    //[ExecuteInEditMode]
    public class GameFeelEffectExecutor : MonoBehaviour
    {
        //Debug info for the Inspector
        [UsedImplicitly]
        public int CurrentlyExecuting;
        
        //TODO: Check how slow this is, and maybe make a type registry+array or a single list with filtering
        //NOTE: we don't need serialized reference here, but it can be good for debugging 19/03/2020
        public List<GameFeelEffect> activeEffects = new List<GameFeelEffect>( );
        private List<GameFeelEffect> tempEffects = new List<GameFeelEffect>();
        
        /// <summary>
        /// disallow spawning new instances when application is shutting down.
        /// </summary>
        private static bool applicationIsQuitting;

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

                if (instance) return instance;
                
                var obj = new GameObject("GameFeelEffectExecution");
                instance = obj.AddComponent<GameFeelEffectExecutor>();
                DontDestroyOnLoad(obj);

                return instance;
            }
        }
        
        #region MonoBehaviour

        public void Awake()
        {
            if(instance == null)
                instance = this;

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

        public void Update()
        {
            //If a frame took more than a second to update, ignore effects
            //NOTE: this is to avoid that first few nasty frames in the editor...
            //TODO: figure out a better way of handling this... 22/03/2020
            if (Time.unscaledDeltaTime > 0.1f) return;
            
            tempEffects.Clear();
            tempEffects.AddRange(activeEffects);
    
            CurrentlyExecuting = tempEffects.Count;
            foreach (var effect in tempEffects)
            {
                //Tell the effect that it can use destroyImmediate.
                effect.triggerData.InCollisionUpdate = false;
                
                // update tween
                if(effect.Tick())
                {
                    // tween completed
                    RemoveEffect(effect);
                    //activeEffects.Remove(effect);
                }
            }
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
            //TODO: instead of passing this "forceQueue" flag, pass a isCollisionUpdate flag 2020-08-27
            //TODO: handle the whole DestroyImmediate issue as well (when copying objects) 2020-08-27
            //TODO: another issue is Disabling the renderer when making a copy that is following the original...2020-09-04
            
            if (effect.triggerData.InCollisionUpdate && (effect is DestroyEffect || effect is DisableEffect || effect is DisableRendererEffect || effect is TrailEffect || effect is SpawnCopyEffect))
            {
                activeEffects.Add(effect);
                return;
            }
            
            //If Delay is Zero, execute the Effect immediately to avoid one frame of lag.
            if (effect.Delay == 0)
            {
                //Tick it, but if it's not done, then add it to active effects.
                if (!effect.Tick())
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