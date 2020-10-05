
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    

    public enum GameFeelTriggerType
    {
        OnCollision, //When two objects collide
        OnMove,      //When an object moves
        OnRotate,    //When an object rotates
        //OnCreate,    //When an object is created
        OnStart,     //When an object is added to the scene
        OnDestroy,   //When an object is destroyed
        //OnEnable? //Should there be an OnEnable, when OnDisable exists?
        OnDisable,   //When an object is disabled
        OnCustomEvent,//When some object sends this event, usually triggered by player eg. OnShoot, OnUsePowerUp etc.
        OnStateChanged, //When a specified value on a component on the attached object is changed.
        /* More might be necessary. */
        //OnLevelLoad, //When a level is loaded?
    }

    //TODO: Maybe remove the enum, and use reflection instead, like GameFeelEffects 06/02/2020

    [Serializable]
    public abstract class GameFeelTrigger
    {
        protected GameFeelTrigger(GameFeelTriggerType type)
        {
            TriggerType = type;
        }
 
        [ReadOnly]
        public GameFeelTriggerType TriggerType;
        
        /// <summary>
        /// Whether the whole group of effects is disabled.
        /// </summary>
        [Header("Controls if the effects are executed, and adhere to current timeScale.")]
        public bool Disabled;
        
        /* 
        //TODO: Remove effect groups 2: move stepthroughmode and disabled up to triggers, and rename it to pause on trigger or something.
       
        [Header("This mode allows you to add effects as events happen while playing.")]
        public bool StepThroughMode;

        /// <summary>
        /// The list of effects to execute
        /// </summary>
        [SerializeReference] 
        [ShowTypeAttribute]
        [Space]
        public List<GameFeelEffect> EffectsToExecute = new List<GameFeelEffect>();
        */
        

        public List<GameFeelEffectGroup> EffectGroups = new List<GameFeelEffectGroup>();
        
        public abstract void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex);

        public static GameFeelTrigger CreateTrigger(GameFeelTriggerType type)
        {
            switch (type)
            {
                case GameFeelTriggerType.OnCollision:
                    return new OnCollisionTrigger();
                case GameFeelTriggerType.OnMove:
                    return new OnMoveTrigger();
                case GameFeelTriggerType.OnRotate:
                    return new OnRotateTrigger();
                case GameFeelTriggerType.OnStart:
                    return new OnStartTrigger();
                case GameFeelTriggerType.OnDestroy:
                    return new OnDestroyTrigger();
                case GameFeelTriggerType.OnDisable:
                    return new OnDisableTrigger();
                case GameFeelTriggerType.OnCustomEvent:
                    return new OnCustomEventTrigger();
                case GameFeelTriggerType.OnStateChanged:
                    return new OnStateChangeTrigger();
//                case GameFeelTriggerType.OnCreate:
//                    //TODO: implement this last one...
//                    //TODO: If OnCreate seems difficult through standard unity, make a "register change" for a tag/object/etc
//                    //TODO, that the user calls through code, or with an attribute, or using a UnityEvent!
//                    //For now, the DynamicReattachRate + previouslyAttached is a slow but functional hack!
//                    return null;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}



