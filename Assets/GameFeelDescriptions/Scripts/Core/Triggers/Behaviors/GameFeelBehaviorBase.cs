using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFeelDescriptions
{
    public class GameFeelBehaviorBase : MonoBehaviour
    {
        private const bool showRecipeDebug = false; 
        
        //public bool StepThroughMode;

        [ReadOnly]
        public GameFeelDescription Description;
        [ReadOnly]
        public int TriggerIndex;
        [ReadOnly]
        public GameFeelTriggerType TriggerType;
        
        public List<GameFeelEffectGroup> EffectGroups
        {
            get
            {
                //If the trigger matches us, return the effect groups.
                if(Description.TriggerList.Count >= TriggerIndex && 
                   Description.TriggerList[TriggerIndex].TriggerType == TriggerType)
                {
                    return Description.TriggerList[TriggerIndex].EffectGroups;
                }

                //Remove this script if the check above fails.
                Destroy(this);
                return new List<GameFeelEffectGroup>();
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //Draw line to all the things this Description is attaching to!
            var descPos = transform.position;
            var attachPos = Description.transform.position;

            var halfPos = (descPos - attachPos) * 0.15f;

            Handles.zTest = CompareFunction.LessEqual;
            
            Handles.DrawBezier(descPos, 
                attachPos, 
                descPos - halfPos.y * Vector3.up - halfPos.x * Vector3.right, 
                attachPos + halfPos,
                Color.white,
                Texture2D.whiteTexture, 
                1f);
        }
        #endif
        
        private void OnEnable()
        {
            if (Description != null)
            {
                TriggerType = Description.TriggerList[TriggerIndex].TriggerType;
                Description.attachedTriggers.Add(this);
            }
        }
        
        private void OnDisable()
        {
            if (Description != null)
            {
                Description.attachedTriggers.Remove(this);
            }
        }

        /// <summary>
        /// The actual list of targets used when executing, should be updated on start, object creation and deletion.
        /// </summary>
        public List<List<GameObject>> targets = new List<List<GameObject>>();

        protected void SetupInitialTargets(bool handlesOther)
        {
            targets.Clear();
            
            for (int i = 0; i < EffectGroups.Count; i++)
            {
                if (!handlesOther && EffectGroups[i].AppliesTo == GameFeelTarget.Other)
                {
                    var name = GetType().Name;
                    Debug.Log(name + ": EffectGroup " + i + " targets could not be found!");
                    Debug.LogException(new Exception("GameFeelTarget.Other cannot be used with " + name + "."));
                    continue;
                }

                targets.Add(EffectGroups[i].FindTargets());
                    
                if (EffectGroups[i].AppliesTo == GameFeelTarget.Self)
                {
                    targets[i].Add(gameObject);
                }
            }
        }

#if UNITY_EDITOR
        protected void HandleStepThroughMode(params object[] context)
        {
            //Step Through Mode
            if (Description.StepThroughMode 
                && (EffectGroups.Count == 0 || 
                    EffectGroups.Any(item => item.EffectsToExecute.Count == 0) || 
                    AutomatedDesigner.AutomatedEffectDesigner))
            {
                PauseAndSelectTrigger(null, context);
            }
        }
        
        protected void HandleStepThroughMode(GameFeelEffectGroup effectGroup, params object[] context)
        {
            //Step Through Mode
            if (effectGroup.StepThroughMode && !Description.StepThroughMode)
            {
                PauseAndSelectTrigger(effectGroup, context);
            }
        }

        private void PauseAndSelectTrigger(GameFeelEffectGroup effectGroup, params object[] context)
        {
            if (AutomatedDesigner.AutomatedEffectDesigner)
            {
                //Notify the designer that an event happened, with the description and type.
                AutomatedDesigner.TriggerDesignerEvent(Description, Description.TriggerList[TriggerIndex], effectGroup, context);
                
                //Try firing gameObject ping vs Description.gameObject, to show the user something is happening.
                EditorGUIUtility.PingObject(Description.gameObject);

                return;
            }

            //Skip the StepThroughMode pausing until the editor is un-paused again.
            if (EditorApplication.isPaused) return;

            var typeName = TriggerType.GetName();
            
            //Get possible effects from trigger type
            //var effects = GetGameFeelEffects(TriggerType, context);
            //Generate a recipe of up to 5 effects, from those effects.
            //var recipe = MakeRecipe(effects);

            var activation = "";
            switch (TriggerType)
            {
                case GameFeelTriggerType.OnCollision:
                    activation = "Reacting to "+((OnCollisionTrigger.CollisionActivationType)context[0]).GetName()
                                              +" with "+ ((GameObject) context[1]).name +" ["+((GameObject)context[1]).tag+"]";
                    break;
                case GameFeelTriggerType.OnMove:
                    activation = "Reacting to "+((OnMoveTrigger.MovementActivationType)context[0]).GetName();
                    break;
                case GameFeelTriggerType.OnRotate:
                    break;
                case GameFeelTriggerType.OnStart:
                    break;
                case GameFeelTriggerType.OnDestroy:
                    break;
                case GameFeelTriggerType.OnDisable:
                    break;
                case GameFeelTriggerType.OnCustomEvent:
                    activation = "Reacting to "+context[0]+" from "+((GameObject) context[1]).name+" ["+((GameObject) context[1]).tag+"]";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Debug.Log("StepThroughMode " + typeName + " for " + gameObject.name + " [" + gameObject.tag + "] \n" + activation);
            
            // var dialogResult = EditorUtility.DisplayDialogComplex("StepThroughMode " + typeName + " for [" + tag + "]",
            //     activation+
            //     (showRecipeDebug ?
            //     "Generated recipe:\n" +
            //     String.Join(",\n", recipe.Select(item => item.GetType().Name)) : ""),
            //     showRecipeDebug?"Add generated Recipe!":"Generate feedback effects!", "Manual editing", "Skip " + typeName);
            
            
            var message = typeName + " for "+gameObject.name+" [" + gameObject.tag + "] \n" +  activation;
            EditorApplication.isPaused = true;
            
            //First move position to the last selected position.
            SceneView.lastActiveSceneView.FixNegativeSize();
            SceneView.lastActiveSceneView.LookAt(this.transform.position + Vector3.forward, this.transform.rotation, 2f);
            SceneView.lastActiveSceneView.Focus();
            
            StepThroughModeWindow.ShowWindow(message, this, effectGroup, context);

            
            //Debug.Break();
            
//             var serializedObject = new SerializedObject(Description);
//             //dialogResult values 2: alt, 1: cancel, 0: ok 
//             if (dialogResult != 2)
//             {
//                 //TODO: Maybe context + trigger type to setup trigger automatically?!? (Maybe not) 
//                 
//                 if (dialogResult == 0)
//                 {
//                     var desc = (GameFeelDescription)serializedObject.targetObject;
//                     Undo.RecordObject(desc, "Generate feedback effects!");
//                     
//                     for (int i = 0; i < recipe.Count; i++)
//                     {
//                         if (effectGroup != null)
//                         {
//                             effectGroup.EffectsToExecute.Add(recipe[i]);
//                         }
//                         else
//                         {
//                             if (desc.TriggerList[TriggerIndex].EffectGroups.Count == 0)
//                             {
//                                 desc.TriggerList[TriggerIndex].EffectGroups.Add(new GameFeelEffectGroup
//                                     {EffectsToExecute = new List<GameFeelEffect> {recipe[i]}});
//                             }
//                             else
//                             {
//                                 desc.TriggerList[TriggerIndex].EffectGroups[0].EffectsToExecute.Add(recipe[i]);
//                                 
//                             }
//                         }    
//                     }
//                     serializedObject.ApplyModifiedProperties();
//
//                     if (GameFeelDescription.saveDataForUserStudy)
//                     {
//                         var name = DateTime.Now.ToString("s").Replace(':', '.')+
//                                    "_"+ Description.name + ".txt";
//
//                         GameFeelDescription.SaveToFile(Description, name, GameFeelDescription.userStudyPath);
//                     }
//                     
//                     //Unpause!
//                     EditorApplication.isPaused = false;
//                     return;
//                 }
//                 
//                 EditorGUIUtility.PingObject(Description.gameObject);
//                 Selection.SetActiveObjectWithContext(Description.gameObject, this);
//
//                 //TODO: highlight Effect group if it's not Null.
//                 EditorHelpers.HighlightedTriggerIndex = TriggerIndex;
//                 EditorHelpers.LastHighlightTime = EditorApplication.timeSinceStartup;
//                 
//
//                 //TODO: figure out how to highlight this using the GameFeelDescriptionEditor maybe?
//                 //Highlighter.Highlight("Game Feel Description (Script)", "TriggerList");
// //                if (effectGroup != null)
// //                {
// //                    Debug.Log(gameObject.name + " triggered " + typeName + ": add effects to be executed on " +
// //                              effectGroup.AppliesTo.GetName() + ".");
// //                }
// //                else
// //                {
// //                    Debug.Log(gameObject.name + " " + typeName +
// //                              " triggered, add an effect group, select what it AppliesTo and add effects to be executed.");
// //                }     
//
//                 EditorApplication.isPaused = true;
//                 Debug.Break();
//             }
//             else
//             {
//                 if (effectGroup != null && effectGroup.StepThroughMode)
//                 {
//                     effectGroup.StepThroughMode = false;
//                 }
//
//                 if (Description.StepThroughMode && EffectGroups.Count == 0)
//                 {
//                     switch (TriggerType)
//                     {
//                         case GameFeelTriggerType.OnCollision:
//                         {
//                             var col = (OnCollisionTrigger) Description.TriggerList[TriggerIndex];
//                             var other = (GameObject) context[1];
//
//                             col.ReactTo.Remove("*");
//                             col.ReactTo.Add("!" + other.tag);
//                             serializedObject.ApplyModifiedProperties();
//
//                             break;
//                         }
//                         default:
//                         //For all the other trigger types, just remove them.
//                         {
//                             var sp = serializedObject.FindProperty("TriggerList");
//                             sp.DeleteArrayElementAtIndex(TriggerIndex);
//                             serializedObject.ApplyModifiedProperties();
//
//                             Destroy(this);
//                             break;
//                         }
//                     }
//                 }
//             }
        }

        public static List<Func<GameFeelEffect>> GetGameFeelEffects(GameFeelTriggerType triggerType = GameFeelTriggerType.OnCollision, params object[] context)
            {
                var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
                var gameFeelEffects = new List<Func<GameFeelEffect>>();

                foreach (var type in types)
                {
                    // Skip abstract classes because they should not be instantiated
                    if (type.IsAbstract)
                        continue;
                    
                    //Determine (aka hardcode for first iteration) which effects can be applied to each trigger type.
                    //Later this might be better determined by adding Attributes to each effect
                    //based on the Triggers we'd like them to show up for.
                    switch (triggerType)
                    {
                        case GameFeelTriggerType.OnStart when 
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                            type == typeof(PropertyTweenEffect) ||
                            //type == typeof(FollowOriginEffect) ||
                            type == typeof(RotateTowardsDirectionEffect) ||
                            type == typeof(SquashAndStretchEffect):
                        {
                            break;        
                        }

                        case GameFeelTriggerType.OnDestroy when
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                            type == typeof(PropertyTweenEffect) ||
                            //type == typeof(FollowOriginEffect) ||
                            type == typeof(RotateTowardsDirectionEffect) ||
                            type == typeof(SquashAndStretchEffect):
                        {
                            break;
                        }
                        
                        case GameFeelTriggerType.OnDisable when
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
                            //type == typeof(AudioSynthPlayEffect) ||
                            type == typeof(PropertyTweenEffect) ||
                            //type == typeof(FollowOriginEffect) ||
                            type == typeof(RotateTowardsDirectionEffect) ||
                            type == typeof(SquashAndStretchEffect):
                        {
                            break;
                        }
                        
                        case GameFeelTriggerType.OnMove when
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
                            type == typeof(PropertyTweenEffect):// ||
//                            type == typeof(AudioSynthPlayEffect):// ||
                            //type == typeof(FollowOriginEffect) ||
                            //type == typeof(RotateTowardsDirectionEffect) ||
                            //type == typeof(SquashAndStretchEffect):
                        {
                            break;
                        }
                        case GameFeelTriggerType.OnRotate when
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                            type == typeof(AudioSynthPlayEffect) ||
                            type == typeof(PropertyTweenEffect) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            //type == typeof(FollowOriginEffect) ||
                            type == typeof(RotateTowardsDirectionEffect):// ||
                            //type == typeof(SquashAndStretchEffect):
                        {
                            break;
                        }
                        case GameFeelTriggerType.OnCustomEvent when
//                            type == typeof(DisableEffect) ||
//                            type == typeof(EnableEffect) ||
//                            type == typeof(DestroyEffect) ||
                            //type == typeof(FollowOriginEffect) ||
//                            type == typeof(AudioSynthPlayEffect) ||
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(PropertyTweenEffect) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(RotateTowardsDirectionEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(SquashAndStretchEffect):
                        {
                            break;
                        }
                        case GameFeelTriggerType.OnCollision when
                            type == typeof(DisableEffect) ||
                            type == typeof(EnableEffect) ||
                            type == typeof(DestroyEffect) ||
                            type == typeof(InvokeUnityEvent) ||
                            type == typeof(RotateTowardsDirectionEffect) ||
                            type == typeof(AudioClipModulationEffect) ||
                            type == typeof(AudioClipPlayEffect) ||
                            type == typeof(DisableRendererEffect) ||
                            type == typeof(TriggerCustomEventEffect) ||
                            type == typeof(PropertyTweenEffect):// ||
//                            type == typeof(AudioSynthPlayEffect):
                        {
                            break;
                        }
                        default:
                        {
                            gameFeelEffects.Add(() =>
                            {
                                var instance = (GameFeelEffect) Activator.CreateInstance(type);
                                
                                //Reasonable default values will be set in the effects themselves for now.
                                
                                //TODO: verify that the effect is compatible with the target. 11/05/2020
                                
                                return instance;
                            });
                            break;
                        }
                    }
                }

//                var gameFeelEffectNames = new string[gameFeelEffects.Count];
//                for (var index = 0; index < gameFeelEffects.Count; index++)
//                {
//                    gameFeelEffectNames[index] = gameFeelEffects[index].Name;
//                }
                return gameFeelEffects;
            }

        public static List<GameFeelEffect> MakeRecipe(List<Func<GameFeelEffect>> effectTypes, int maxEffects = 5, float queueProp = 0.2f)
        {
            //Min 1, max = maxEffects
            var elements = 1 + Random.Range(0, maxEffects);
            var list = new List<GameFeelEffect>();
            for (int i = 0; i < elements; i++)
            {
                //TODO: consider making a factory function on GameFeelEffects that returns a randomized instance. 30/04/2020
                //TODO: consider making sure random elements don't repeat, such as multiple destroy / disables. 2020-08-13 
                var instance = effectTypes.GetRandomElement().Invoke();
                
                if (list.Count > 0 &&  Random.value < queueProp)
                {
                    list.GetRandomElement().OnComplete(instance);
                }
                else
                {
                    list.Add(instance);    
                }
            }

            return list;
        }


#endif
    }
}