using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


namespace GameFeelDescriptions
{
   [Serializable]
   public abstract class GameFeelEffect
   {
      /// <summary>
      /// Stacking effect types:
      /// <para>- Discard discards this, if there's currently an active copy of this effect running.</para> 
      /// <para>- Replace removes any currently active copy of this effect from execution before inserting itself.</para>
      /// <para>- Add allows simultaneously executing multiple instances of the effect on a single target.</para>
      /// <para>- Queue executes this effect after the currently active copy of this effect has completed.</para>
      /// <para>- Yield allows any effect to replace it, and discards itself if there's currently an active copy of this effect running.</para>
      /// </summary>
      public enum StackEffectType
      {
         [Tooltip("Discard discards this, if there's currently an active copy of this effect running.")]
         Discard,
         [Tooltip("Replace removes any currently active copy of this effect from execution before inserting itself.")]
         Replace,
         [Tooltip("Add allows simultaneously executing multiple instances of the effect on a single target.")]
         Add,
         [Tooltip("Queue executes this effect after the currently active copy of this effect has completed.")]
         Queue,
         [Tooltip("Yield allows any effect to replace it, and discards itself if there's currently an active copy of this effect running.")]
         Yield,
      }
      
      /// <summary>
      /// How to handle multiple invocation of an effect on the same target.
      /// </summary>
      [Space(-5, order = 0), 
       Header ("Stacking decides how to handle multiple invocation", order = 1), 
       Space(-10, order = 2),
       Header("of an effect on the same target.", order = 3)]
      public StackEffectType StackingType;

      /// <summary>
      /// If the effect is disabled, then it just won't be executed.
      /// </summary>
      public bool Disabled;

      /// <summary>
      /// Lock the effect, meaning it won't be removed when the regenerate button is pressed.
      /// </summary>
      [HideInInspector]
      public bool Lock;
      
      /// <summary>
      /// Description of what the effect does and what to use it for.
      /// </summary>
      [TextArea]
      public string Description;

      /// <summary>
      /// Whether the updates will occur on unscaled or regular <see cref="Time.timeScale"/>.
      /// </summary>
      public bool UnscaledTime;

      /// <summary>
      /// Delay in seconds from call to execution.
      /// Can be used to 'jumpstart' the effect, by setting a negative delay (Only works for <see cref="DurationalGameFeelEffect"/>).
      /// </summary>
      [Header("Delay in seconds from call to execution.")]
      [Tooltip("Can be used to 'jumpstart' the effect, by setting a negative delay, ie. starting it between 0 and 1.")]
      public float Delay;

      /// <summary>
      /// Randomize the delay, between 0 and <see cref="Delay"/>.
      /// </summary>
      [Header("Randomize the delay, between 0 and the specified Delay")]
      [Space(-10)]
      public bool RandomizeDelay;
      
      [Header("Cooldown in seconds before another instance of this effect can be executed!")]
      [Min(0f)]
      public float Cooldown;
      
      private float lastCopyTime = float.NegativeInfinity;
      
      
      /// <summary>
      /// List of effects to execute after completion of this effect.
      /// </summary>
      /// <returns></returns>
      //[SerializeReferenceButton(readOnly: true)] //NOTE: active SerializeReferenceButton on GameFeelEffects are broken.
//      [HideInInspector]
      //[Space]
      [SerializeReference]
      [ShowType]
      //[Header("List of effects to execute after this effect finishes.")]
      public List<GameFeelEffect> ExecuteAfterCompletion = new List<GameFeelEffect>();
      
      //TODO: Consider INSTEAD OF THE ABOVE LIST: make a GameFeelEffect ref, that's called ExecuteAfter,
      //TODO: and it'll just calculate the added delay on execution by traversing that tree backwards. 04/07/2020
      //TODO: Alternatively, each effect should have an ID, and a List like the above, it could be called ExecuteAfter,
      //TODO: or WaitFor, and it'll wait for all of them to complete, before executing. 2021-03-13
      
      // Progression tracker
      protected float elapsed;

