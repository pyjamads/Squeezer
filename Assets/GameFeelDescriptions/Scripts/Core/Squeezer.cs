using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class Squeezer
    {
        
        #region Effect trigger API first draft  
        //These Triggers can be used directly through gameobject.Trigger(...), effect.Trigger(...) or Squeezer.Trigger(gameobject/effect,...)
        //These triggers do away with the GameFeelTriggerData as a parameter, and can be used with singular effects.
        //This and with two options, either provide a direction and position,
        //or just a direction (which will use the position of the game object instead). TODO: 2021-02-24
        
        //They each return the queued effect, which allows Chaining like:
        //effect.Trigger(...).OnComplete(effect2).OnOffspring(effect3)
        //The above would trigger the effect on a target, then once that effect is done, effect2 will run, and on effect2's offspring effect3 will be executed.
        public static GameFeelEffect Trigger(this GameFeelEffect effect, GameObject target, Vector3 direction)
        {
            return Trigger(target, effect, direction);
        }
        
        public static GameFeelEffect Trigger(this GameFeelEffect effect, GameObject target, Vector3 direction, Vector3 position)
        {
            return Trigger(target, effect, direction, position);
        }
        
        public static GameFeelEffect Trigger(this GameFeelEffect effect, GameObject target)
        {
            return Trigger(target, effect);
        }

        public static GameFeelEffect Trigger(this Behaviour target, GameFeelEffect effect)
        {
            return Trigger(target.gameObject, effect);
        }
        
        public static GameFeelEffect Trigger(this Behaviour target, GameFeelEffect effect, Vector3 direction)
        {
            return Trigger(target.gameObject, effect, direction);
        }
        
        public static GameFeelEffect Trigger(this Behaviour target, GameFeelEffect effect, Vector3 direction, Vector3 position)
        {
            return Trigger(target.gameObject, effect, direction, position);
        }
        
        public static GameFeelEffect Trigger(this GameObject target, GameFeelEffect effect)
        {
            var triggerData = new GameFeelTriggerData();
            return QueueExecution(effect, target, triggerData);
        }
        
        public static GameFeelEffect Trigger(this GameObject target, GameFeelEffect effect, Vector3 direction)
        {
            var triggerData = new DirectionalData (direction);
            return QueueExecution(effect, target, triggerData);
        }
        
        public static GameFeelEffect Trigger(this GameObject target, GameFeelEffect effect, Vector3 direction, Vector3 position)
        {
            var triggerData = new PositionalData (position, direction);
            return QueueExecution(effect, target, triggerData);
        }
        
        public static GameFeelEffect QueueExecution(GameFeelEffect effect, GameObject target, GameFeelTriggerData data)
        {
            //If the effect is disabled, skip it.
            if(effect.Disabled) return null;
        
            //Copy and initialize effect.
            var copy = effect.CopyAndSetElapsed(null, target, data);
        
            //Singleton Effects might return null, to avoid copies.
            if(copy == null) return null;
        
            //Find previously active copy
            var previous = copy.CurrentActiveEffect();

            //Handle overlapping
            var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

            //Queue the effect
            if (queueCopy)
            {
                GameFeelEffectExecutor.Instance.QueueEffect(copy);
                return copy;
            }

            return null;
        }
        
        #endregion
        
        #region Category trigger API first draft
        //These Triggers can be used directly through gameobject.Trigger(...) or Squeezer.Trigger(gameobject,...)
        //The current API is missing some clean generator (without triggering) and caching functionality.
        //It's also a bit clunky to use GameFeelTriggerData as a parameter, it might make sense in the future,
        //but currently the only real options are DirectionalData and PositionalData. TODO: 2021-02-24
        
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
            GameFeelEffectExecutor.Instance.QueueEffect(root);
            
            //Return the delay object, for caching and further manipulation.
            return root;
        }

        public static GameFeelEffect Trigger(StepThroughModeWindow.EffectGeneratorCategories selectedCategory,
            int selectedIntensity, GameFeelTriggerData location, GameObject target = null, float delay = 0,
            bool randomizeDelay = false)
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
            GameFeelEffectExecutor.Instance.QueueEffect(root);

            //Return the delay object, for caching and further manipulation.
            return root;
        }

        //TODO: add generator and caching functionality here! 2021-02-03
        
        #endregion
    }
}