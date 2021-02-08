using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using AsyncOperation = UnityEngine.AsyncOperation;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    /// <summary>
    /// This Script assumes it's living in a simple evaluation scene, with a single description and trigger!
    /// </summary>
    public class EffectTreeEvaluation : MonoBehaviour
    {
        private string SavePath = Path.Combine(GameFeelDescription.savePath, "EffectAnalysis");
        
        [ReadOnly]
        public string SceneName;

        private bool reloadingScene;
        
        //TODO: make this dynamic, based on effect length.
        [Header("Evaluation Time Per tree!")]
        public float EvaluationTime = 5f;
        [Header("Total generated trees!")]
        public int EvaluationAmount = 80;
        [Header("Amount of randomly generated trees!")]
        public int RandomAmount = 10;

        [ReadOnly]
        [Header("Time left evaluating this tree!")]
        public float TimeLeft = 5f;
        
        private int evaluationIndex;
        private GameFeelEffectGroup[] generatedGroups; 
            
        private GameFeelDescription description;
        private GameFeelTrigger trigger;
        private GameFeelEffectGroup effectGroup;
        
        [Header("Do static analysis")] 
        public bool staticAnalysis;
        
        [Header("Threading for the static analysis")]
        public bool multiThreaded = true;

        [Range(1, 16)] public int threads = 8;

        public string EstimatedTimeLeft;
        public float progress;
        [Indent(5)] public float progressThread1;
        [Indent(5)] public float progressThread2;
        [Indent(5)] public float progressThread3;
        [Indent(5)] public float progressThread4;
        [Indent(5)] public float progressThread5;
        [Indent(5)] public float progressThread6;
        [Indent(5)] public float progressThread7;
        [Indent(5)] public float progressThread8;

        private float[] distanceMatrix;
        
        // Start is called before the first frame update
        void Start()
        {
            var name = gameObject.scene.name;
            DontDestroyOnLoad(gameObject);
            SceneManager.UnloadSceneAsync(name);
            
            var scene = SceneManager.GetActiveScene();
            SceneName = scene.name;

            var timeStamp = DateTime.Now.ToString("s").Replace(':', '.');
            var path = Path.Combine(SavePath, "Evaluation_" + timeStamp);

            //Make base and current folder
            MakeFolder(path);
            SavePath = path;
            
            //TODO: handle No desc, trigger or group! 2020-12-18
            description = transform.GetComponentInChildren<GameFeelDescription>();
            trigger = description.TriggerList[0];
            effectGroup = trigger.EffectGroups[0];

            //TODO: maybe mutate some more here ! 2020-12-18
            GenerateGroups();
            
            evaluationIndex = -1;

            GameFeelEffectExecutor.Instance.shouldRemoveTweensOnLevelLoad = true;
            
            SetupNextEffect();

            if (staticAnalysis)
            {
                //Evaluates the effects, statically and writes out a distance matrix.
                StartCoroutine(EvaluateMany());    
            }

            //Writes out the structure of the list of effects
            Debug.Log("Writing effect list file");
            FunctionPrintRecipes(generatedGroups, Path.Combine(SavePath,"effectList_" + EvaluationAmount));
        }

        private void GenerateGroups(bool mutateMore = false)
        {
            Debug.Log("Generating recipes");
            generatedGroups = new GameFeelEffectGroup[EvaluationAmount];
            
            //TODO: REMOVE RANDOM SEED!!! 2020-12-18
            Random.InitState(0);
            var category = 0;
            
            //Calculate how many we need of each category.
            var updateCategoryAfter = (EvaluationAmount - RandomAmount) / 7;
            
            //Generate recipes
            for (int i = 0; i < EvaluationAmount; i++)
            {
                //Categories are equally divided. 
                if (category > 0)
                {
                    category = (i - RandomAmount) / updateCategoryAfter;    
                }
                else if (category == 0 && i > RandomAmount)
                {
                    category = 1;
                }

                var intensity = (i % 10) + 1;
                
                generatedGroups[i] = new GameFeelEffectGroup
                {
                    EffectsToExecute =
                        StepThroughModeWindow.GenerateRecipe((StepThroughModeWindow.EffectGeneratorCategories) category,
                            intensity)
                };
                
                if (mutateMore)
                {
                    MutateGroup(generatedGroups[i],0.4f, 0.4f, 0.4f);
                }
            }
        }

        /// <summary>
        /// Replaces effects on effect group!
        /// Make sure the scene is reloaded, before this!
        /// </summary>
        private void SetupNextEffect()
        {
            evaluationIndex++;
            
            effectGroup.ReplaceEffectsWithRecipe(generatedGroups[evaluationIndex].EffectsToExecute);

            var path = Path.Combine(SavePath, "id" + evaluationIndex);
            
            //Create new folder!
            MakeFolder(path);
        }

        // Update is called once per frame
        void Update()
        {
            //Skip updates while reloading;
            if (reloadingScene) return;
            
            TimeLeft -= Time.unscaledDeltaTime;

            //Group switching
            if (TimeLeft <= 0)
            {
                ReloadScene();
                SetupNextEffect();
                TimeLeft = EvaluationTime;
            }
            else
            {
                //TODO: save screen shot here, every x frames!!! 2020-12-18
            }
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
                //var unload = SceneManager.UnloadSceneAsync(SceneName);
                
                //StartCoroutine(LoadAndReattachTriggers(unload));
                StartCoroutine(LoadAndReattachTriggers(null));
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
        
        private IEnumerator EvaluateMany()
        {
            List<BackgroundWorker> backgroundWorkers = new List<BackgroundWorker>();

            if (multiThreaded)
            {
                for (int i = 0; i < threads; i++)
                {
                    BackgroundWorker bg = new BackgroundWorker();

                    bg.DoWork += DoDistanceCalculation;
                    bg.WorkerReportsProgress = true;
                    bg.ProgressChanged += DoProgessUpdate;
                    bg.RunWorkerCompleted += WorkComplete;

                    backgroundWorkers.Add(bg);
                }
            }

            distanceMatrix = new float[EvaluationAmount * EvaluationAmount];

            Debug.Log("Calculating distances");
            yield return null;

            //var longestDistance = 0f;
            //Generate distance matrix
            //NOTE: could be optimized, by reflection.
            var indexAtLastTimeStamp = 0;
            var startTime = Time.unscaledTime;

            if (!multiThreaded) threads = 1;

            for (int i = 0; i < EvaluationAmount;) //i++ happens when you start new bg worker.
            {
                if (i > threads)
                {
                    var now = Time.unscaledTime;
                    var diff = now - startTime;

                    var avg = diff / (i - threads);
                    EstimatedTimeLeft = TimeSpan.FromSeconds((EvaluationAmount - i) * avg).ToString("g");
                }

                if (multiThreaded)
                {
                    for (int j = 0; j < threads; j++)
                    {
                        if (!backgroundWorkers[j].IsBusy)
                        {
                            backgroundWorkers[j].RunWorkerAsync(new TheWorkParameters
                                {workerID = j + 1, index = i++, generatedRandomRecipes = generatedGroups});
                        }
                    }
                }
                else
                {
                    float[] distances;
                    CalculateDistanceRow(i, generatedGroups, out distances);

                    for (int j = i + 1; j < EvaluationAmount; j++)
                    {
                        var index = i + j * EvaluationAmount;

                        distanceMatrix[index] = distances[j];

                        //Also reflect the distance around the diagonal
                        distanceMatrix[j + i * EvaluationAmount] = distanceMatrix[index];
                    }

                    //In the multi threaded version, i is updated when a new task is begun. 
                    i++;

                    //Update row progress.
                    progressThread1 = i / (EvaluationAmount - 1f) * 100;
                }

                yield return null;

                progress = i;
            }

            //All done!
            var elapsed = Time.unscaledTime - startTime;
            if (elapsed > 60)
            {
                EstimatedTimeLeft = "Finished in " + TimeSpan.FromSeconds(elapsed).ToString("g");
            }
            else
            {
                EstimatedTimeLeft = "Finished in " + elapsed.ToString("F1") + "s";
            }

            progress = EvaluationAmount;

            Debug.Log("Writing csv string");
            yield return null;

            var csv = new StringBuilder("");
            for (int i = 0; i < EvaluationAmount; i++)
            {
                for (int j = 0; j < EvaluationAmount; j++)
                {
                    var index = i + j * EvaluationAmount;
                    if (j == EvaluationAmount - 1)
                    {
                        csv.Append(distanceMatrix[index]);
                    }
                    else
                    {
                        csv.Append(distanceMatrix[index]);
                        csv.Append(",");
                    }
                }

                if (i != EvaluationAmount - 1)
                {
                    csv.Append("\n");
                }
            }

            Debug.Log("Writing csv file");
            yield return null;
            
            using (var stream = new StreamWriter(Path.Combine(SavePath, "distanceMatrix_" + EvaluationAmount + ".csv")))
            {
                stream.WriteLine(csv.ToString());
            }
        }
        
        
        //TODO: make proper directory creator!! 2020-12-18
        private void MakeFolder(string path = null)
        {
#if UNITY_EDITOR         
            if (!AssetDatabase.IsValidFolder(SavePath))
            {
                EditorHelpers.CreateFolder(SavePath);
            }
            
            if (!String.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
            {
                EditorHelpers.CreateFolder(path);
            }
#endif
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
        
        private void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (DistanceResult) e.Result;

            for (int j = result.index + 1; j < EvaluationAmount; j++)
            {
                var index = result.index + j * EvaluationAmount;

                distanceMatrix[index] = result.distances[j];

                //Also reflect the distance around the diagonal
                distanceMatrix[j + result.index * EvaluationAmount] = distanceMatrix[index];
            }
        }

        private void DoProgessUpdate(object sender, ProgressChangedEventArgs e)
        {
            switch ((int) e.UserState)
            {
                case 1:
                    progressThread1 = e.ProgressPercentage;
                    break;
                case 2:
                    progressThread2 = e.ProgressPercentage;
                    break;
                case 3:
                    progressThread3 = e.ProgressPercentage;
                    break;
                case 4:
                    progressThread4 = e.ProgressPercentage;
                    break;
                case 5:
                    progressThread5 = e.ProgressPercentage;
                    break;
                case 6:
                    progressThread6 = e.ProgressPercentage;
                    break;
                case 7:
                    progressThread7 = e.ProgressPercentage;
                    break;
                case 8:
                    progressThread8 = e.ProgressPercentage;
                    break;
            }
        }

        class TheWorkParameters
        {
            public int workerID;
            public int index;
            public GameFeelEffectGroup[] generatedRandomRecipes;
        }

        struct DistanceResult
        {
            public int workerID;
            public int index;
            public float[] distances;
        }

        private void DoDistanceCalculation(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;

            var distances = new float[EvaluationAmount];

            var parameters = (TheWorkParameters) e.Argument;
            var result = new DistanceResult {workerID = parameters.workerID, index = parameters.index};

            var highestPercentageReached = 0;

            for (int j = parameters.index + 1; j < EvaluationAmount; j++)
            {
                //NOTE: nodeSum can be used for nomalization!
                var nodeSum =
                    1f; // = generatedRandomRecipes[i].EffectsToExecute.Count + generatedRandomRecipes[j].EffectsToExecute.Count;
                var distance = EffectTreeDistance.TreeEditDistance(
                    parameters.generatedRandomRecipes[parameters.index].EffectsToExecute,
                    parameters.generatedRandomRecipes[j].EffectsToExecute);
                distances[j] = 1f / nodeSum * distance;

                // Report progress as a percentage of the total task.
                int percentComplete = (int) (j / (EvaluationAmount - 1f) * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete, parameters.workerID);
                }
            }

            result.distances = distances;
            e.Result = result;
        }
        
        private void CalculateDistanceRow(int i, GameFeelEffectGroup[] generatedRandomRecipes,
            out float[] distanceMatrix)
        {
            distanceMatrix = new float[EvaluationAmount];
            for (int j = i + 1; j < EvaluationAmount; j++)
            {
                //NOTE: nodeSum can be used for nomalization!
                var nodeSum = 1f; // = generatedRandomRecipes[i].EffectsToExecute.Count + generatedRandomRecipes[j].EffectsToExecute.Count;
                var distance = EffectTreeDistance.TreeEditDistance(generatedRandomRecipes[i].EffectsToExecute,
                    generatedRandomRecipes[j].EffectsToExecute);
                distanceMatrix[j] = 1f / nodeSum * distance;
            }
        }
        
         private void FunctionPrintRecipes(GameFeelEffectGroup[] groups, string filename, bool abstractLabels = false)
        {
            using (var stream = new StreamWriter(filename + ".txt"))
            {
                var str = ""; //"["

                for (int i = 0; i < groups.Length; i++)
                {
                    //output += "\"id\":" + (i+1);
                    str += "[";
                    for (var index = 0; index < groups[i].EffectsToExecute.Count; index++)
                    {
                        var effect = groups[i].EffectsToExecute[index];

                        str += FunctionPrintEffect(effect, abstractLabels);

                        if (index < groups[i].EffectsToExecute.Count - 1)
                        {
                            str += ", ";
                        }
                    }

                    str += "]\n";
                }

                //output += paddingChar+"]\n}";

                stream.WriteLine(str);
            }
        }

        private string FunctionPrintEffect(GameFeelEffect effect, bool abstractLabels = false)
        {
            //element begin
            //var str = "(";

            var str = effect.GetType().Name + "([";

            for (var index = 0; index < effect.ExecuteAfterCompletion.Count; index++)
            {
                var gameFeelEffect = effect.ExecuteAfterCompletion[index];

                str += FunctionPrintEffect(gameFeelEffect, abstractLabels);

                if (index < effect.ExecuteAfterCompletion.Count - 1)
                {
                    str += ", ";
                }
            }

            if (effect is SpawningGameFeelEffect spawner)
            {
                str += "],[";

                for (var index = 0; index < spawner.ExecuteOnOffspring.Count; index++)
                {
                    var gameFeelEffect = spawner.ExecuteOnOffspring[index];

                    str += FunctionPrintEffect(gameFeelEffect, abstractLabels);

                    if (index < spawner.ExecuteOnOffspring.Count - 1)
                    {
                        str += ", ";
                    }
                }
            }

            str += "]";

            //element end
            str += ")";

            return str;
        }
    }
}