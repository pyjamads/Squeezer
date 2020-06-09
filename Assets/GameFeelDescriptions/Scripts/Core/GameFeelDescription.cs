
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace GameFeelDescriptions
{
    //Data struct for deserializing a GameFeelDescription.
    [Serializable]
    public class GameFeelDescriptionDataStruct
    {
        #region Fields

        public string Name;

        public string Description;

        public bool StepThroughMode;

        public float DynamicReattachRate;

        public string AttachToTag;
        
        public GameObject[] AttachToObjects;
        
        public string AttachToComponentType;
        
        [SerializeReference]
        public List<GameFeelTrigger> TriggerList;

        #endregion
    }
    
    [DisallowMultipleComponent]
    [HelpURL("https://github.com/pyjamads/GameFeelDescriptions")]
    [Serializable]
    public class GameFeelDescription : MonoBehaviour
    //TODO: rename to something more appropriate, like GameFeelAugmentation or GameFeelEnhancement or something...04/03/2020 
    {
        public const string savePath = "Assets/GameFeelDescriptions/Recipes";
        public const bool saveDataForUserStudy = true;
        public static string userStudyPath = Path.Combine(savePath, "UserStudyData");
        
        [NonSerialized]
        public List<GameFeelBehaviorBase> attachedTriggers = new List<GameFeelBehaviorBase>();
        
        #region Fields (Remember GameFeelDescriptionDataStruct when adding/removing fields, as well as OverrideDescriptionData)
        
        /// <summary>
        /// The name of this description.
        /// </summary>
        public string Name;

        /// <summary>
        /// Description of what this entire description does.
        /// </summary>
        [TextArea]
        public string Description;

        //TODO: make multiline header? 31/05/2020
        [Header("How often should the description try to reattach triggers.", order=1)]
        [Tooltip("Only use this if objects with the tag/component are dynamically spawned at runtime.\n0 or less means only attach at start up.")]
        public float DynamicReattachRate;
        
        /// <summary>
        /// The tag to find and apply listeners to.  
        /// </summary>
        [Header("The triggers will be attached to all objects with this tag.")]
        public string AttachToTag;
        //AttachToName will not work, as there's no way to lookup all objects with a certain name easily.
        
        //Attach to a specified object might be desirable later on.
        [Header("The triggers will be attached to all objects in this list.")]
        public GameObject[] AttachToObjects;

        //Attaching to a componentType, might be a good substitute for Tag/Name.
        [Header("The triggers will be attached to all objects with this component type.")]
        public string AttachToComponentType;

        [Header("This mode allows you to add effects as events happen while playing in the editor.")]
        public bool StepThroughMode;
        
        [SerializeReference]
        [ShowTypeAttribute]
        public List<GameFeelTrigger> TriggerList = new List<GameFeelTrigger>();

        #endregion

        #region MonoBehavior functionality

        public void OnEnable()
        {
            //NOTE: Moved from Start() to Awake(), to deal with setting up OnStartTrigger, before Start is run. 12/02/2020 
            //TODO: figure out if this is the most optimal way to set these up. 04/02/2020
            AttachTriggers();
            LastAttachTime = Time.unscaledTime;
        }

        public void OnDisable()
        {
            RemoveTriggers();
        }


        #if UNITY_EDITOR
        [NonSerialized]
        private List<GameObject> attachingTo;
        
        [ExecuteAlways]
        private void OnDrawGizmosSelected()
        {
            if (attachingTo == null || attachingTo.Count == 0)
            {
                attachingTo = FindGameObjectsToAttach(); 
            }

            Handles.zTest = CompareFunction.LessEqual;
            
            //Draw line to all the things this Description is attaching to!
            foreach (var go in attachingTo)
            {
                var descPos = transform.position;
                var attachPos = go.transform.position;

                var halfPos = (descPos - attachPos) * 0.25f;
                
                Handles.DrawBezier(descPos, 
                    attachPos, 
                    descPos - halfPos.y * Vector3.up - halfPos.x * Vector3.right, 
                    attachPos + halfPos,
                    Color.white,
                    Texture2D.whiteTexture, 
                    1f);
            }
        }
        #endif

        private float LastAttachTime = 0f;

        
        private void LateUpdate()
        {
            if (
                #if UNITY_EDITOR
                EditorApplication.isPlaying &&
                #endif
                DynamicReattachRate > 0 &&
                Time.unscaledTime - LastAttachTime > DynamicReattachRate)
            {
                AttachTriggers(true);
                LastAttachTime = Time.unscaledTime;
            }
        }

        #endregion
        
        #region Setup the triggers and Targets
        
        public void AttachTriggers(bool reattach = false)
        {
            if (string.IsNullOrEmpty(AttachToTag) && AttachToComponentType == null)
            {
                Debug.LogException(new Exception(Name+": No AttachToTag or AttachToComponentType defined."));
                return;
            }
            
            #if UNITY_EDITOR
            if (StepThroughMode && !reattach)
            {
                //NOTE: Just add collisions to step through mode for now! 11/05/2020
                Debug.Log("Description StepThroughMode currently only supports collision step through. " +
                          "For other trigger types use StepThroughMode on Effect Groups.");
                var trigger = (OnCollisionTrigger)GameFeelTrigger.CreateTrigger(GameFeelTriggerType.OnCollision);
                trigger.ReactTo = new List<string>();
                trigger.type = OnCollisionTrigger.CollisionActivationType.OnAllEnter;
                
                //Step through mode will handle any case not currently handled by a trigger.
                foreach (var otherTrigger in TriggerList.Where(item => item is OnCollisionTrigger))
                {
                    var col = (OnCollisionTrigger) otherTrigger;
                    foreach (var otherTag in col.ReactTo)
                    {
                        trigger.ReactTo.Add("!"+otherTag);
                    }
                }

                if (trigger.ReactTo.Count == 0)
                {
                    trigger.ReactTo.Add("*");
                }
                
                TriggerList.Add(trigger);
            }
            #endif
            
            if (TriggerList.Count == 0)
            {
                Debug.LogWarning(Name+": No items in TriggerList");
                return;
            }

            var attachTo = FindGameObjectsToAttach();

            for (var index = 0; index < TriggerList.Count; index++)
            {
                var trigger = TriggerList[index];
                if (trigger.EffectGroups.Count > 0 || StepThroughMode)
                {
                    trigger.Attach(this, attachTo, index);  
                }
            }
        }

        public List<GameObject> FindGameObjectsToAttach()
        {
            //Find game objects to attach to!
            var attachTo = new List<GameObject>();

            if (!string.IsNullOrEmpty(AttachToTag))
            {
                var objects = GameObject.FindGameObjectsWithTag(AttachToTag);
                foreach (var go in objects)
                {
                    if (attachedTriggers.Exists(item => item.gameObject == go)) continue;
                    attachTo.Add(go);
                }
            }

            if (!string.IsNullOrEmpty(AttachToComponentType))
            {
                var type = Type.GetType(AttachToComponentType);
                if (type != null && type.IsSubclassOf(typeof(Component)))
                {
                    var components = FindObjectsOfType(type);
                    foreach (Component comp in components)
                    {
                        if (attachedTriggers.Exists(item => item.gameObject == comp.gameObject)) continue;
                        attachTo.Add(comp.gameObject);
                    }    
                }
                else
                {
                    Debug.LogWarning("Failed to find Component type: "+AttachToComponentType);
                }
            }

            if (AttachToObjects?.Length > 0)
            {
                attachTo.AddRange(AttachToObjects);
            }

            return attachTo;
        }

        public void RemoveTriggers()
        {
            while(attachedTriggers.Count > 0)
            {
                DestroyImmediate(attachedTriggers[0]);
            }
        }
        #endregion
        
        #region Utils

        public static string LoadJsonFromFile(string filepath)
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(filepath)))
            {
                Debug.LogError("Path not valid: "+filepath);
                return "";
            }
