using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeelDescriptions
{   
    //[ExecuteInEditMode]
    public class GameFeelEffectExecutor : MonoBehaviour
    {
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

        public void QueueEffect(GameFeelEffect effect, bool forceQueue = true)
        {
            // if (forceQueue)
            // {
            //     activeEffects.Add(effect);
            //     return;
            // }
            
            //Destruction effects cannot occur during collision updates, so we delay it until next update
            //TODO: instead of passing this "forceQueue" flag, pass a isCollisionUpdate flag 2020-08-27
            //TODO: handle the whole DestroyImmediate issue as well (when copying objects) 2020-08-27
            if (forceQueue && (effect is DestroyEffect || effect is SpawningGameFeelEffect))
            {
                activeEffects.Add(effect);
                return;
            }
            
            //If Delay is Zero, execute the Effect immediately to avoid one frame of lag.
            if (effect.Delay == 0)
            {
                effect.Tick();

                //If Duration > 0, queue the "rest" of the effect.
                if (effect is DurationalGameFeelEffect durational && durational.Duration > 0)
                {
                    activeEffects.Add(effect);
                }
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
        
        public void TriggerCustomEvent(GameObject origin, string eventName, Vector3 direction) => OnCustomEventTriggered.Invoke(origin, eventName, direction);

        //TODO: make an GameFeelEventExecutor instead.
        //TODO: Make an attribute CustomEventTriggerAttribute(string eventName), that invokes this when the Method is called. 7/4/2020
        public void TriggerCustomEvent(GameObject origin, string eventName, Vector3? direction = null) => OnCustomEventTriggered.Invoke(origin, eventName, direction);

        
        //TODO: Maybe register events separately per "eventName", ie. Dictionary<string, Action<GameObject, string, Vector3?>> ... 7/4/2020
        public event Action<GameObject, string, Vector3?> OnCustomEventTriggered = delegate(GameObject o, string s, Vector3? arg3) {  };
        //public event System.Action OnNormalJump = delegate { };
    }
}