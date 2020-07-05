using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;


namespace GameFeelDescriptions
{
   [Serializable]
   public abstract class GameFeelEffect
   {
      /// <summary>
      /// Returns a Copy of the effect.
      /// </summary>
      /// <param name="origin"></param>
      /// <param name="target"></param>
      /// <param name="unscaledTime"></param>
      /// <param name="interactionDirection"></param>
      /// <returns>A copy of the effect or null if copies are suspended.</returns>
      public abstract GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime, Vector3? interactionDirection = null);

      protected virtual T DeepCopy<T>(T shallow) where T : GameFeelEffect
      {
         shallow.Description = Description;
         shallow.Disabled = Disabled;
         shallow.Delay = Delay;
         shallow.RandomizeDelay = RandomizeDelay;
         shallow.StackingType = StackingType;
         shallow.ExecuteAfterCompletion = new List<GameFeelEffect>(ExecuteAfterCompletion);
         
         //Initialize elapsed
         shallow.SetElapsed();
         
         //Copy is now "deeper"
         return shallow;
      }
      
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
      /// Description of what the effect does and what to use it for.
      /// </summary>
      [TextArea]
      public string Description;

      /// <summary>
      /// Delay in seconds from call to execution.
      /// Can be used to 'jumpstart' the effect, by setting a negative delay (Only works for <see cref="DurationalGameFeelEffect"/>).
      /// </summary>
      [EnableFieldIf("RandomizeDelay", 0, negate = true)]
      [Header("Delay in seconds from call to execution.")]
      [Tooltip("Can be used to 'jumpstart' the effect, by setting a negative delay, ie. starting it between 0 and 1.")]
      public float Delay;

      /// <summary>
      /// Randomize the delay, between 0 and <see cref="Delay"/>.
      /// </summary>
      [HideInInspector]
      [Header("Randomize the delay, between 0 and the specified Delay")]
      public bool RandomizeDelay;
      
      /// <summary>
      /// List of effects to execute after completion of this effect.
      /// </summary>
      /// <returns></returns>
      //[SerializeReferenceButton(readOnly: true)] //NOTE: active SerializeReferenceButton on GameFeelEffects are broken.
//      [HideInInspector]
      [Space]
      [SerializeReference]
      [ShowTypeAttribute]
      [Header("List of effects to execute after this effect finishes.")]
      public List<GameFeelEffect> ExecuteAfterCompletion = new List<GameFeelEffect>();
      
      //TODO: INSTEAD OF THE ABOVE LIST: make a GameFeelEffect ref, that's called ExecuteAfter,
      //TODO: and it'll just calculate the added delay on execution by traversing that tree backwards. 04/07/2020
      
      // Progression tracker
      [Space]
      protected float elapsed;
      
      /// <summary>
      /// Whether the updates will occur on unscaled or regular <see cref="Time.timeScale"/>.
      /// </summary>
      protected bool unscaledTime;

      protected GameObject origin;
      protected internal GameObject target;
      protected Vector3? interactionDirection;

      protected bool firstTick = true;
      protected bool isComplete;

      public virtual float GetRemainingTime()
      {
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
      public (bool queueCopy, bool isOverlapping) HandleEffectOverlapping(GameFeelEffect previous)
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
      /// <param name="interactionDirection"></param>
      public void Init(GameObject origin, GameObject target, bool unscaledTime, Vector3? interactionDirection = null) => (this.origin, this.target, this.unscaledTime, this.interactionDirection) = (origin, target, unscaledTime, interactionDirection);
     
      /// <summary>
      /// Set whether to use scaled or unscaled deltaTime.
      /// </summary>
      /// <param name="unscaledTime"></param>
      public void SetTimeScaling(bool unscaledTime) => this.unscaledTime = unscaledTime;

      /// <summary>
      /// Set the delay before the effect begins.
      /// </summary>
      public void SetElapsed() => elapsed = RandomizeDelay ? -Delay * Random.value : -Delay;
      
      /// <summary>
      /// Update effects based on the elapsed time.
      /// </summary>
      /// <returns></returns>
      public virtual bool Tick()
      {
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
            //Make sure we always get to the end (ie. when elapsed == Duration)
            ExecuteTick(); //Ignore output, in single execution setting
            //Queue effects in the ExecuteAfterCompletion list
            ExecuteComplete();

            return true;
         }
         
         elapsed += unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

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
      /// <returns>True if effect finishes early. False otherwise</returns>
      protected abstract bool ExecuteTick();

      /// <summary>
      /// Executes on effect completion, for most effects the default of executing the follow up effects is enough.
      /// </summary>
      protected virtual void ExecuteComplete()
      {
         isComplete = true;
         foreach (var effect in ExecuteAfterCompletion)
         {
            //If the effect is disabled, skip it.
            if(effect.Disabled) continue;
            
            //Copy and initialize effect.
            var copy = effect.CopyAndSetElapsed(origin, target, unscaledTime, interactionDirection);
            
            //Singleton Effects might return null, to avoid copies.
            if(copy == null) continue;
            
            //Find previously active copy
            var previous = copy.CurrentActiveEffect();

            //Handle overlapping
            var (queueCopy, _) = copy.HandleEffectOverlapping(previous);

            //Queue the effect
            if (queueCopy)
            {
               copy.QueueExecution();   
            }
         }
      }
   }
}
