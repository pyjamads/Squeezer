using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace GameFeelDescriptions
{
    [RequireComponent(typeof(ABTest))]
    public class InteractiveEvolution : MonoBehaviour
    {
        private KeyCode[] keyCodes = {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0,
        };

        private string SessionSavePath = Path.Combine(GameFeelDescription.savePath, "EvolvedDescriptions");
        
        public string SceneName;
        
        public ABTest activeEffectSwitcher;
        public KeyCode selectNextEffect = KeyCode.N;
        public KeyCode selectPrevEffect = KeyCode.P;
        
        public GameObject InitialVariation;

        //public int VariationCount = 8;
        
        [Header("How long in seconds, to show each individual setup for")]
        public float displayTimer = 10f;

        public KeyCode increaseDisplayTime = KeyCode.Plus;
        public KeyCode decreaseDisplayTime = KeyCode.Minus;
        
        public bool autoSwithcing = true;
        public KeyCode toggleAutoSwitching = KeyCode.Space;
        
        public KeyCode evolveSelectedEffect = KeyCode.Return;
        
        public KeyCode saveSelectedEffect = KeyCode.S;
        // [Header("Whether to initialize all descriptions with the settings below.")]
        // public bool initializeDescriptions;
        //
        // public TextAsset SeedDescription;

        // [Range(1,10)]
        // public int intensity = 1;
        //
        // public StepThroughModeWindow.EffectGeneratorCategories category;
        //
        
        private int selectedVariationIndex = 0;

        private float lastSwitchTime = 3f; //3f allows for a 3 second grace period after pressing Run.
        private float lastEvolveTime = -20f; //-20f to make sure the "Mutation Complete text does show up on start.

        //private static InteractiveEvolution Instance { get; set; }
        private bool reloadingScene;

        private int selectedDescriptionIndex = 0;
        private int selectedTriggerIndex = 0;
        
        // Start is called before the first frame update
        void Start()
        {
            var name = gameObject.scene.name;
            DontDestroyOnLoad(gameObject);
            SceneManager.UnloadSceneAsync(name);
            
            var scene = SceneManager.GetActiveScene();
            SceneName = scene.name;

            activeEffectSwitcher = GetComponent<ABTest>();
            activeEffectSwitcher.enabled = false;
            activeEffectSwitcher.displayCurrent = false;

            //AddDescriptionsToSwitcher();
            AddChildrenToSwitcher();

            activeEffectSwitcher.Initialize();
            
            var timeStamp = DateTime.Now.ToString("s").Replace(':', '.');
            SessionSavePath = Path.Combine(SessionSavePath, "Session_" + timeStamp);
        }

        private void AddChildrenToSwitcher()
        {
            //Add children to switcher Groups.
            //var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
            for (int i = 0; i < transform.childCount; i++)
            {
                var descObj = transform.GetChild(i).gameObject;
                //add it if it doesn't already exist.
                if (!activeEffectSwitcher.ABGroups.Contains(descObj))
                {
                    activeEffectSwitcher.ABGroups.Add(descObj);
                }

                //NOTE: maybe we should include disabled descriptions, but we're not doing that now... 2020-10-27
                var descriptions = descObj.GetComponentsInChildren<GameFeelDescription>();

                for (int descriptionIndex = 0; descriptionIndex < descriptions.Length; descriptionIndex++)
                {
                    //Skip unselected descriptions.
                    if(selectedDescriptionIndex != -1 && selectedDescriptionIndex != descriptionIndex) continue;
                    
                    var description = descriptions[descriptionIndex];
                    for (int triggerIndex = 0; triggerIndex < description.TriggerList.Count; triggerIndex++)
                    {
                        //Skip unselected triggers.
                        if(selectedTriggerIndex != -1 && selectedTriggerIndex != triggerIndex) continue;
                        
                        var trigger = description.TriggerList[triggerIndex];
                        if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                        {
                            trigger.EffectGroups.Add(new GameFeelEffectGroup(){GroupName = trigger.GetType().Name+"Group"});
                        }

                        for (int groupIndex = 0; groupIndex < trigger.EffectGroups.Count; groupIndex++)
                        {
                            var group = trigger.EffectGroups[groupIndex];

                            if (group.EffectsToExecute == null || group.EffectsToExecute.Count == 0)
                            {
                                ReplaceRecipeOnGroup(group);
                            }
                        }
                    }
                }
            }
        }
        
        // private void AddDescriptionsToSwitcher()
        // {
        //     //Add children to switcher Groups.
        //     var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
        //     for (int i = 0; i < descriptions.Length; i++)
        //     {
        //         var description = descriptions[i];
        //         activeEffectSwitcher.ABGroups.Add(description.gameObject);
        //
        //         if (initializeDescriptions)
        //         {
        //             if (SeedDescription != null)
        //             {
        //                 try
        //                 {
        //                     var desc = GameFeelDescription.LoadDescriptionFromJson(SeedDescription.text);
        //                     description.OverrideDescriptionData(desc, false);
        //                     
        //                     foreach (var trigger in description.TriggerList)
        //                     {
        //                         foreach (var effectGroup in trigger.EffectGroups)
        //                         {
        //                             //Take the seeded tree, and mutate it!
        //                             MutateGroup(effectGroup, 0.25f, 0.25f, 0.10f);
        //                         }
        //                     }
        //                     continue;
        //                 }
        //                 catch (Exception)
        //                 {
        //                     Debug.LogWarning(SeedDescription.name + " is not a serialized Game Feel Description.");
        //                 }
        //             }
        //             
        //             foreach (var trigger in description.TriggerList)
        //             {
        //                 foreach (var effectGroup in trigger.EffectGroups)
        //                 {
        //                     var recipe = StepThroughModeWindow.GenerateRecipe(category, intensity);
        //
        //                     effectGroup.ReplaceEffectsWithRecipe(recipe);
        //
        //                     //Take the handcrafted tree, and mutate it!
        //                     MutateGroup(effectGroup, 0.25f, 0.25f, 0.10f);
        //                 }
        //             }
        //         }
        //     }
        // }
        
        private void OnGUI()
        {
            if (transform.childCount != activeEffectSwitcher.ABGroups.Count && transform.childCount < activeEffectSwitcher.currentGroup) return;
            
            var currentGroup = transform.GetChild(activeEffectSwitcher.currentGroup);
            
            if (Time.unscaledTime - lastEvolveTime < 2f)
            {
                GUI.Label(new Rect(Screen.width / 2f - 50, 100, 100, 100), "Mutation complete!");
            }
            else
            {
                GameFeelDescription selectedDescription = null;
                GameFeelTrigger selectedTrigger = null;
                var trigName = "Everything";

                if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
                {
                    selectedDescription = InitialVariation.GetComponentsInChildren<GameFeelDescription>()[selectedDescriptionIndex];
                    selectedTrigger = selectedDescription.TriggerList[selectedTriggerIndex];
                    trigName = selectedDescription.name + "_" + selectedTrigger.GetType().Name;
                }
                
                var fullWidth = Screen.width - 80f;
                var buttonWidth = fullWidth / 4f - 10f;

                for (var i = 0; i < transform.childCount; i++)
                {
                    var selected = i == activeEffectSwitcher.currentGroup;
                 
                    if (selected 
                        ? GUI.Button(new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 40 * (i / 4), buttonWidth, 30),
                            "Select Variation " + (i+1), EditorStyles.helpBox) //HELP BOX STYLE WHEN SELECTED! 
                        : GUI.Button(new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 40 * (i / 4), buttonWidth, 30),
                            "Select Variation " + (i+1)))
                    {
                        ReloadScene();
                        activeEffectSwitcher.SwitchToGroup(i);
                        autoSwithcing = false;    
                    }
                }
                
                // if (GUI.Button(new Rect(40, Screen.height / 2f, 80, 30),
                //     "Previous"))
                // {
                //     ReloadScene();
                //     activeEffectSwitcher.SwitchToPrevGroup();
                //     lastSwitchTime = Time.unscaledTime;
                // }
                //Replacing previous with trigger selection.

                GUILayout.BeginArea(new Rect(40, Screen.height / 2f - 300, 250, 600));
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Select trigger to evolve effects for:");
                    
                    var descriptions = InitialVariation.GetComponentsInChildren<GameFeelDescription>();

                    for (var descriptionIndex = 0; descriptionIndex < descriptions.Length; descriptionIndex++)
                    {
                        var description = descriptions[descriptionIndex];
                        
                        GUILayout.Label(description.name);

                        for (var triggerIndex = 0; triggerIndex < description.TriggerList.Count; triggerIndex++)
                        {
                            var trigger = description.TriggerList[triggerIndex];
                            
                            if (selectedDescriptionIndex == descriptionIndex && selectedTriggerIndex == triggerIndex)
                            {
                                GUILayout.Toggle(true, "Evolve "+trigger.GetType().Name); 
                            }
                            else
                            {
                                if (GUILayout.Toggle(false, "Evolve "+trigger.GetType().Name))
                                {
                                    selectedDescriptionIndex = descriptionIndex;
                                    selectedTriggerIndex = triggerIndex;

                                    //Copy the currently selected individual to all
                                    CopySelected(activeEffectSwitcher.currentGroup);
                                    
                                    descriptions = InitialVariation.GetComponentsInChildren<GameFeelDescription>();
                                    
                                    //Reinitialize the selected triggers!
                                    AddChildrenToSwitcher();
                                }
                            }
                        }
                    }
                    
                    if(selectedDescriptionIndex == -1 && selectedTriggerIndex == -1)
                    {
                        GUILayout.Toggle(true, "Evolve Everything!");
                    }
                    else
                    {
                        if (GUILayout.Toggle(false, "Evolve Everything!"))
                        {
                            selectedDescriptionIndex = -1;
                            selectedTriggerIndex = -1;

                            //Copy the currently selected individual to all
                            CopySelected(activeEffectSwitcher.currentGroup);
                                
                            AddChildrenToSwitcher();
                        }
                    }
                }
                GUILayout.EndArea();
                
                
                var timer = " ("+(displayTimer - (Time.unscaledTime - lastSwitchTime)).ToString("F1")+"s)";
                if (GUI.Button(new Rect(Screen.width - 40 - 100, Screen.height / 2f, 100, 30),
                    "Next"+(autoSwithcing ? timer : "")))
                {
                    ReloadScene();
                    activeEffectSwitcher.SwitchToNextGroup();
                    lastSwitchTime = Time.unscaledTime;
                }
                
                //Evaluation time adjustment
                if (autoSwithcing)
                {
                    if (GUI.Button(new Rect(Screen.width - 40 - 100, Screen.height / 2f + 35f, 40, 30), "+5s"))
                    {
                        lastSwitchTime = Time.unscaledTime;
                        displayTimer += 5f;
                    }
                    
                    if (GUI.Button(new Rect(Screen.width - 80, Screen.height / 2f + 35f, 40, 30), "-5s"))
                    {
                        lastSwitchTime = Time.unscaledTime;
                        displayTimer -= 5f;
                    }
                }

                var variationID = activeEffectSwitcher.currentGroup +1;
                
                if (GUI.Button(new Rect((Screen.width / 2f) + 5f, Screen.height - 30, 300, 30),
                    "Evolve Variation " + variationID))
                {
                    autoSwithcing = false;
                    EvolveSelected();
                }

                if (GUI.Button(new Rect(40f, Screen.height - 30, 300, 30),
                    "Reset "+trigName+"!"))
                {
                    autoSwithcing = false;
                    ResetEvolution();
                }


                //TODO: maybe only save the current selected description and/or trigger, when bookmarking!
                if (GUI.Button(new Rect((Screen.width / 2f) - 205f, Screen.height - 30, 200, 30),
                    "Save Variation "+variationID))
                {
                    var currentDescriptions = currentGroup.GetComponentsInChildren<GameFeelDescription>(true);
                    
                    foreach (var description in currentDescriptions)
                    {
                        var name = "Variation_"+variationID +"_"+ description.name+ ".txt";
                        Debug.Log("THE DESCRIPTION IS SAVED!!\n"+SessionSavePath+"/"+name);

                        GameFeelDescription.SaveToFile(description, name, SessionSavePath);    
                    }
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Group switching
            if (autoSwithcing && Time.unscaledTime > lastSwitchTime + displayTimer)
            {
                //var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
                if (activeEffectSwitcher.ABGroups.Count != transform.childCount)
                {
                    activeEffectSwitcher.ABGroups.Clear();
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        activeEffectSwitcher.ABGroups.Add(transform.GetChild(i).gameObject);    
                    }
                }
                
                lastSwitchTime = Time.unscaledTime;
                ReloadScene();
                activeEffectSwitcher.SwitchToNextGroup();
            }

            for (int i = 0; i < activeEffectSwitcher.ABGroups.Count; i++)
            {
                if (i >= 0 && i < 9 && Input.GetKeyDown(keyCodes[i]))
                {
                    ReloadScene();
                    activeEffectSwitcher.SwitchToGroup(i);
                    autoSwithcing = false;
                }
                else if (Input.GetKeyDown(keyCodes[9]))
                {
                    ReloadScene();
                    //For anything above 9 groups, pressing "0" just goes to the next group.
                    activeEffectSwitcher.SwitchToNextGroup();
                    autoSwithcing = false;
                }
            }

            if (Input.GetKeyDown(increaseDisplayTime))
            {
                displayTimer += 1f;
            }
            
            if (Input.GetKeyDown(decreaseDisplayTime))
            {
                displayTimer = Mathf.Max(3f, displayTimer - 1f);
            }
            
            if (Input.GetKeyDown(selectNextEffect))
            {
                lastSwitchTime = Time.unscaledTime;
                ReloadScene();
                activeEffectSwitcher.SwitchToNextGroup();
            }
            
            if (Input.GetKeyDown(selectPrevEffect))
            {
                lastSwitchTime = Time.unscaledTime;
                ReloadScene();
                activeEffectSwitcher.SwitchToPrevGroup();
            }
            
            if (Input.GetKeyDown(evolveSelectedEffect))
            {
                autoSwithcing = false;
                EvolveSelected();
            }
            
            if (Input.GetKeyDown(toggleAutoSwitching))
            {
                autoSwithcing = !autoSwithcing;
            }

            if (Input.GetKeyDown(saveSelectedEffect))
            {
                var autoOn = autoSwithcing;
                autoSwithcing = false;
                var currentDescriptions = activeEffectSwitcher.ABGroups[activeEffectSwitcher.currentGroup]
                    .GetComponentsInChildren<GameFeelDescription>();
                    
                foreach (var description in currentDescriptions)
                {
                    var name = "Variation"+activeEffectSwitcher.currentGroup +"_"+ description.name+ ".txt";
                    Debug.Log("THE DESCRIPTION IS SAVED!!\n"+SessionSavePath+"/"+name);

                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);    
                }
                
                autoSwithcing = autoOn;
            }

            //TODO: loading bookmarks into the evolution 2020-10-05
        }

        private void EvolveSelected()
        {
            if (Time.unscaledTime < lastEvolveTime + 1f)
            {
                //Evolve cooldown of 1 second!
                return;
            }
            
            lastSwitchTime = Time.unscaledTime;
            lastEvolveTime = Time.unscaledTime;    
                
            // if (autoSwithcing)
            // {
            //     //NOTE: Nothing was selected, so we're gonna just mutate all of them...
            //     Evolve();
            // }
            // else
            // {
            Evolve(activeEffectSwitcher.currentGroup);
            // }
            
            ReloadScene();

            //Re-enable autoSwitching...
            autoSwithcing = true;
        }

        private void ResetEvolution()
        {
            if (Time.unscaledTime < lastEvolveTime + 1f)
            {
                //Evolve cooldown of 1 second!
                return;
            }
            
            lastSwitchTime = Time.unscaledTime;
            lastEvolveTime = Time.unscaledTime - 30f; //We don't want it to say Mutation Complete here!
            
            //Randomize all descriptions based on their trigger groups selected category and intensity.
            Randomize();
            
            ReloadScene();

            //Re-enable autoSwitching...
            autoSwithcing = true;
        }

        private void ReloadScene()
        {
            if (reloadingScene) return;
            
            reloadingScene = true;
            
            //Stop all current effects!
            GameFeelEffectExecutor.Instance.activeEffects.Clear();
            //Reset timeScale!
            Time.timeScale = 1f;
            
            //Remove all the current effect generated gameObjects
            var childCount = GameFeelEffectExecutor.Instance.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = GameFeelEffectExecutor.Instance.transform.GetChild(i);
                Destroy(child.gameObject);
            }
            
            //Reload the main scene...
            if (SceneName != null)
            {
                var unload = SceneManager.UnloadSceneAsync(SceneName);
                StartCoroutine(LoadAndReattachTriggers(unload));
            }

            IEnumerator LoadAndReattachTriggers(AsyncOperation unloadOperation)
            {
                yield return unloadOperation;
                
                yield return SceneManager.LoadSceneAsync(SceneName, LoadSceneMode.Single);

                //Reattach triggers
                CustomMenus.AttachAllTriggers();
                
                reloadingScene = false;
            }
        }

        public void CopySelected(int index)
        {
            var individuals = activeEffectSwitcher.ABGroups.Count;
            
            //If an individual is selected!
            if (0 <= index && index < individuals)
            {
                //Copy the selected description, to all other descriptions
                //var selectedIndividual = descriptions[index];
                var selectedIndividual = transform.GetChild(index).gameObject;
                InitialVariation = selectedIndividual;
                
                //Destoy all other individuals
                for (int i = 0; i < individuals; i++)
                {
                    if (i != index)
                    {
                        DestroyImmediate(activeEffectSwitcher.ABGroups[i]);  
                    }
                }
                
                //Clear AB list and add the selected individual again.
                activeEffectSwitcher.ABGroups.Clear();
                activeEffectSwitcher.ABGroups.Add(selectedIndividual);
                
                //Select the previous selection.
                activeEffectSwitcher.currentGroup = 0;

                //var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>(true);

                //NOTE: for more advanced evolution, with multiple targets in a scene, maybe only copy over the effect groups or effect lists.
                //var json = JsonUtility.ToJson(selectedIndividual, true);

                for (int i = 0; i < individuals - 1; i++)
                {
                    var individualCopy = Instantiate(selectedIndividual, transform);
                    activeEffectSwitcher.ABGroups.Add(individualCopy);
                }
                
                //Gently mutate the selected individual...
                //NOTE: we're ignoring the inactive descriptions here!
                var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>();
                
                var evolveName = "Saved_Everything";

                if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
                {
                    var description = descriptions[selectedDescriptionIndex];
                    var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                    evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Saved";
                }
                
                //Also save the selected individual!
                foreach (var description in descriptions)
                {
                    var name = description.name + ".txt";
                    
                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                }

                SessionSavePath = Path.Combine(SessionSavePath, evolveName);
            }
        }
        
        public void Evolve(int index)
        {
            var individuals = activeEffectSwitcher.ABGroups.Count;
            
            //If an individual is selected!
            if (0 <= index && index < individuals)
            {
                //Copy the selected description, to all other descriptions
                //var selectedIndividual = descriptions[index];
                var selectedIndividual = transform.GetChild(index).gameObject;
                InitialVariation = selectedIndividual;
                
                //Destoy all other individuals
                for (int i = 0; i < individuals; i++)
                {
                    if (i != index)
                    {
                        DestroyImmediate(activeEffectSwitcher.ABGroups[i]);  
                    }
                }
                
                //Clear AB list and add the selected individual again.
                activeEffectSwitcher.ABGroups.Clear();
                activeEffectSwitcher.ABGroups.Add(selectedIndividual);
                
                //Select the previous selection.
                activeEffectSwitcher.currentGroup = 0;

                //var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>(true);

                //NOTE: for more advanced evolution, with multiple targets in a scene, maybe only copy over the effect groups or effect lists.
                //var json = JsonUtility.ToJson(selectedIndividual, true);

                for (int i = 0; i < individuals - 1; i++)
                {
                    var individualCopy = Instantiate(selectedIndividual, transform);
                    activeEffectSwitcher.ABGroups.Add(individualCopy);
                    
                    //NOTE: This is sub-optimal, but it creates a "deep" copy of the object.
                    // var data = GameFeelDescription.LoadDescriptionFromJson(json);
                    // desc.OverrideDescriptionData(data);
                    
                    //NOTE: we're ignoring the inactive descriptions here!
                    var descs = individualCopy.GetComponentsInChildren<GameFeelDescription>();

                    if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                        selectedDescriptionIndex >= descs.Length)
                    {
                        //Mutate all the descriptions on the copy.
                        foreach (var desc in descs)
                        {
                            foreach (var trigger in desc.TriggerList)
                            {
                                foreach (var effectGroup in trigger.EffectGroups)
                                {
                                    //Aggressively mutate copies
                                    MutateGroup(effectGroup, 0.05f, 0.4f, 0.4f);
                                }
                            }
                        }    
                    }
                    else
                    {
                        //Mutate the selected description and trigger.
                        var description = descs[selectedDescriptionIndex];
                        var trigger = description.TriggerList[selectedTriggerIndex];
                        
                        foreach (var effectGroup in trigger.EffectGroups)
                        {
                            MutateGroup(effectGroup, 0.05f, 0.4f, 0.4f);
                        }
                    }
                }
                
                //Gently mutate the selected individual...
                //NOTE: we're ignoring the inactive descriptions here!
                var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>();
                
                var evolveName = "Evolved_Everything";

                if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
                {
                    var description = descriptions[selectedDescriptionIndex];
                    var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                    evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Evolved";
                }
                
                //Also save the selected individual!
                foreach (var description in descriptions)
                {
                    var name = description.name + ".txt";
                    
                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                }

                SessionSavePath = Path.Combine(SessionSavePath, evolveName);
                
                if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                    selectedDescriptionIndex >= descriptions.Length)
                {
                    //Mutate all the descriptions on the copy.
                    foreach (var description in descriptions)
                    {
                        foreach (var trigger in description.TriggerList)
                        {
                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                MutateGroup(effectGroup, 0.05f, 0, 0);
                            }
                        }
                    }
                }
                else
                {
                    //Mutate the selected description and trigger.
                    var description = descriptions[selectedDescriptionIndex];
                    var trigger = description.TriggerList[selectedTriggerIndex];
                        
                    foreach (var effectGroup in trigger.EffectGroups)
                    {
                        MutateGroup(effectGroup, 0.05f, 0, 0);
                    }
                }
            }
            // else
            // {
            //     //NOTE: we're ignoring the inactive descriptions here!
            //     var descriptions = transform.GetComponentsInChildren<GameFeelDescription>();
            //     
            //     var evolveName = "Evolved_Everything";
            //     
            //     if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
            //     {
            //         var description = descriptions[selectedDescriptionIndex];
            //         var selectedTrigger = description.TriggerList[selectedTriggerIndex];
            //         evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Evolved";
            //     }
            //     
            //     //Also save the evolved individuals prior to evolving!
            //     foreach (var description in descriptions)
            //     {
            //         var name = description.name + ".txt";
            //         
            //         GameFeelDescription.SaveToFile(description, name, SessionSavePath);
            //     }
            //     
            //     SessionSavePath = Path.Combine(SessionSavePath, evolveName);
            //     
            //     //Mutate all of them...
            //     foreach (var description in descriptions)
            //     {
            //         if ()
            //         {
            //             foreach (var trigger in description.TriggerList)
            //             {
            //                 foreach (var effectGroup in trigger.EffectGroups)
            //                 {
            //                     //Mutate them a lot.
            //                     MutateGroup(effectGroup, 0.25f, 0.4f, 0.4f);
            //                 }
            //             }
            //         }
            //         else
            //         {
            //             //Mutate the selected description and trigger.
            //             var description = descriptions[selectedDescriptionIndex];
            //             var trigger = description.TriggerList[selectedTriggerIndex];
            //             
            //             foreach (var effectGroup in trigger.EffectGroups)
            //             {
            //                 MutateGroup(effectGroup, 0.05f, 0, 0);
            //             }
            //         }
            //     }
            // }
        }

        public void Randomize(bool useGroupCategories = true)
        {
            var evolveName = "Reset_Everything";
                
            if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
            {
                var descriptions = InitialVariation.GetComponentsInChildren<GameFeelDescription>();
                var description = descriptions[selectedDescriptionIndex];
                var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Reset";
            }

            SessionSavePath = Path.Combine(SessionSavePath, evolveName);
            
            //DO THE RESET!!
            foreach (var individual in activeEffectSwitcher.ABGroups)
            {
                var descs = individual.GetComponentsInChildren<GameFeelDescription>();                            

                if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                    selectedDescriptionIndex >= descs.Length)
                {
                    //Mutate all the descriptions on the copy.
                    foreach (var desc in descs)
                    {
                        foreach (var trigger in desc.TriggerList)
                        {
                            if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                            {
                                trigger.EffectGroups.Add(new GameFeelEffectGroup(){GroupName = trigger.GetType().Name+"Group"});
                            }
                            
                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                ReplaceRecipeOnGroup(effectGroup, useGroupCategories);
                            }
                        }
                    }    
                }
                else
                {
                    //Mutate the selected description and trigger.
                    var description = descs[selectedDescriptionIndex];
                    var trigger = description.TriggerList[selectedTriggerIndex];
                        
                    if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                    {
                        trigger.EffectGroups.Add(new GameFeelEffectGroup(){GroupName = trigger.GetType().Name+"Group"});
                    }
                    
                    foreach (var effectGroup in trigger.EffectGroups)
                    {
                        ReplaceRecipeOnGroup(effectGroup, useGroupCategories);
                    }
                }
            }
        }

        private void ReplaceRecipeOnGroup(GameFeelEffectGroup group, bool useGroupCategories = true)
        {
            var recipe = StepThroughModeWindow.GenerateRecipe(useGroupCategories ? group.selectedCategory : StepThroughModeWindow.EffectGeneratorCategories.RANDOM, group.selectedIntensity, group.EffectsToExecute);
            
            group.ReplaceEffectsWithRecipe(recipe);
            
            MutateGroup(group, 0.25f, 0.25f, 0.10f);
        }


        public static void MutateGroup(GameFeelEffectGroup gameFeelEffectGroup, float mutateAmount = 0.05f,
            float addProbability = 0.05f, float removeProbability = 0.05f)
        {
            var constructors = GameFeelBehaviorBase<GameFeelTrigger>.GetGameFeelEffects();

            if (gameFeelEffectGroup.EffectsToExecute.Count > 2)
            {
                //Group level remove, then mutate, then add!
                var unlockedEffects = gameFeelEffectGroup.EffectsToExecute.Where(item => !item.Lock);
                if (RandomExtensions.Boolean(removeProbability))
                {
                    var effectToRemove = unlockedEffects.GetRandomElement();
                    gameFeelEffectGroup.EffectsToExecute.Remove(effectToRemove);
                    //TODO: Consider whether this should be a "replace",
                    //TODO: because currently it's addProb * removeProb that a replace happens. 2020-07-15
                }    
            }

            //Mutate all effects in the list recursively. Also drop the probabilities for adding and removing by 10% per layer.
            gameFeelEffectGroup.EffectsToExecute.ForEach(item =>
                MutateEffectsRecursive(constructors, item, mutateAmount, addProbability * 0.9f, removeProbability * 0.9f));

            //Add new effect to group...
            if (RandomExtensions.Boolean(addProbability))
            {
                var instance = constructors.GetRandomElement().Invoke();
                gameFeelEffectGroup.EffectsToExecute.Add(instance);
            }
        }

        public static void MutateEffectsRecursive(List<Func<GameFeelEffect>> effectConstructors,
            GameFeelEffect outerEffect,
            float amount = 0.05f, float addProb = 0.05f, float removeProb = 0.05f, List<GameFeelEffect> traversed = null)
        {
            //Make a visited list!
            var visited = new List<GameFeelEffect>();
            if(traversed != null && !(outerEffect is ParticlePuffEffect || outerEffect is ShatterEffect)) visited.AddRange(traversed);
            visited.Add(outerEffect);
            
            //Remove an unlocked child
            if (RandomExtensions.Boolean(removeProb))
            {
                //If it's a spawner effect, add it to the offspring effects instead.
                if (outerEffect is SpawningGameFeelEffect spawner && spawner.ExecuteOnOffspring.Count > 0)
                {
                    var element = spawner.ExecuteOnOffspring.Where(item => !item.Lock).GetRandomElement();

                    //Don't remove Destroy effects from spawner offspring effects.
                    if (element is DestroyEffect || element.ExecuteAfterCompletion.Any(item => item is DestroyEffect))
                    {
                        //TODO: figure out if we should do something else instead. 2020-09-29
                    }
                    else
                    {
                        spawner.ExecuteOnOffspring.Remove(element);    
                    }
                }
                else if(outerEffect.ExecuteAfterCompletion.Count > 0)
                {
                    var element = outerEffect.ExecuteAfterCompletion.Where(item => !item.Lock).GetRandomElement();

                    //Don't remove Destroy effects from spawner offspring effects.
                    if ((element is DestroyEffect || element.ExecuteAfterCompletion.Any(item => item is DestroyEffect)) && 
                        visited.Any(item => item is SpawningGameFeelEffect))
                    {
                        //TODO: figure out if we should do something else instead. 2020-09-29
                    }
                    else
                    {
                        outerEffect.ExecuteAfterCompletion.Remove(element);    
                    }
                }
            }

            //Run mutate on the children.
            foreach (var innerEffect in outerEffect.ExecuteAfterCompletion)
            {
                if (!innerEffect.Lock)
                {
                    innerEffect.Mutate(amount);
                }

                MutateEffectsRecursive(effectConstructors, innerEffect, amount, addProb * 0.9f, removeProb * 0.9f, visited);
            }

            if (outerEffect is SpawningGameFeelEffect spawnerEffect)
            {
                //Run mutate on the children.
                foreach (var innerEffect in spawnerEffect.ExecuteOnOffspring)
                {
                    if (!innerEffect.Lock)
                    {
                        innerEffect.Mutate(amount);
                    }

                    MutateEffectsRecursive(effectConstructors, innerEffect, amount, addProb * 0.9f, removeProb * 0.9f, visited);
                }
            }

            //TODO: Consider moving this to only unlocked effects 2020-08-20
            //Always allow a small probability for adding an extra effect?
            if (RandomExtensions.Boolean(addProb))
            {
                var instance = effectConstructors.GetRandomElement().Invoke();

                var isParticleSpawner = instance is ShatterEffect || instance is ParticlePuffEffect;

                //Don't add Shatter or Particle puff effects to other shatter/puff offspring
                if (isParticleSpawner && visited.Any(item => item is ShatterEffect || item is ParticlePuffEffect))
                {
                    //TODO: figure out if we should do anything else here. 2020-09-29
                }
                else
                {
                    //If it's a spawner effect, add it to the offspring effects instead.
                    if (outerEffect is SpawningGameFeelEffect spawner)
                    {
                        //If a destroy effect exists, don't add the effect unless it's a spawningEffect.
                        if (spawner.ExecuteOnOffspring.Any(item => item is DestroyEffect) || 
                            visited.Any(item => item is DestroyEffect))
                        {
                            if (instance is SpawningGameFeelEffect)
                            {
                                spawner.ExecuteOnOffspring.Add(instance); 
                            }
                        }
                        else
                        {
                            spawner.ExecuteOnOffspring.Add(instance);    
                        }
                    }
                    else
                    {
                        //If a destroy effect exists, don't add the effect unless it's a spawningEffect.
                        if (outerEffect.ExecuteAfterCompletion.Any(item => item is DestroyEffect) || 
                            visited.Any(item => item is DestroyEffect))
                        {
                            if (instance is SpawningGameFeelEffect)
                            {
                                outerEffect.ExecuteAfterCompletion.Add(instance); 
                            }
                        }
                        else
                        {
                            outerEffect.ExecuteAfterCompletion.Add(instance);
                        }
                    }
                }
            }

            if (!outerEffect.Lock)
            {
                outerEffect.Mutate(amount);
                
                //Check if this effect is looping infinitely...
                var loopingInfinitely = outerEffect.GetRemainingTime() == float.PositiveInfinity;

                if (loopingInfinitely)
                {
                    //Remove all execute after completion effects if looping infinitely.
                    outerEffect.ExecuteAfterCompletion.Clear();
                }
            }
        }
    }
}