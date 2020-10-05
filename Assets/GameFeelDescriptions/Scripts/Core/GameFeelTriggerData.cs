using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public class GameFeelTriggerData
    {
        //TODO: consider adding more data here, such that we can give better error info when effects fail and while debugging.
        //eg. public TriggerInfo trigger, with references to description and triggerID and TriggerType, etc!
        public GameObject Origin;
    }

    [Serializable]
    public class CollisionData : GameFeelTriggerData
    {
        public OnCollisionTrigger.CollisionActivationType ActivationType;
        
        //pass the collision event details.
        public Collision Collision;
        public Collision2D Collision2D;
        
        //For triggers pass the collider we hit.
        public Collider Collider;
        public Collider2D Collider2D;

        public bool wasCollision2D()
        {
            return ActivationType == OnCollisionTrigger.CollisionActivationType.OnCollisionEnter2D ||
                   ActivationType == OnCollisionTrigger.CollisionActivationType.OnCollisionExit2D ||
                   ActivationType == OnCollisionTrigger.CollisionActivationType.OnCollisionStay2D ||
                   ActivationType == OnCollisionTrigger.CollisionActivationType.OnTriggerEnter2D ||
                   ActivationType == OnCollisionTrigger.CollisionActivationType.OnTriggerExit2D ||
                   ActivationType == OnCollisionTrigger.CollisionActivationType.OnTriggerStay2D;
        }
    }
    
    [Serializable]
    public class DirectionalData : GameFeelTriggerData
    {
        public Vector3 DirectionDelta;
    }

    [Serializable]
    public class PositionalData : DirectionalData
    {
        public Vector3 Position;
    }

    [Serializable]
    public class MovementData : PositionalData
    {
        public OnMoveTrigger.MovementActivationType ActivationType;
    }

    [Serializable]
    public class RotationalData : GameFeelTriggerData
    {
        //TODO: consider adding position + radius as well. 2020-09-07
        public Vector3 RotationDelta;
    }
    
    // [Serializable]
    // public class CustomEventData : GameFeelTriggerData
    // {
    //     public string EventName;
    //
    //     /// <summary>
    //     /// This allows us to pass event details for custom detection of Collisions, Movement, as well as Positional or Rotational events.
    //     /// </summary>
    //     public GameFeelTriggerEvent InnerType;
    // }
}