#endif

            var reader = new StreamReader(filepath); 
            var json = reader.ReadToEnd();
            reader.Close();

            return json;
        }

        public static (List<GameFeelDescriptionDataStruct>, string[] files) LoadDescriptionsFromDirectory(string path)
        {
            var list = new List<GameFeelDescriptionDataStruct>();
            
#if UNITY_EDITOR         
            if (!AssetDatabase.IsValidFolder(Path.GetDirectoryName(path)))
            {
                Debug.LogError("Path not valid: "+path);
                return (list, null);
            }
#endif
            
            var files = Directory.GetFiles(path, "*.txt");

            for (var i = 0; i < files.Length; i++)
            {
                var json = LoadJsonFromFile(files[i]);
                
                list.Add(LoadDescriptionFromJson(json));
            }

            return (list, files);
        }

        //JSON utility does not allow deserialization of MonoBehaviors,
        //so we make a dataStruct instead and copy over the data.
        public static GameFeelDescriptionDataStruct LoadDescriptionFromJson(string json)
        {
            return JsonUtility.FromJson<GameFeelDescriptionDataStruct>(json);
        }

        public void OverrideDescriptionData(GameFeelDescriptionDataStruct data)
        {
            Name = data.Name;
            Description = data.Description;
            AttachToTag = data.AttachToTag;
            AttachToObjects = data.AttachToObjects;
            AttachToComponentType = data.AttachToComponentType;
            DynamicReattachRate = data.DynamicReattachRate;
            StepThroughMode = data.StepThroughMode;
            TriggerList = data.TriggerList;
        }

       

        public static void SaveToFile(GameFeelDescription description, string filename, string path = null)
        {
            var json = JsonUtility.ToJson(description, true);

            if (string.IsNullOrEmpty(path)) path = savePath;
            
#if UNITY_EDITOR         
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorHelpers.CreateFolder(path);
            }
#endif
            var writer = new StreamWriter(Path.Combine(path, filename), false);
            writer.Write(json);
            writer.Close();
        }

        #endregion
    }
}
