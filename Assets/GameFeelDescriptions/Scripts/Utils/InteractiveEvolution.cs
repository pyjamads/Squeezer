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

        public string SceneName;
        
        public ABTest activeEffectSwitcher;

        [Header("How long in seconds, to show each individual setup for")]
        public float displayTimer = 10f;

        public bool autoSwithcing = true;
        
        [Header("Whether to initialize all descriptions with the settings below.")]
        public bool initializeDescriptions;

        public TextAsset SeedDescription;

        [Range(1,10)]
        public int intensity = 1;
        
        public StepThroughModeWindow.EffectGeneratorCategories category;
        
        private int selectedIndex = 0;

        private float lastSwitchTime = 3f;
        private float lastEvolveTime = -20f;

        private static InteractiveEvolution Instance { get; set; }
        private bool reloadingScene; 
        
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

            //Add children to switcher Groups.
            var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
            for (int i = 0; i < descriptions.Length; i++)
            {
                var description = descriptions[i];
                activeEffectSwitcher.ABGroups.Add(description.gameObject);

                if (initializeDescriptions)
                {
                    if (SeedDescription != null)
                    {
                        try
                        {
                            var desc = GameFeelDescription.LoadDescriptionFromJson(SeedDescription.text);
                            description.OverrideDescriptionData(desc, false);
                            
                            foreach (var trigger in description.TriggerList)
                            {
                                foreach (var effectGroup in trigger.EffectGroups)
                                {
                                    //Take the seeded tree, and mutate it!
                                    MutateGroup(effectGroup, 0.25f, 0.25f, 0.10f);
                                }
                            }
                            continue;
                        }
                        catch (Exception)
                        {
                            Debug.LogWarning(SeedDescription.name + " is not a serialized Game Feel Description.");
                        }
                    }
                    
                    foreach (var trigger in description.TriggerList)
                    {
                        foreach (var effectGroup in trigger.EffectGroups)
                        {
                            var recipe = StepThroughModeWindow.GenerateRecipe(category, intensity);

                            effectGroup.ReplaceEffectsWithRecipe(recipe);

                            //Take the handcrafted tree, and mutate it!
                            MutateGroup(effectGroup, 0.25f, 0.25f, 0.10f);
                        }
                    }
                }
            }

            activeEffectSwitcher.Initialize();
        }
        
        private void OnGUI()
        {
            var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
            var current = descriptions[activeEffectSwitcher.currentGroup];
            
            if (Time.unscaledTime - lastEvolveTime < 2f)
            {
                GUI.Label(new Rect(Screen.width / 2f - 50, 100, 100, 100), "Mutation complete!");
            }
            else
            {
                var fullWidth = Screen.width - 80f;
                var buttonWidth = fullWidth / 4f - 10f;

                for (var i = 0; i < descriptions.Length; i++)
                {
                    var selected = i == activeEffectSwitcher.currentGroup;
                 
                    if (selected 
                        ? GUI.Button(new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 40 * (i / 4), buttonWidth, 30),
                            "Select " + descriptions[i].name, EditorStyles.helpBox) //HELP BOX STYLE WHEN SELECTED! 
                        : GUI.Button(new Rect(40 + buttonWidth * (i % 4) + 10 * (i % 4), 40 * (i / 4), buttonWidth, 30),
                            "Select " + descriptions[i].name))
                    {
                        ReloadScene();
                        activeEffectSwitcher.SwitchToGroup(i);
                        autoSwithcing = false;    
                    }
                }
                
                if (GUI.Button(new Rect(40, Screen.height / 2f, 80, 30),
                    "Previous"))
                {
                    ReloadScene();
                    activeEffectSwitcher.SwitchToPrevGroup();
                    lastSwitchTime = Time.unscaledTime;
                }
                
                var timer = " ("+(displayTimer - (Time.unscaledTime - lastSwitchTime)).ToString("F1")+"s)";
                if (GUI.Button(new Rect(Screen.width - 40 - 80, Screen.height / 2f, 80, 30),
                    "Next"+(autoSwithcing ? timer : "")))
                {
                    ReloadScene();
                    activeEffectSwitcher.SwitchToNextGroup();
                    lastSwitchTime = Time.unscaledTime;
                }

                if (GUI.Button(new Rect((Screen.width / 2f) + 5f, Screen.height - 30, 200, 30),
                    "Evolve " + current.name))
                {
                    autoSwithcing = false;
                    EvolveSelected();
                }
                
                if (GUI.Button(new Rect((Screen.width / 2f) - 205f, Screen.height - 30, 200, 30),
                    "Bookmark "+ current.name))
                {
                    var name = "Evolved_" + DateTime.Now.ToString("s").Replace(':', '.') + ".txt";
                    Debug.Log("THE DESCRIPTION IS BOOKMARKED!!\n"+GameFeelDescription.savePath+"/EvolvedDescriptions/"+name);
                    GameFeelDescription.SaveToFile(current, name, Path.Combine(GameFeelDescription.savePath, "EvolvedDescriptions"));
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            //Group switching
            if (autoSwithcing && Time.unscaledTime > lastSwitchTime + displayTimer)
            {
                var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);
                if (activeEffectSwitcher.ABGroups.Count != descriptions.Length)
                {
                    activeEffectSwitcher.ABGroups.Clear();
                    activeEffectSwitcher.ABGroups.AddRange(descriptions.Select(item => item.gameObject));
                }
                
                lastSwitchTime = Time.unscaledTime;
                ReloadScene();
                activeEffectSwitcher.SwitchToNextGroup();
                //TODO: handle show active group text!!
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

            //TODO: handle selection of individual for evolution, copy to all other descriptions, and mutate everyone.
            if (Input.GetKeyDown(KeyCode.Return))
            {
                EvolveSelected();
            }

            //TODO: handle bookmarking, so each individual will show 2 buttons, one for use as next seed, and one for bookmarking... 

            //TODO: loading bookmarks into the evolution as a "seed"
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
                
            if (autoSwithcing)
            {
                //NOTE: Nothing was selected, so we're gonna just mutate all of them...
                Evolve();
            }
            else
            {
                Evolve(activeEffectSwitcher.currentGroup);
            }

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

        public void Evolve(int index = -1)
        {
            ReloadScene();
            
            var descriptions = transform.GetComponentsInChildren<GameFeelDescription>(true);

            if (0 < index && index < activeEffectSwitcher.ABGroups.Count)
            {
                //Copy the selected description, to all other descriptions
                var selectedIndividual = descriptions[index];

                //NOTE: for more advanced evolution, with multiple targets in a scene, maybe only copy over the effect groups or effect lists.
                var json = JsonUtility.ToJson(selectedIndividual, true);

                for (int i = 0; i < descriptions.Length; i++)
                {
                    if (i == index) continue;
                    var desc = descriptions[i];

                    //NOTE: This is sub-optimal, but it creates a "deep" copy of the object.
                    var data = GameFeelDescription.LoadDescriptionFromJson(json);
                    desc.OverrideDescriptionData(data);
                    
                    foreach (var trigger in desc.TriggerList)
                    {
                        foreach (var effectGroup in trigger.EffectGroups)
                        {
                            //Aggressively mutate others
                            MutateGroup(effectGroup, 0.05f, 0.4f, 0.4f);
                        }
                    }
                }
                
                //Gently mutate the selected individual...
                foreach (var trigger in selectedIndividual.TriggerList)
                {
                    foreach (var effectGroup in trigger.EffectGroups)
                    {
                        MutateGroup(effectGroup, 0.05f, 0,0);
                    }
                }
            }
            else
            {
                //Mutate all of them...
                foreach (var description in descriptions)
                {
                    foreach (var trigger in description.TriggerList)
                    {
                        foreach (var effectGroup in trigger.EffectGroups)
                        {
                            //Mutate them a lot.
                            MutateGroup(effectGroup, 0.25f, 0.4f, 0.4f);
                        }
                    }
                }
            }
        }

        public void Randomize()
        {
            // var recipe = StepThroughModeWindow.GenerateRecipe(selectedCategory,
            //     selectedIntensity, group.EffectsToExecute);
            
            // foreach (var description in descriptions)
            // {
            //     foreach (var trigger in description.TriggerList)
            //     {
            //         foreach (var effectGroup in trigger.EffectGroups)
            //         {
            //             MutateGroup(effectGroup, 0.25f, 0.25f, 0.10f);
            //         }
            //     }
            // }
        }


        public static void MutateGroup(GameFeelEffectGroup gameFeelEffectGroup, float mutateAmount = 0.05f,
            float addProbability = 0.05f, float removeProbability = 0.05f)
        {
            var constructors = GameFeelBehaviorBase.GetGameFeelEffects();

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