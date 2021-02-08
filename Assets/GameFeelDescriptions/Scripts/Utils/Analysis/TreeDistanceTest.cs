using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Threading;
using GameFeelDescriptions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public class TreeDistanceTest : MonoBehaviour
    {
        [Tooltip("N Equally divided between categories (M in each), first M are Random, second M are Pickup, etc...")]
        public int RecipesToEvaluate = 80;



        [TextArea] public string output;

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
            Debug.Log("Testing Effect Tree Distance!");

            var ShortListA = new List<GameFeelEffect>();
            ShortListA.Add(new Delay());

            var ShortListB = new List<GameFeelEffect>();
            ShortListB.Add(new Delay());

            var stopwatch = Stopwatch.StartNew();

            var dist = EffectTreeDistance.TreeEditDistance(ShortListA, ShortListB);

            stopwatch.Stop();

            Debug.Log("Comparing two similar effects! Time = " + stopwatch.Elapsed.TotalSeconds + "s, Distance = " +
                      dist);



            var ShortListC = new List<GameFeelEffect>();
            ShortListC.Add(new Selector());

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(ShortListA, ShortListC, true);

            stopwatch.Stop();

            Debug.Log("Comparing two different effects! Time = " + stopwatch.Elapsed.TotalSeconds + "s, Distance = " +
                      dist);

            var ShortListD = new List<GameFeelEffect>();
            var sel = new Selector();
            ShortListD.Add(sel);
            ShortListD.Add(new Looper());
            ShortListD.Add(new ScaleEffect());
            ShortListD.Add(new ParticlePuffEffect());

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(ShortListA, ShortListD, true);

            stopwatch.Stop();

            Debug.Log("Comparing (a) to (b,c,d,e(f,g,h,i)) ! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);

            //EXAMPLE FROM PAPER!
            // x¯ = a(b(c, d), e)
            // y¯ = f(g)

            var x_ = new List<GameFeelEffect>();
            var a = new Delay();
            var b = new Selector();
            var c = new ScaleEffect();
            var d = new RotateEffect();
            var e = new BlinkEffect();

            a.OnComplete(b);
            b.OnComplete(c);
            b.OnComplete(d);
            a.OnComplete(e);
            x_.Add(a);

            var y_ = new List<GameFeelEffect>();
            var f = new AudioSynthPlayEffect();
            var g = new MaterialColorChangeEffect();
            f.OnComplete(g);
            y_.Add(f);

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(x_, y_, true);

            stopwatch.Stop();

            Debug.Log("Comparing x¯ = a(b(c, d), e) with y¯ = f(g)! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);

            var effectListA = new List<GameFeelEffect>();

            effectListA.Add(new AudioSynthPlayEffect());

            var delayA = new Delay();
            delayA.OnComplete(new ScaleEffect());
            var shakeA = new ShakeEffect();
            shakeA.OnComplete(new DestroyEffect());
            delayA.OnComplete(shakeA);
            effectListA.Add(delayA);

            var looperA = new Looper();
            looperA.OnComplete(new BlinkEffect());
            looperA.OnComplete(new RotateEffect());
            effectListA.Add(looperA);



            // delayA(Scale, ShakeA(Destroy)),looperA(Blink, Rotate),AudioSynth

            var effectListB = new List<GameFeelEffect>();

            effectListB.Add(new AudioSynthPlayEffect());

            var delayB = new Delay();
            delayB.OnComplete(new ScaleEffect());
            var shakeB = new ShakeEffect();
            shakeB.OnComplete(new DestroyEffect());
            delayB.OnComplete(shakeB);
            effectListB.Add(delayB);

            var looperB = new Looper();
            looperB.OnComplete(new BlinkEffect());
            looperB.OnComplete(new RotateEffect());
            effectListB.Add(looperB);



            // delayB(Scale, ShakeB(Destroy)),looperB(Blink, Rotate),AudioSynth

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectListA, effectListB);

            stopwatch.Stop();

            Debug.Log("Comparing two versions of the same tree! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);


            var effectListC = new List<GameFeelEffect>();

            var looperC = new Looper();
            looperC.OnComplete(new RotateEffect());
            //looperC.OnComplete(new BlinkEffect());
            effectListC.Add(looperC);

            var looperC1 = new Looper();
            looperC1.OnComplete(new RotateEffect());
            looperC1.OnComplete(new BlinkEffect());
            effectListC.Add(looperC1);

            var delayC = new Delay();
            delayC.OnComplete(new ScaleEffect());
            var shakeC = new ShakeEffect();
            shakeC.OnComplete(new DestroyEffect());
            delayC.OnComplete(shakeC);
            effectListC.Add(delayC);

            effectListC.Add(new AudioSynthPlayEffect());


            var effectListC2 = new List<GameFeelEffect>();

            effectListC2.Add(new AudioSynthPlayEffect());

            var looperC2 = new Looper();
            looperC2.OnComplete(new BlinkEffect());
            //looperC2.OnComplete(new RotateEffect());
            effectListC2.Add(looperC2);

            var delayC2 = new Delay();
            delayC2.OnComplete(new ScaleEffect());
            var shakeC2 = new ShakeEffect();
            shakeC2.OnComplete(new DestroyEffect());
            delayC2.OnComplete(shakeC2);
            effectListC2.Add(delayC2);

            var looperC2_2 = new Looper();
            //looperC2_2.OnComplete(new BlinkEffect());
            looperC2_2.OnComplete(new RotateEffect());
            effectListC2.Add(looperC2_2);

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectListC, effectListC2, true);

            stopwatch.Stop();

            Debug.Log("Comparing two reordered versions with a single changed leaf! Time = " +
                      stopwatch.Elapsed.TotalSeconds + "s, Distance = " + dist);

            var effectListD = new List<GameFeelEffect>();
            var delayD = new Delay();
            delayD.OnComplete(new BlinkEffect());
            effectListD.Add(delayD);

            var looperD = new Looper();
            looperD.OnComplete(new ScaleEffect());
            var shakeD = new ShakeEffect();
            shakeD.OnComplete(new DestroyEffect());
            shakeD.OnComplete(new AudioSynthPlayEffect());
            looperD.OnComplete(shakeD);
            effectListD.Add(looperD);

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectListA, effectListD, true);

            stopwatch.Stop();

            Debug.Log("Comparing two versions with a multiple changes! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);


            var effectGroup1 = new GameFeelEffectGroup();
            var effectGroup2 = new GameFeelEffectGroup();

            effectGroup1.EffectsToExecute =
                StepThroughModeWindow.GenerateRecipe(StepThroughModeWindow.EffectGeneratorCategories.EXPLODE, 1);
            effectGroup2.EffectsToExecute =
                StepThroughModeWindow.GenerateRecipe(StepThroughModeWindow.EffectGeneratorCategories.EXPLODE, 10);

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectGroup1.EffectsToExecute, effectGroup2.EffectsToExecute);

            stopwatch.Stop();

            Debug.Log("Comparing high and low intensity explosions! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);

            InteractiveEvolution.MutateGroup(effectGroup1, 0.4f, 0.25f, 0.25f);
            InteractiveEvolution.MutateGroup(effectGroup2, 0.4f, 0.25f, 0.25f);

            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectGroup1.EffectsToExecute, effectGroup2.EffectsToExecute);

            stopwatch.Stop();

            Debug.Log("Comparing the same two after heavy mutation! Time = " + stopwatch.Elapsed.TotalSeconds +
                      "s, Distance = " + dist);


            var effectGroup3 = new GameFeelEffectGroup();
            effectGroup3.EffectsToExecute =
                StepThroughModeWindow.GenerateRecipe(StepThroughModeWindow.EffectGeneratorCategories.RANDOM, 6);
            InteractiveEvolution.MutateGroup(effectGroup3, 0.4f, 0.25f, 0.25f);


            stopwatch = Stopwatch.StartNew();

            dist = EffectTreeDistance.TreeEditDistance(effectGroup3.EffectsToExecute, effectGroup2.EffectsToExecute);

            stopwatch.Stop();

            Debug.Log("Comparing Explosion to Random! Time = " + stopwatch.Elapsed.TotalSeconds + "s, Distance = " +
                      dist);



            StartCoroutine(EvaluateMany());
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

            distanceMatrix = new float[RecipesToEvaluate * RecipesToEvaluate];

            var generatedRandomRecipes = new GameFeelEffectGroup[RecipesToEvaluate];

            Debug.Log("Generating recipes");
            yield return null;

            Random.InitState(0);
            var category = 0;
            var updateCategoryAfter = RecipesToEvaluate / 8;
            //Generate recipes
            for (int i = 0; i < RecipesToEvaluate; i++)
            {
                //Categories are equally divided. 
                category = i / updateCategoryAfter;
                generatedRandomRecipes[i] = new GameFeelEffectGroup
                {
                    EffectsToExecute =
                        StepThroughModeWindow.GenerateRecipe((StepThroughModeWindow.EffectGeneratorCategories) category,
                            (i % 10) + 1)
                };
            }

            Debug.Log("Calculating distances");
            yield return null;

            //var longestDistance = 0f;
            //Generate distance matrix
            //NOTE: could be optimized, by reflection.
            var indexAtLastTimeStamp = 0;
            var startTime = Time.unscaledTime;

            if (!multiThreaded) threads = 1;

            for (int i = 0; i < RecipesToEvaluate;) //i++ happens when you start new bg worker.
            {
                if (i > threads)
                {
                    var now = Time.unscaledTime;
                    var diff = now - startTime;

                    var avg = diff / (i - threads);
                    EstimatedTimeLeft = TimeSpan.FromSeconds((RecipesToEvaluate - i) * avg).ToString("g");
                }

                if (multiThreaded)
                {
                    for (int j = 0; j < threads; j++)
                    {
                        if (!backgroundWorkers[j].IsBusy)
                        {
                            backgroundWorkers[j].RunWorkerAsync(new TheWorkParameters
                                {workerID = j + 1, index = i++, generatedRandomRecipes = generatedRandomRecipes});
                        }
                    }
                }
                else
                {
                    float[] distances;
                    CalculateDistanceRow(i, generatedRandomRecipes, out distances);

                    for (int j = i + 1; j < RecipesToEvaluate; j++)
                    {
                        var index = i + j * RecipesToEvaluate;

                        distanceMatrix[index] = distances[j];

                        //Also reflect the distance around the diagonal
                        distanceMatrix[j + i * RecipesToEvaluate] = distanceMatrix[index];
                    }

                    //In the multi threaded version, i is updated when a new task is begun. 
                    i++;

                    //Update row progress.
                    progressThread1 = i / (RecipesToEvaluate - 1f) * 100;
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

            progress = RecipesToEvaluate;

            Debug.Log("Writing csv string");
            yield return null;

            var csv = new StringBuilder("");
            for (int i = 0; i < RecipesToEvaluate; i++)
            {
                for (int j = 0; j < RecipesToEvaluate; j++)
                {
                    var index = i + j * RecipesToEvaluate;
                    if (j == RecipesToEvaluate - 1)
                    {
                        csv.Append(distanceMatrix[index]);
                    }
                    else
                    {
                        csv.Append(distanceMatrix[index]);
                        csv.Append(",");
                    }
                }

                if (i != RecipesToEvaluate - 1)
                {
                    csv.Append("\n");
                }
            }

            output = csv.ToString();

            Debug.Log("Writing csv file");
            yield return null;

            using (var stream =
                new StreamWriter("distanceMatrix_" + RecipesToEvaluate + "_N" + updateCategoryAfter + ".csv"))
            {
                stream.WriteLine(output);
            }

            Debug.Log("Writing json file");
            yield return null;
            PrettyPrintJsonRecipes(generatedRandomRecipes,
                "distanceMatrix_" + RecipesToEvaluate + "_N" + updateCategoryAfter);
            FunctionPrintRecipes(generatedRandomRecipes,
                "distanceMatrix_" + RecipesToEvaluate + "_N" + updateCategoryAfter);

            //Debug.Log("Longest distance found: " + longestDistance);
        }

        private const char paddingChar = ' ';

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

        private void PrettyPrintJsonRecipes(GameFeelEffectGroup[] groups, string filename,
            bool inlineFirstElement = true)
        {
            using (var stream =
                new StreamWriter(filename + ".json"))
            {
                var json = "{\n" + paddingChar + "\"recipes\": [\n";
                var increment = 2;

                for (int i = 0; i < groups.Length; i++)
                {
                    json += string.Empty.PadRight(increment, paddingChar);

                    if (inlineFirstElement)
                    {
                        json += "{";
                    }
                    else
                    {
                        json += "{\n";
                        increment++;
                        json += string.Empty.PadRight(increment, paddingChar);
                    }

                    json += "\"id\": " + (i + 1) + ",\n";
                    json += string.Empty.PadRight(increment, paddingChar);
                    json += "\"EffectsToExecute\": [\n";
                    increment++;
                    for (var index = 0; index < groups[i].EffectsToExecute.Count; index++)
                    {
                        var effect = groups[i].EffectsToExecute[index];

                        //Array element begin
                        json += string.Empty.PadRight(increment, paddingChar);
                        if (inlineFirstElement)
                        {
                            json += "{";
                        }
                        else
                        {
                            json += "{\n";
                        }

                        json += PrettyPrintJsonEffect(effect, increment + 1, inlineFirstElement);

                        //Array element end
                        json += string.Empty.PadRight(increment, paddingChar);
                        json += "}";

                        if (index < groups[i].EffectsToExecute.Count - 1)
                        {
                            json += ",";
                        }

                        json += "\n";
                    }

                    if (!inlineFirstElement)
                    {
                        increment--;
                        json += string.Empty.PadRight(increment, paddingChar);
                        json += "]\n";
                    }

                    increment--;
                    json += string.Empty.PadRight(increment, paddingChar);
                    if (i < groups.Length - 1)
                    {
                        if (!inlineFirstElement)
                        {
                            json += "},\n";
                        }
                        else
                        {
                            json += "]},\n";
                        }

                    }
                    else
                    {
                        if (!inlineFirstElement)
                        {
                            json += "}\n";
                        }
                        else
                        {
                            json += "]}\n";
                        }
                    }
                }

                json += paddingChar + "]\n}";

                stream.WriteLine(json);
            }
        }

        private string PrettyPrintJsonEffect(GameFeelEffect effect, int increment, bool inlineFirstElement = true)
        {
            var json = "";
            if (!inlineFirstElement)
            {
                json += string.Empty.PadRight(increment, paddingChar);
            }

            json += "\"" + effect.GetType().Name + "\": {\n";

            increment++;
            json += string.Empty.PadRight(increment, paddingChar);
            json += "\"ExecuteAfterCompletion\": [\n";

            increment++;
            for (var index = 0; index < effect.ExecuteAfterCompletion.Count; index++)
            {
                var gameFeelEffect = effect.ExecuteAfterCompletion[index];

                //Array element begin
                json += string.Empty.PadRight(increment, paddingChar);
                if (inlineFirstElement)
                {
                    json += "{";
                }
                else
                {
                    json += "{\n";
                }

                json += PrettyPrintJsonEffect(gameFeelEffect, increment + 1, inlineFirstElement);

                //Array element end
                json += string.Empty.PadRight(increment, paddingChar);
                json += "}";

                if (index < effect.ExecuteAfterCompletion.Count - 1)
                {
                    json += ",\n";
                }
                else
                {
                    json += "\n";
                }
            }

            increment--;
            json += string.Empty.PadRight(increment, paddingChar);

            if (effect is SpawningGameFeelEffect spawner)
            {
                json += "],\n";

                json += string.Empty.PadRight(increment, paddingChar);
                json += "\"ExecuteOnOffspring\": [\n";

                increment++;
                for (var index = 0; index < spawner.ExecuteOnOffspring.Count; index++)
                {
                    var gameFeelEffect = spawner.ExecuteOnOffspring[index];
                    //Array element begin
                    json += string.Empty.PadRight(increment, paddingChar);
                    if (inlineFirstElement)
                    {
                        json += "{";
                    }
                    else
                    {
                        json += "{\n";
                    }

                    json += PrettyPrintJsonEffect(gameFeelEffect, increment + 1, inlineFirstElement);

                    //Array element end
                    json += string.Empty.PadRight(increment, paddingChar);
                    json += "}";

                    if (index < spawner.ExecuteOnOffspring.Count - 1)
                    {
                        json += ",\n";
                    }
                    else
                    {
                        json += "\n";
                    }
                }

                increment--;
                json += string.Empty.PadRight(increment, paddingChar);
            }

            json += "]\n";

            //Object end
            increment--;
            json += string.Empty.PadRight(increment, paddingChar);
            json += "}\n";

            return json;
        }

        private void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            var result = (DistanceResult) e.Result;

            for (int j = result.index + 1; j < RecipesToEvaluate; j++)
            {
                var index = result.index + j * RecipesToEvaluate;

                distanceMatrix[index] = result.distances[j];

                //Also reflect the distance around the diagonal
                distanceMatrix[j + result.index * RecipesToEvaluate] = distanceMatrix[index];
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

            var distances = new float[RecipesToEvaluate];

            var parameters = (TheWorkParameters) e.Argument;
            var result = new DistanceResult {workerID = parameters.workerID, index = parameters.index};

            var highestPercentageReached = 0;

            for (int j = parameters.index + 1; j < RecipesToEvaluate; j++)
            {
                // if (i == j) continue;

                var nodeSum =
                    1f; // = generatedRandomRecipes[i].EffectsToExecute.Count + generatedRandomRecipes[j].EffectsToExecute.Count;
                var distance = EffectTreeDistance.TreeEditDistance(
                    parameters.generatedRandomRecipes[parameters.index].EffectsToExecute,
                    parameters.generatedRandomRecipes[j].EffectsToExecute);
                distances[j] = 1f / nodeSum * distance;

                // Report progress as a percentage of the total task.
                int percentComplete = (int) (j / (RecipesToEvaluate - 1f) * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete, parameters.workerID);
                }
            }

            result.distances = distances;
            e.Result = result;
        }


        //Burst and jobs for multithreading, but it requires different data structures, or at least struct wrappers.
        //https://github.com/stella3d/job-system-cookbook/blob/master/Assets/Scripts/ImageProcessing/BurstRGBJobs.cs
        //https://github.com/stella3d/job-system-cookbook/blob/master/Assets/Scripts/PointCloudProcessing.cs
        //https://github.com/korzen/Unity3D-JobsSystemAndBurstSamples/blob/master/Assets/JobsAndBurst/Scripts/JobsBasics.cs
        // struct PowParallelForJob : IJobParallelFor
        // {
        //     [GameFeelDescriptions.ReadOnly]
        //     public NativeArray<float> dataIn;
        //
        //     [WriteOnly]
        //     public NativeArray<float> dataOut;
        //
        //     public void Execute(int i)
        //     {
        //         float res = Mathf.Pow(dataIn[i], 2.0f);
        //         dataOut[i] = res;
        //     }
        // }


        // private void ParallelFor()
        // {
        //     var powJob = new PowParallelForJob
        //     {
        //         dataIn = dataA,
        //         dataOut = dataB
        //     };
        //     
        //     JobHandle jobHndl = powJob.Schedule(dataSize, 64);
        //
        //     jobHndl.Complete();
        // }
        //
        // private struct GameFeelEffectGroupStruct
        // {
        //     public List<GameFeelEffect> effects;
        // }

        //Single threaded row calculation
        private void CalculateDistanceRow(int i, GameFeelEffectGroup[] generatedRandomRecipes,
            out float[] distanceMatrix)
        {
            distanceMatrix = new float[RecipesToEvaluate];
            for (int j = i + 1; j < RecipesToEvaluate; j++)
            {
                // if (i == j) continue;

                var nodeSum =
                    1f; // = generatedRandomRecipes[i].EffectsToExecute.Count + generatedRandomRecipes[j].EffectsToExecute.Count;
                var distance = EffectTreeDistance.TreeEditDistance(generatedRandomRecipes[i].EffectsToExecute,
                    generatedRandomRecipes[j].EffectsToExecute);
                distanceMatrix[j] = 1f / nodeSum * distance;

                // if (distance > longestDistance)
                // {
                //     longestDistance = distance;
                // }
            }
        }
    }

}
