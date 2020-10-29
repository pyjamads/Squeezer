using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class GameFeelMovementScript : GameFeelBehaviorBase<OnMoveTrigger>
    {
        // public OnMoveTrigger.MovementActivationType type;

        private Vector3 lastPosition;

        private bool isMoving;
        private Vector3 lastDirection;
        
        private void Start()
        {
            lastPosition = transform.position;
            lastDirection = Vector3.zero;
            
            SetupInitialTargets();
        }

        private void Update()
        {
            if (Disabled) return;
            
            var justStoppedMoving = false;
            var justChangedDirection = false;
            var justBeganMoving = false;
            
            //Update last known direction if we're moving.
            if (lastPosition.CompareTo(transform.position, .0001f) == false)
            {
                var direction = transform.position - lastPosition;
                direction.Normalize();
                if (direction.CompareTo(lastDirection, .0001f) == false)
                {
                    justChangedDirection = true;
                    lastDirection = direction;
                }
            }

            //Check for begin/end movement
            if (isMoving == false && lastPosition.CompareTo(transform.position, .001f) == false)
            {
                isMoving = true;
                //signal the change
                justBeganMoving = true;
            }
            else if (isMoving && lastPosition.CompareTo(transform.position, .001f))
            {
                isMoving = false;
                //signal the change
                justStoppedMoving = true;
            }
            
            //React according to movement type.
            switch (Trigger.type)
            {
                case OnMoveTrigger.MovementActivationType.OnBeginMoving:
                    if (justBeganMoving)
                    {
                        ExecuteEffectGroups(Trigger.type);
                    }
                    break;
                case OnMoveTrigger.MovementActivationType.WhileMoving:
                    if (isMoving)
                    {
                        ExecuteEffectGroups(Trigger.type);
                    }
                    break;
                case OnMoveTrigger.MovementActivationType.OnDirectionChange:
                    if (justChangedDirection)
                    {
                        ExecuteEffectGroups(Trigger.type);
                    }
                    break;
                case OnMoveTrigger.MovementActivationType.OnStopMoving:
                    if (justStoppedMoving)
                    {
                        ExecuteEffectGroups(Trigger.type);
                    }
                    break;
                case OnMoveTrigger.MovementActivationType.WhileNotMoving:
                    if (!isMoving)
                    {
                        ExecuteEffectGroups(Trigger.type);
                    }
                    break;
                case OnMoveTrigger.MovementActivationType.OnAnyStateChange:
                    if (justBeganMoving)
                    {
                        ExecuteEffectGroups(OnMoveTrigger.MovementActivationType.OnDirectionChange);
                    }
                    else if (justChangedDirection)
                    {
                        ExecuteEffectGroups(OnMoveTrigger.MovementActivationType.OnDirectionChange);
                    }
                    else if(justStoppedMoving)
                    {
                        ExecuteEffectGroups(OnMoveTrigger.MovementActivationType.OnStopMoving);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            //Update last known position if we've moved enough.
            if (lastPosition.CompareTo(transform.position, .001f) == false)
            {
                //update last known position
                lastPosition = transform.position;
            }
        }

        public void ExecuteEffectGroups(OnMoveTrigger.MovementActivationType activationType)
        {
            if (Disabled) return;
            
            if (EffectGroups.Count != targets.Count)
            {
                SetupInitialTargets();
            }

            //TODO: Get rigidbody and figure out "relativeVelocity" on our own here, if there is one?
            var direction = lastDirection.normalized * (transform.position - lastPosition).magnitude;
            
#if UNITY_EDITOR
            if (Description.StepThroughMode)
            {
                /* Trigger StepThroughMode Popup! */
                HandleStepThroughMode(activationType, direction);
            }
#endif
            
            for (int i = 0; i < EffectGroups.Count; i++)
            {
#if UNITY_EDITOR
                //Handle StepThroughMode for this specific group, if enabled.
                HandleStepThroughMode(EffectGroups[i], activationType, direction);
#endif
                
                EffectGroups[i].InitializeAndQueueEffects(gameObject, targets[i], new MovementData(transform.position, direction, Trigger.type){Origin = gameObject});    
            }
        }
    }
}