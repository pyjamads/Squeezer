using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameFeelDescriptions
{
    public abstract class GameFeelBehaviorBase<T> : MonoBehaviour where T : GameFeelTrigger
    {
        private const bool showRecipeDebug = false;
        private static bool disableStepThroughModeHandling = true;
        
        //public bool StepThroughMode;

        //[ReadOnly] //Replace with [DisableAtRuntime] attribute (TBD)
        public GameFeelDescription Description;
        //[ReadOnly] //Replace with [DisableAtRuntime] attribute (TBD)
        public int TriggerIndex;
        //[ReadOnly] //Replace with [DisableAtRuntime] attribute (TBD)
        public GameFeelTriggerType TriggerType;

        public T Trigger
        {
            get
            {
                if (Description.TriggerList.Count >= TriggerIndex && Description.TriggerList[TriggerIndex] is T trigger)
                {
                    return trigger;
                }
                
                //Remove this script if the check above fails.
                Destroy(this);
                Debug.LogError(Description.name+" Trigger List changed could not find ["+typeof(T).Name+"] at index: "+TriggerIndex);
                return null;
            }
        }

        public bool Disabled => Description.TriggerList.Count <= TriggerIndex || Description.TriggerList[TriggerIndex].Disabled;

        public List<GameFeelEffectGroup> EffectGroups
        {
            get
            {
                //If the trigger matches us, return the effect groups.
                if(Description.TriggerList.Count > TriggerIndex &&
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

        protected void SetupInitialTargets()
        {
            targets.Clear();
            
            for (int i = 0; i < EffectGroups.Count; i++)
            {
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
            if (disableStepThroughModeHandling) return;
            
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
            if (disableStepThroughModeHandling) return;
            
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

            switch (this)
            {
                case GameFeelCollisionScript col:
                    activation = "Reacting to "+((OnCollisionTrigger.CollisionActivationType)context[0]).GetName()
                                               +" with "+ ((GameObject) context[1]).name +" ["+((GameObject)context[1]).tag+"]";
                    break;
                case GameFeelMovementScript mov:
                    activation = "Reacting to "+((OnMoveTrigger.MovementActivationType)context[0]).GetName();
                    break;
                case GameFeelRotationScript rot:
                    break;
                case GameFeelStartScript start:
                    break;
                case GameFeelDestroyScript destroy:
                    break;
                case GameFeelDisableScript disable:
                    break;
                case GameFeelCustomEventScript custom:
                    activation = "Reacting to "+context[0]+" from "+((GameObject) context[1]).name+" ["+((GameObject) context[1]).tag+"]";
                    break;
                case StateChangedTriggerScript stateChanged:
                    activation = "Reacting a value change in "+stateChanged.field+" on "+stateChanged.selectedComponent.name + " on "+gameObject.name;
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
            
            
            //StepThroughModeWindow.ShowWindow(message, this, effectGroup, context);
        }

//         public static List<Func<GameFeelEffect>> GetGameFeelEffects(GameFeelTriggerType triggerType = GameFeelTriggerType.OnCollision, params object[] context)
//             {
//                 var types = TypeCache.GetTypesDerivedFrom(typeof(GameFeelEffect));
//                 var gameFeelEffects = new List<Func<GameFeelEffect>>();
//
//                 foreach (var type in types)
//                 {
//                     // Skip abstract classes because they should not be instantiated
//                     if (type.IsAbstract)
//                         continue;
//                     
//                     //Determine (aka hardcode for first iteration) which effects can be applied to each trigger type.
//                     //Later this might be better determined by adding Attributes to each effect
//                     //based on the Triggers we'd like them to show up for.
//                     switch (triggerType)
//                     {
//                         case GameFeelTriggerType.OnStart when 
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
// //                            type == typeof(AudioSynthPlayEffect) ||
//                             type == typeof(PropertyTweenEffect) ||
//                             //type == typeof(FollowOriginEffect) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(SquashAndStretchEffect):
//                         {
//                             break;        
//                         }
//
//                         case GameFeelTriggerType.OnDestroy when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
// //                            type == typeof(AudioSynthPlayEffect) ||
//                             type == typeof(PropertyTweenEffect) ||
//                             //type == typeof(FollowOriginEffect) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(SquashAndStretchEffect):
//                         {
//                             break;
//                         }
//                         
//                         case GameFeelTriggerType.OnDisable when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                             //type == typeof(AudioSynthPlayEffect) ||
//                             type == typeof(PropertyTweenEffect) ||
//                             //type == typeof(FollowOriginEffect) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(SquashAndStretchEffect):
//                         {
//                             break;
//                         }
//                         
//                         case GameFeelTriggerType.OnMove when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
//                             type == typeof(PropertyTweenEffect):// ||
// //                            type == typeof(AudioSynthPlayEffect):// ||
//                             //type == typeof(FollowOriginEffect) ||
//                             //type == typeof(RotateTowardsDirectionEffect) ||
//                             //type == typeof(SquashAndStretchEffect):
//                         {
//                             break;
//                         }
//                         case GameFeelTriggerType.OnRotate when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(RagdollEffect) || //Ragdoll is destructive as a default
// //                            type == typeof(AudioSynthPlayEffect) ||
//                             type == typeof(PropertyTweenEffect) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             //type == typeof(FollowOriginEffect) ||
//                             type == typeof(RotateTowardsDirectionEffect):// ||
//                             //type == typeof(SquashAndStretchEffect):
//                         {
//                             break;
//                         }
//                         case GameFeelTriggerType.OnCustomEvent when
// //                            type == typeof(DisableEffect) ||
// //                            type == typeof(EnableEffect) ||
// //                            type == typeof(DestroyEffect) ||
//                             //type == typeof(FollowOriginEffect) ||
// //                            type == typeof(AudioSynthPlayEffect) ||
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(PropertyTweenEffect) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(SquashAndStretchEffect):
//                         {
//                             break;
//                         }
//                         case GameFeelTriggerType.OnCollision when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(PropertyTweenEffect):// ||
// //                            type == typeof(AudioSynthPlayEffect):
//                         {
//                             break;
//                         }
//                         case GameFeelTriggerType.OnStateChanged when
//                             type == typeof(DisableEffect) ||
//                             type == typeof(EnableEffect) ||
//                             type == typeof(DestroyEffect) ||
//                             type == typeof(InvokeUnityEvent) ||
//                             type == typeof(RotateTowardsDirectionEffect) ||
//                             type == typeof(AudioClipModulationEffect) ||
//                             type == typeof(AudioClipPlayEffect) ||
//                             type == typeof(DisableRendererEffect) ||
//                             type == typeof(TriggerCustomEventEffect) ||
//                             type == typeof(PropertyTweenEffect):// ||
// //                            type == typeof(AudioSynthPlayEffect):
//                         {
//                             break;
//                         }
//                         default:
//                         {
//                             gameFeelEffects.Add(() =>
//                             {
//                                 var instance = (GameFeelEffect) Activator.CreateInstance(type);
//                                 
//                                 //Reasonable default values will be set in the effects themselves for now.
//                                 
//                                 //TODO: verify that the effect is compatible with the target. 11/05/2020
//                                 
//                                 return instance;
//                             });
//                             break;
//                         }
//                     }
//                 }
//
// //                var gameFeelEffectNames = new string[gameFeelEffects.Count];
// //                for (var index = 0; index < gameFeelEffects.Count; index++)
// //                {
// //                    gameFeelEffectNames[index] = gameFeelEffects[index].Name;
// //                }
//                 return gameFeelEffects;
//             }


#endif
    }
}