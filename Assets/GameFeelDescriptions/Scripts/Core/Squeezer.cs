using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class Squeezer
    {
        private static List<Func<GameFeelEffect>> constructors;
        
        public static GameFeelEffect Trigger(this GameObject target,
            StepThroughModeWindow.EffectGeneratorCategories selectedCategory, int selectedIntensity,
            GameFeelTriggerData direction, float delay = 0, bool randomizeDelay = false)
        {
            //TODO: maybe make this smarter! 2020-10-30
            var root = new Delay();

            if (constructors == null)
            {
                constructors = GameFeelBehaviorBase<GameFeelTrigger>.GetGameFeelEffects();    
            }

            //Generate recipe for the selected category and intensity.
            var recipe =
                StepThroughModeWindow.GenerateRecipe(selectedCategory,
                    selectedIntensity);
            
            //Add the generated tree to the delay.
            root.OnComplete(recipe);
            
            //Take the handcrafted tree, and mutate it!
            InteractiveEvolution.MutateEffectsRecursive(constructors, root);
            
            root.Delay = delay;
            root.RandomizeDelay = randomizeDelay;

            if (target == null && !(direction is PositionalData))
            {
                Debug.LogError("location has to be PositionalData when target is not provided.");
                return null;
            }
            
            root.Init(null, target, direction);
            root.SetElapsed();
            root.QueueExecution();
            
            //Return the delay object, for caching and further manipulation.
            return root;
        }
        
        public static GameFeelEffect Trigger(StepThroughModeWindow.EffectGeneratorCategories selectedCategory, int selectedIntensity, GameFeelTriggerData location, GameObject target = null, float delay = 0, bool randomizeDelay = false)
        {
            //TODO: maybe make this smarter! 2020-10-30
            var root = new Delay();

            if (constructors == null)
            {
                constructors = GameFeelBehaviorBase<GameFeelTrigger>.GetGameFeelEffects();    
            }

            //Generate recipe for the selected category and intensity.
            var recipe =
                StepThroughModeWindow.GenerateRecipe(selectedCategory,
                    selectedIntensity);
            
            //Add the generated tree to the delay.
            root.OnComplete(recipe);
            
            //Take the handcrafted tree, and mutate it!
            InteractiveEvolution.MutateEffectsRecursive(constructors, root);
            
            root.Delay = delay;
            root.RandomizeDelay = randomizeDelay;

            if (target == null && !(location is PositionalData))
            {
                Debug.LogError("location has to be PositionalData when target is not provided.");
                return null;
            }
            
            root.Init(null, target, location);
            root.SetElapsed();
            root.QueueExecution();
            
            //Return the delay object, for caching and further manipulation.
            return root;
        }
        
        //TODO: add generator and stashing functionality here! 2021-02-03 
    }
}