      protected GameObject origin;
      protected internal GameObject target;
      
      [HideInInspector]
      public GameFeelTriggerData triggerData;

      protected bool firstTick = true;
      public bool isComplete;

      /// <summary>
      /// Returns a Copy of the effect.
      /// </summary>
      /// <param name="origin"></param>
      /// <param name="target"></param>
      /// <param name="triggerData"></param>
      /// <param name="ignoreCooldown"></param>
      /// <returns>A copy of the effect or null if copies are suspended.</returns>
      public abstract GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
         GameFeelTriggerData triggerData, bool ignoreCooldown = false);

      protected virtual T DeepCopy<T>(T shallow, bool ignoreCooldown) where T : GameFeelEffect
      {
         //If the cooldown is not over, disallow the copy.
         if (!ignoreCooldown && Time.unscaledTime < lastCopyTime + Cooldown)
         {
            return null;
         }
         
         shallow.Description = Description;
         shallow.Disabled = Disabled;
         shallow.UnscaledTime = UnscaledTime;
         shallow.Delay = Delay;
         shallow.RandomizeDelay = RandomizeDelay;
         
         //NOTE: we don't actually need to pass this parameter, as the cooldown is handled here, but we might as well. 2020-09-04
         shallow.Cooldown = Cooldown;
         
         shallow.StackingType = StackingType;
         if (ignoreCooldown)
         {
            shallow.ExecuteAfterCompletion = new List<GameFeelEffect>();
         }
         else
         {
            shallow.ExecuteAfterCompletion = new List<GameFeelEffect>(ExecuteAfterCompletion);   
         }

         //Initialize elapsed
         shallow.SetElapsed();
         
         //Set the latest time this effect was copied.
         lastCopyTime = Time.unscaledTime;
         
         //Copy is now complete!
         return shallow;
      }
      
      public virtual void Mutate(float amount = 0.05f)
      {
         if (RandomExtensions.Boolean())
         {
            //Add or remove the amount [-amount, amount] from the delay, but never goes below 0.
            var delayAmount = RandomExtensions.MutationAmount(amount);
            Delay = Mathf.Max(0,Delay + delayAmount); 
         }

         if (RandomExtensions.Boolean(amount))
         {
            //XOR with a amount probability to flip the bool.
            RandomizeDelay = !RandomizeDelay;
         }
         
         //Flip stacking type with prop = amount.
         if (RandomExtensions.Boolean(trueProp: amount))
         {
            //NOTE: Queue and Add stacking types might create un-intuitive behavior, so disabled for mutation. 2020-11-09
            StackingType = EnumExtensions.GetRandomValue(new List<StackEffectType>{StackEffectType.Queue, StackEffectType.Add});   
         }
         
         //NOTE: For spawning effects, this is not super awesome, so we're gonna reduce the chance that it happens. 
         if (RandomExtensions.Boolean(amount))
         {
            var cooldownAmount = RandomExtensions.MutationAmount(amount);
            Cooldown = Mathf.Max(0,Cooldown + cooldownAmount);
         }

         //If called on an already initialized Effect, this makes sure the elapsed is set correctly.
         SetElapsed();
      }

      public virtual float GetRemainingTime(bool includeDelay = false)
      {
         if (includeDelay)
         {
            return Delay;
         }
         
         return -elapsed;
      }

      public virtual bool CompareTo(GameFeelEffect other)
      {
         //Default Comparison, check type and then target.
         return other.GetType() == GetType() && other.target == target;
      }
      
