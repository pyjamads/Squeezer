using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeelDescriptions
{
    [RequireComponent(typeof(ABTest))]
    public class InteractiveEvolution : MonoBehaviour
    {
        private KeyCode[] keyCodes =
        {
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
        private List<GameFeelDescription> lastEvolvedDescriptions = new List<GameFeelDescription>();

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

        private int generationIndex;

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

            lastEvolvedDescriptions.Replace(InitialVariation.GetComponentsInChildren<GameFeelDescription>());

            //AddDescriptionsToSwitcher();
            AddChildrenToSwitcher();

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
                    if (selectedDescriptionIndex != -1 && selectedDescriptionIndex != descriptionIndex) continue;

                    var description = descriptions[descriptionIndex];
                    for (int triggerIndex = 0; triggerIndex < description.TriggerList.Count; triggerIndex++)
                    {
                        //Skip unselected triggers.
                        if (selectedTriggerIndex != -1 && selectedTriggerIndex != triggerIndex) continue;

                        var trigger = description.TriggerList[triggerIndex];
                        if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                        {
                            trigger.EffectGroups.Add(new GameFeelEffectGroup()
                                {GroupName = trigger.GetType().Name + "Group"});
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

            activeEffectSwitcher.Initialize();
        }

        private void OnGUI()
        {
            if (transform.childCount != activeEffectSwitcher.ABGroups.Count &&
                transform.childCount < activeEffectSwitcher.currentGroup) return;

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
                    selectedDescription =
                        InitialVariation.GetComponentsInChildren<GameFeelDescription>()[selectedDescriptionIndex];
                    selectedTrigger = selectedDescription.TriggerList[selectedTriggerIndex];
                    trigName = selectedDescription.name + "_" + selectedTrigger.GetType().Name;
                }

                var fullWidth = Screen.width - 80f;
                var buttonWidth = fullWidth / 4f - 10f;

                for (var i = 0; i < transform.childCount; i++)
                {
                    var selected = i == activeEffectSwitcher.currentGroup;

                    if (selected
                        ? GUI.Button(
                            new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 10 + 40 * (i / 4), buttonWidth, 30),
                            "Select Variation " + (i + 1), EditorStyles.helpBox) //HELP BOX STYLE WHEN SELECTED! 
                        : GUI.Button(
                            new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 10 + 40 * (i / 4), buttonWidth, 30),
                            "Select Variation " + (i + 1)))
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
                    GUILayout.Space(50);
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
                                GUILayout.Toggle(true, "Evolve " + trigger.GetType().Name);
                            }
                            else
                            {
                                if (GUILayout.Toggle(false, "Evolve " + trigger.GetType().Name))
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

                    if (selectedDescriptionIndex == -1 && selectedTriggerIndex == -1)
                    {
                        GUILayout.Space(25);
                        GUILayout.Toggle(true, "Evolve Everything!");
                    }
                    else
                    {
                        GUILayout.Space(25);
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


                var timer = " (" + (displayTimer - (Time.unscaledTime - lastSwitchTime)).ToString("F1") + "s)";
                if (GUI.Button(new Rect(Screen.width - 40 - 100, Screen.height / 2f - 35f, 100, 30),
                    "Next" + (autoSwithcing ? timer : "")))
                {
                    ReloadScene();
                    activeEffectSwitcher.SwitchToNextGroup();
                    lastSwitchTime = Time.unscaledTime;
                }

                if (GUI.Button(new Rect(Screen.width - 40 - 100, Screen.height / 2f, 100, 30),
                    (autoSwithcing ? "Pause" : "Resume")))
                {
                    autoSwithcing = !autoSwithcing;
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

                var variationID = activeEffectSwitcher.currentGroup + 1;

                if (GUI.Button(new Rect(40f, Screen.height - 65f, 300, 30),
                    "Regenerate " + trigName + "!"))
                {
                    autoSwithcing = false;
                    ResetEvolution();
                }

                if (GUI.Button(new Rect(40f, Screen.height - 30f, 300, 30),
                    "Clear " + trigName + "!"))
                {
                    autoSwithcing = false;
                    ClearEffects();
                }



                if (GUI.Button(new Rect((Screen.width / 2f) + 355f, Screen.height - 30, 100, 30),
                    "Re-Roll"))
                {
                    autoSwithcing = false;
                    EvolveSelected(true);
                }

                //TODO: maybe only save the current selected description and/or trigger, when bookmarking!
                if (GUI.Button(new Rect((Screen.width / 2f) - 205f, Screen.height - 30, 200, 30),
                    "Save Variation " + variationID))
                {
                    var currentDescriptions = currentGroup.GetComponentsInChildren<GameFeelDescription>(true);

                    foreach (var description in currentDescriptions)
                    {
                        var name = "Generation" + generationIndex + " Variation_" + variationID + "_" +
                                   description.name + ".txt";
                        Debug.Log("THE DESCRIPTION IS SAVED!!\n" + SessionSavePath + "/" + name);

                        GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                    }
                }

                if (GUI.Button(new Rect((Screen.width / 2f) + 5f, Screen.height - 30, 300, 30),
                    "Evolve Variation " + variationID))
                {
                    autoSwithcing = false;
                    EvolveSelected();
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
                    var name = "Generation" + generationIndex + " Variation" + activeEffectSwitcher.currentGroup + "_" +
                               description.name + ".txt";
                    Debug.Log("THE DESCRIPTION IS SAVED!!\n" + SessionSavePath + "/" + name);

                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                }

                autoSwithcing = autoOn;
            }

            //TODO: loading bookmarks into the evolution 2020-10-05
        }

        private void EvolveSelected(bool reroll = false)
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
            Evolve(activeEffectSwitcher.currentGroup, reroll);
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

        private void ClearEffects()
        {
            if (Time.unscaledTime < lastEvolveTime + 1f)
            {
                //Evolve cooldown of 1 second!
                return;
            }

            lastSwitchTime = Time.unscaledTime;
            lastEvolveTime = Time.unscaledTime - 30f; //We don't want it to say Mutation Complete here!

            //Clear the effects from the selected trigger!
            Clear();

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
                if (SceneManager.sceneCount <= 1)
                {
                    StartCoroutine(LoadAndReattachTriggers(null));
                }
                else
                {
                    var unload = SceneManager.UnloadSceneAsync(SceneName);
                    StartCoroutine(LoadAndReattachTriggers(unload));    
                }
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
                lastEvolvedDescriptions.Replace(InitialVariation.GetComponentsInChildren<GameFeelDescription>());

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

                //Increase generation index when selecting a new trigger to evolve for.
                generationIndex++;

                //Gently mutate the selected individual...
                //NOTE: we're ignoring the inactive descriptions here!
                var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>();

                var evolveName = "Saved_Everything";

                if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
                {
                    var description = descriptions[selectedDescriptionIndex];
                    var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                    evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Saved.txt";
                }

                //Also save the selected individual!
                foreach (var description in descriptions)
                {
                    var name = "Generation" + generationIndex + " " + description.name + ".txt";

                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                }

                //TODO: save a "seperator" text file with the status and name, but don't make the path deeper 2020-10-30
                //SessionSavePath = Path.Combine(SessionSavePath, evolveName);
            }
        }

        public void Evolve(int index, bool reroll = false)
        {
            var individuals = activeEffectSwitcher.ABGroups.Count;

            //If an individual is selected!
            if (0 <= index && index < individuals)
            {
                //Copy the selected description, to all other descriptions
                //var selectedIndividual = descriptions[index];
                var selectedIndividual = transform.GetChild(index).gameObject;
                InitialVariation = selectedIndividual;

                if (reroll)
                {
                    //Re-initialize selected individual with the last evolved descriptions
                    var descs = InitialVariation.GetComponentsInChildren<GameFeelDescription>();

                    for (int i = 0; i < descs.Length; i++)
                    {
                        descs[i].OverrideDescriptionData(lastEvolvedDescriptions[i]);
                    }
                }
                else
                {
                    //Otherwise save selected individual as the last evolved.
                    lastEvolvedDescriptions.Replace(InitialVariation.GetComponentsInChildren<GameFeelDescription>());
                }

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
                    //NOTE: Copying the game object, copies all descriptions in the object hierarchy.
                    var individualCopy = Instantiate(selectedIndividual, transform);
                    activeEffectSwitcher.ABGroups.Add(individualCopy);

                    //NOTE: This is an alternative, that creates a "deep" copy of the description.
                    // var data = GameFeelDescription.LoadDescriptionFromJson(json);
                    // desc.OverrideDescriptionData(data);


                    var descs = individualCopy.GetComponentsInChildren<GameFeelDescription>(true);

                    //TODO: add Novelty search here, using the treeEditDistance as a measure.
                    //TODO: First generate more individuals than needed, then select them based on novelty.

                    //TODO: add Custom directed fitness, using user input selecting a direction
                    //TODO: Eg. more/less particles, specific colors, intensity / size of things.
                    //TODO: First generate more individuals than needed, then sort them by fitness, select the top 4.
                    //TODO: Then use Novelty to find 4 individuals furthest from the selected 4.

                    if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                        selectedDescriptionIndex >= descs.Length)
                    {
                        //Mutate all the descriptions on the copy.
                        foreach (var desc in descs)
                        {
                            //NOTE: we're ignoring the inactive descriptions here!
                            if (desc.gameObject.activeSelf == false) continue;

                            foreach (var trigger in desc.TriggerList)
                            {
                                foreach (var effectGroup in trigger.EffectGroups)
                                {
                                    //Aggressively mutate copies
                                    EffectGenerator.MutateGroup(effectGroup, 0.05f, 0.4f, 0.4f);
                                }
                            }
                        }
                    }
                    else
                    {
                        //Mutate the selected description and trigger.
                        var description = descs[selectedDescriptionIndex];
                        var trigger = description.TriggerList[selectedTriggerIndex];

                        //TODO: move this Novelty generation out of here, then make the new fitness functions (order by, particle count, color association, sound etc...)
                        //TODO: then select the top 4 with with the best fitness, and select 4 that have the biggest distance to others, make sure they are distinct.
                        var populationToEvaluate = 20;
                        var population = new List<List<GameFeelEffect>>();
                        for (int j = 0; j < populationToEvaluate; j++)
                        {
                            var effects = trigger.EffectGroups[0].GetRecipeCopy();
                            EffectGenerator.MutateGroup(effects, 0.05f, 0.4f, 0.4f);
                            population.Add(effects);
                        }
                        
                        var mostNovel = Novelty(population);
                        
                        trigger.EffectGroups[0].ReplaceEffectsWithRecipe(mostNovel);
                        foreach (var effectGroup in trigger.EffectGroups)
                        {
                            EffectGenerator.MutateGroup(effectGroup, 0.05f, 0.4f, 0.4f);
                        }
                    }
                }

                //Increment before saving, so first generation is gen 1 instead of gen 0. 
                generationIndex++;

                //Gently mutate the selected individual...
                var descriptions = selectedIndividual.GetComponentsInChildren<GameFeelDescription>(true);

                var evolveName = "Evolved_Everything";

                if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
                {
                    var description = descriptions[selectedDescriptionIndex];
                    var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                    evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Evolved.txt";
                }


                //Also save the selected individual!
                foreach (var description in descriptions)
                {
                    var name = "Generation" + generationIndex + " " + description.name + ".txt";

                    GameFeelDescription.SaveToFile(description, name, SessionSavePath);
                }

                //TODO: save a "seperator" text file with the status and name, but don't make the path deeper 2020-10-30 
                //SessionSavePath = Path.Combine(SessionSavePath, evolveName);

                if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                    selectedDescriptionIndex >= descriptions.Length)
                {
                    //Mutate all the descriptions on the copy.
                    foreach (var description in descriptions)
                    {
                        //NOTE: we're ignoring the inactive descriptions here!
                        if (description.gameObject.activeSelf == false) continue;

                        foreach (var trigger in description.TriggerList)
                        {
                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                EffectGenerator.MutateGroup(effectGroup, 0.05f, 0, 0);
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
                        EffectGenerator.MutateGroup(effectGroup, 0.05f, 0, 0);
                    }
                }

                activeEffectSwitcher.Initialize();
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

        public List<GameFeelEffect> Novelty(List<List<GameFeelEffect>> population)
        {
            var distanceMatrix = new float[population.Count * population.Count];
            var maxSum = 0f;
            var indexOfMax = 0;
            for (int i = 0; i < population.Count; i++)
            {
                float[] distances;
                CalculateDistanceRow(i, population, out distances);

                for (int j = i + 1; j < population.Count; j++)
                {
                    var index = i + j * population.Count;

                    distanceMatrix[index] = distances[j];

                    //Also reflect the distance around the diagonal
                    distanceMatrix[j + i * population.Count] = distanceMatrix[index];
                }

                var sum = CalculateDistanceRowSum(i, distanceMatrix, population.Count);
                if (sum > maxSum)
                {
                    maxSum = sum;
                    indexOfMax = i;
                }
            }
            
            //Return the "noblest" or most novel of individuals, eg. the one with the highest distance score across the board.
            return population[indexOfMax];
        }

        private float CalculateDistanceRowSum(int i, float[] distanceMatrix, int rowLength)
        {
            var sum = 0f;
            for (int j = 0; j < rowLength; j++)
            {
                var index = i + j * rowLength;
                sum += distanceMatrix[index];
            }
            return sum;
        }
        private void CalculateDistanceRow(int i, IReadOnlyList<List<GameFeelEffect>> recipesToEvaluate,
            out float[] distanceMatrix)
        {
            distanceMatrix = new float[recipesToEvaluate.Count];
            for (int j = i + 1; j < recipesToEvaluate.Count; j++)
            {
                // if (i == j) continue;

                var nodeSum =
                    1f; // = generatedRandomRecipes[i].EffectsToExecute.Count + generatedRandomRecipes[j].EffectsToExecute.Count;
                var distance = EffectTreeDistance.TreeEditDistance(recipesToEvaluate[i],
                    recipesToEvaluate[j]);
                distanceMatrix[j] = 1f / nodeSum * distance;

                // if (distance > longestDistance)
                // {
                //     longestDistance = distance;
                // }
            }
        }

        public void Randomize(bool useGroupCategories = true)
        {
            var evolveName = "Reset_Everything";

            if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
            {
                var descriptions = InitialVariation.GetComponentsInChildren<GameFeelDescription>();
                var description = descriptions[selectedDescriptionIndex];
                var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Reset.txt";
            }


            //TODO: save a "seperator" text file with the status and name, but don't make the path deeper 2020-10-30 
            //SessionSavePath = Path.Combine(SessionSavePath, evolveName);

            //DO THE RESET!!
            foreach (var individual in activeEffectSwitcher.ABGroups)
            {
                var descs = individual.GetComponentsInChildren<GameFeelDescription>(true);

                if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                    selectedDescriptionIndex >= descs.Length)
                {
                    //Regenerate all the descriptions on the copy.
                    foreach (var desc in descs)
                    {
                        foreach (var trigger in desc.TriggerList)
                        {
                            if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                            {
                                trigger.EffectGroups.Add(new GameFeelEffectGroup()
                                    {GroupName = trigger.GetType().Name + "Group"});
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
                    //Regenerate the selected description and trigger.
                    var description = descs[selectedDescriptionIndex];
                    var trigger = description.TriggerList[selectedTriggerIndex];

                    if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                    {
                        trigger.EffectGroups.Add(new GameFeelEffectGroup()
                            {GroupName = trigger.GetType().Name + "Group"});
                    }

                    foreach (var effectGroup in trigger.EffectGroups)
                    {
                        ReplaceRecipeOnGroup(effectGroup, useGroupCategories);
                    }
                }
            }
        }

        public void Clear()
        {
            var evolveName = "Clear_Everything";

            if (selectedDescriptionIndex != -1 && selectedTriggerIndex != -1)
            {
                var descriptions = InitialVariation.GetComponentsInChildren<GameFeelDescription>();
                var description = descriptions[selectedDescriptionIndex];
                var selectedTrigger = description.TriggerList[selectedTriggerIndex];
                evolveName = description.name + "_" + selectedTrigger.GetType().Name + "_Clear.txt";
            }

            //DO THE CLEAR!!
            foreach (var individual in activeEffectSwitcher.ABGroups)
            {
                var descs = individual.GetComponentsInChildren<GameFeelDescription>(true);

                if (selectedDescriptionIndex <= -1 && selectedTriggerIndex <= -1 ||
                    selectedDescriptionIndex >= descs.Length)
                {
                    //Clear all the descriptions on the copy.
                    foreach (var desc in descs)
                    {
                        foreach (var trigger in desc.TriggerList)
                        {
                            if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                            {
                                trigger.EffectGroups.Add(new GameFeelEffectGroup()
                                    {GroupName = trigger.GetType().Name + "Group"});
                            }

                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                effectGroup.EffectsToExecute.Clear();
                            }
                        }
                    }
                }
                else
                {
                    //Clear the selected description and trigger.
                    var description = descs[selectedDescriptionIndex];
                    var trigger = description.TriggerList[selectedTriggerIndex];

                    if (trigger.EffectGroups == null || trigger.EffectGroups.Count == 0)
                    {
                        trigger.EffectGroups.Add(new GameFeelEffectGroup()
                            {GroupName = trigger.GetType().Name + "Group"});
                    }

                    foreach (var effectGroup in trigger.EffectGroups)
                    {
                        effectGroup.EffectsToExecute.Clear();
                    }
                }
            }
        }
        
        private void ReplaceRecipeOnGroup(GameFeelEffectGroup group, bool useGroupCategories = true)
        {
            var recipe = EffectGenerator.GenerateRecipe(
                useGroupCategories ? group.selectedCategory : EffectGenerator.EffectGeneratorCategories.RANDOM,
                group.selectedIntensity, group.EffectsToExecute);

            group.ReplaceEffectsWithRecipe(recipe);

            EffectGenerator.MutateGroup(group, 0.25f, 0.25f, 0.10f);
        }
    }
}