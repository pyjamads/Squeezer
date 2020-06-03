using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    [InitializeOnLoad]
    public static class StepThroughModeDataHandler
    {
        private const string stepThroughModeDir = "StepThroughMode";
        
        static StepThroughModeDataHandler()
        {
            EditorApplication.playModeStateChanged += ModeChanged;
            
            EditorApplication.quitting += () =>
            {
                //When doing the user study, also save data on application quit. 
                if (GameFeelDescription.saveDataForUserStudy)
                {
                    ModeChanged(PlayModeStateChange.ExitingEditMode);
                }
            };
        }

        static void ModeChanged(PlayModeStateChange playModeState)
        {
            //Save StepThroughMode Descriptions when we're exiting Play Mode!
            if (playModeState == PlayModeStateChange.ExitingPlayMode)
            {
                //OnExitPlayMode set AutomatedGameDesigner=false
                AutomatedDesigner.AutomatedEffectDesigner = false;
            
                //Clean up events from Automatic Effect Designer. 
                AutomatedDesigner.OnDesignerEvent = null;
                
                var descriptions = Object.FindObjectsOfType<GameFeelDescription>();

                for (var index = 0; index < descriptions.Length; index++)
                {
                    var description = descriptions[index];
                    var innerStepThroughMode = false;

                    //Cleanup Triggers with empty effect groups.
                    var emptyTriggers = new List<GameFeelTrigger>();
                    foreach (var trigger in description.TriggerList)
                    {
                        if (trigger.EffectGroups.Count == 0)
                        {
                            emptyTriggers.Add(trigger);
                        }
                        else
                        {
                            foreach (var effectGroup in trigger.EffectGroups)
                            {
                                if (effectGroup.StepThroughMode)
                                {
                                    innerStepThroughMode = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (description.StepThroughMode || innerStepThroughMode)
                    {
                        foreach (var trigger in emptyTriggers)
                        {
                            description.TriggerList.Remove(trigger);
                        }

                        //Make a custom name, based on the index, to avoid issues when the user changes the name of
                        //the Description during Step Through Mode. 
                        var name = index+".SerializedDescription.txt";
                        var path = Path.Combine(GameFeelDescription.savePath, stepThroughModeDir);

                        GameFeelDescription.SaveToFile(description, name, path);
                    }
                }

                //Debug.Log("Exiting Play mode.");
            }
            //Restore the StepThroughMode descriptions when we've entered Edit Mode!
            else if (playModeState == PlayModeStateChange.EnteredEditMode)
            {
                var path = Path.Combine(GameFeelDescription.savePath, stepThroughModeDir);
                //Validate path before trying to load!
                if (!AssetDatabase.IsValidFolder(path))
                {
                    EditorHelpers.CreateFolder(path);
                }
                var (loadedDescriptions, files) = GameFeelDescription.LoadDescriptionsFromDirectory(path);
                
                var descriptions = Object.FindObjectsOfType<GameFeelDescription>();

                for (var index = 0; index < descriptions.Length; index++)
                {
                    var description = descriptions[index];
                    if (description.StepThroughMode)
                    {
                        var name = index+".SerializedDescription.txt";

                        for (var fileIndex = 0; fileIndex < loadedDescriptions.Count; fileIndex++)
                        {
                            var file = files[fileIndex];
                            if (!file.Equals(Path.Combine(path, name))) continue;

                            var loadedDescription = loadedDescriptions[fileIndex];
                            
                            Undo.RecordObject(description, "Updated " + name 
                                + " from StepThroughMode.");
                            
                            description.OverrideDescriptionData(loadedDescription);
                        }
                    }
                    else
                    {
                        //TODO: cleanup this ugly nesting 26/04/2020
                        for (var triggerIndex = 0; triggerIndex < description.TriggerList.Count; triggerIndex++)
                        {
                            var trigger = description.TriggerList[triggerIndex];
                            if (trigger.EffectGroups.Count == 0) continue;

                            for (var groupIndex = 0; groupIndex < trigger.EffectGroups.Count; groupIndex++)
                            {
                                var effectGroup = trigger.EffectGroups[groupIndex];
                                if (effectGroup.StepThroughMode)
                                {
                                    //Replace effectGroup from loaded.
                                    var name = index+".SerializedDescription.txt";                            
                                                                                                              
                                    for (var fileIndex = 0; fileIndex < loadedDescriptions.Count; fileIndex++)
                                    {                                                                         
                                        var file = files[fileIndex];                                     
                                        if (!file.Equals(Path.Combine(path, name))) continue;                 

                                        var loadedDescription = loadedDescriptions[fileIndex];
                                        
                                        Undo.RecordObject(description,
                                            "Updated EffectGroup in " + name + " " 
                                            + trigger.TriggerType.GetName() 
                                            + " from StepThroughMode.");

                                        trigger.EffectGroups[groupIndex] =
                                            loadedDescription.TriggerList[triggerIndex].EffectGroups[groupIndex];
                                    }
                                }
                            }
                        }
                    }
                }
                
                CleanUpFiles(files);
                //Debug.Log("Entered Edit mode.");
            }
            else if (playModeState == PlayModeStateChange.ExitingEditMode)
            {
                //Saving User Study data
                if(GameFeelDescription.saveDataForUserStudy)
                {
                    var descriptions = Object.FindObjectsOfType<GameFeelDescription>();

                    for (var index = 0; index < descriptions.Length; index++)
                    {
                        var description = descriptions[index];

                        //Cleanup Triggers with empty effect groups.
                        var emptyTriggers = new List<GameFeelTrigger>();
                        foreach (var trigger in description.TriggerList)
                        {
                            if (trigger.EffectGroups.Count == 0)
                            {
                                emptyTriggers.Add(trigger);
                            }
                        }

                        //Empty triggers appear if you started with StepThroughMode enabled, but don't contain data.
                        foreach (var trigger in emptyTriggers)
                        {
                            description.TriggerList.Remove(trigger);
                        }

                        //Make a custom name, based on the index, to avoid issues when the user changes the name of
                        //the Description during Step Through Mode. 
                        var name = DateTime.Now.ToString("s").Replace(':', '.')+
                                   "_"+ description.name + ".txt";

                        GameFeelDescription.SaveToFile(description, name, GameFeelDescription.userStudyPath);
                    }
                }
            }
            
            void CleanUpFiles(string[] files)
            {
                for (int i = 0; i < files.Length; i++)
                {
                    Debug.Log("Removing temp file: "+files[i]);
                    File.Delete(files[i]);
                }  
            }
        }
        
        
    }
}