      /// <summary>
      /// Function for handling when one effect overrides another running effect.
      /// </summary>
      /// <returns>Returns true if the copy should be created, and false otherwise.</returns>
      public virtual (bool queueCopy, bool isOverlapping) HandleEffectOverlapping(GameFeelEffect previous)
      {
         //If there's nothing to override, allow the copy to be created. 
         if (previous == null || previous.isComplete) return (true, false);

         //If previous finishes before copy begins, we don't need to do anything. Unless it's Replace.
         //TODO NOTE: Delay will have to change to a GetDelay function, if we remove nested effect lists!
         if (previous.GetRemainingTime() < Delay) return (true, false);

         //When the previous has stacking type yield, we handle it like replace.
         if (previous.StackingType == StackEffectType.Yield)
         {
            previous.StopExecution();
            return (true, true);
         }
         
         //TODO: draw up a figure for how StackingTypes work, and which of previous or the new effect controls this!!
         switch (StackingType)
         {
            case StackEffectType.Yield: //Yield functions like Discard, when adding a new copy.
            case StackEffectType.Discard: //Can be handled here.
               return (false, true);
            
            case StackEffectType.Replace: //Can be handled here.
               previous.StopExecution();
               return (true, true);
            
            case StackEffectType.Add: //Can be handled here.
               //NOTE: singleton effects might need to handle this themselves!
               return (true, true);
            
            case StackEffectType.Queue: //Can be handled here.
               previous.OnComplete(this); //TODO NOTE: OnComplete will have to change if we remove nested effect lists
               return (false, true);
            default:
               throw new ArgumentOutOfRangeException();
         }
      }

      /// <summary>
      /// Initialize origin, target and direction of interaction.
      /// Direction is provided in some cases (OnCollision, OnMove, OnRotate, OnCustomEvent)
      /// </summary>
      /// <param name="origin"></param>
      /// <param name="target"></param>
      /// <param name="eventData1"></param>
      public void Init(GameObject origin, GameObject target, GameFeelTriggerData dataData) => (this.origin, this.target, this.triggerData) = (origin, target, dataData);
      
      /// <summary>
      /// Set the delay before the effect begins.
      /// </summary>
      public void SetElapsed() => elapsed = RandomizeDelay ? -Delay * Random.value : -Delay;
      
      /// <summary>
      /// Update effects based on the elapsed time.
      /// </summary>
      /// <returns></returns>
      public virtual bool Tick(float unscaledDeltaTime)
      {
         //Update elapsed
         elapsed += UnscaledTime ? unscaledDeltaTime : Time.timeScale * unscaledDeltaTime;
         
         //If this is the first Tick, after the delay, run setup
         if (firstTick && elapsed >= 0)
         {
            ExecuteSetup();
            firstTick = false;
         }
         
         //If this is the first Tick after the delay, run tick, then complete
         //negative elapsed, is used for setting delays.
         if (elapsed >= 0)
         {
            //Check effect is complete
            var complete = ExecuteTick(); 

            if (complete)
            {
               //Queue effects in the ExecuteAfterCompletion list
               ExecuteComplete();
               
               return true;
            }
         }

         return false;
      }
      
      /// <summary>
      /// Do any setup needed before the effect is executed.
      /// </summary>
      /// <param name="origin"></param>
      /// <param name="target"></param>
      /// <param name="interactionDirection"></param>
      protected virtual void ExecuteSetup() { /* DO NOTHING */ }
      
      /// <summary>
      /// Executes the effect, on the target. 
      /// </summary>
      /// <returns>True when an effect finishes and false otherwise.</returns>
      protected abstract bool ExecuteTick();

      /// <summary>
      /// Executes on effect completion, for most effects the default of executing the follow up effects is enough.
      /// </summary>
      protected void ExecuteComplete()
      {
         isComplete = true;
         ExecuteCleanUp();
         
         QueueEffects();
      }

      protected void QueueEffects()
      {
         var queuedEffects = new List<GameFeelEffect>();
         
         foreach (var effect in ExecuteAfterCompletion)
         {
            //If the effect is disabled, skip it.
            if(effect.Disabled) continue;
            
            //Copy and initialize effect.
            var copy = effect.CopyAndSetElapsed(origin, target, triggerData);
            
            //Singleton Effects might return null, to avoid copies.
            if(copy == null) continue;
            
            //Find previously active copy
            var previous = copy.CurrentActiveEffect();

            //Handle overlapping
            var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

            //Queue the effect
            if (queueCopy)
            {
               if (copy is WaitForAbove waitForAboveEffect)
               {
                  waitForAboveEffect.WaitFor(queuedEffects.ToList());
               }

               GameFeelEffectExecutor.Instance.QueueEffect(copy);
               
               queuedEffects.Add(copy);
            }
         }
      }

      public virtual void ExecuteCleanUp() { /* CLEAN UP STUFF CREATED IN THE SETUP */ }
      
      
      
      
      //TREE EDIT DISTANCE!
      #region Tree Edit Distance

      // public static float DistanceCost(List<GameFeelEffect> a, List<GameFeelEffect> b)
      // {
      //    //First let's figure out the difference in child count.
      //    float distanceCost = Mathf.Abs(a.Count - b.Count);
      //
      //    var treeCost = new float[a.Count, b.Count];
      //
      //    //Get the replacement costs of each child combination.
      //    for (var i = 0; i < a.Count; i++)
      //    {
      //       var minValue = float.MaxValue;
      //       
      //       for (int j = 0; j < b.Count; j++)
      //       {
      //          treeCost[i,j] = a[i].DistanceCost(b[j]);
      //          if (treeCost[i, j] < minValue)
      //          {
      //             minValue = treeCost[i, j];
      //          }
      //       }
      //       
      //       //Then for each child find the lowest replacementCost (NOTE: can be 0, if they match exactly)
      //       //TODO: this does not take into account, that multiple i's could select a single j. please fix 2020-11-27
      //       distanceCost += minValue;
      //    }
      //
      //    return distanceCost;
      // }
      
      // public float DistanceCost(GameFeelEffect other)
      // {
      //    //We're working under the assumption that the two trees will most likely be different, at least at the leaf level.
      //    
      //    //If the type is the same, get ReplacementCost (this can be 0, if they are equal.)
      //    return ReplacementCost(other) + SubtreeCost(other);
      // }
      
      /// <summary>
      /// The distance between two 
      /// </summary>
      /// <param name="other"></param>
      /// <returns></returns>
      public virtual float ReplacementCost(GameFeelEffect other)
      {
         //TODO: implement cost function in all effects! (probably make it abstract!) 2020-12-01
         //Default replacement cost, implement comparison in effects.
         return 0f;
      }

      public static float ReplacementCost(GameFeelEffect a, GameFeelEffect b)
      {
         if (a == b) return 0f;
         
         if (a != null && b != null && a.GetType() == b.GetType())
         {
            //Custom replacement cost for two effects of the same type,
            //based on a comparison in the effects themselves.
            //Cost range [0f, 1f[
            return a.ReplacementCost(b);
         }

         //Delete, Insert and Replace with another effect type, all costs 1f;
         return 1f;
      }

      // public virtual float SubtreeCost(GameFeelEffect other)
      // {
      //    //First let's figure out the difference in child count.
      //    float distanceCost = Mathf.Abs(other.ExecuteAfterCompletion.Count - ExecuteAfterCompletion.Count);
      //
      //    var treeCost = new float[ExecuteAfterCompletion.Count, other.ExecuteAfterCompletion.Count];
      //
      //    //Get the distance costs of each child combination.
      //    for (var i = 0; i < ExecuteAfterCompletion.Count; i++)
      //    {
      //       var minValue = float.MaxValue;
      //       
      //       for (int j = 0; j < other.ExecuteAfterCompletion.Count; j++)
      //       {
      //          treeCost[i,j] = ExecuteAfterCompletion[i].DistanceCost(other.ExecuteAfterCompletion[j]);
      //          if (treeCost[i, j] < minValue)
      //          {
      //             minValue = treeCost[i, j];
      //          }
      //       }
      //       
      //       //Then for each child find the lowest replacementCost (NOTE: can be 0, if they match exactly)
      //       //TODO: this does not take into account, that multiple i's could select a single j. please fix 2020-11-27
      //       distanceCost += minValue;
      //    }
      //
      //    return distanceCost;
      // }
      
      #endregion
   }
}
