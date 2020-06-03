using UnityEngine;
using UnityEngine.Events;

namespace GameFeelDescriptions
{
    public class InvokeUnityEvent : GameFeelEffect
    {
        public InvokeUnityEvent()
        {
            Description = "Invokes a Unity Event, letting you callback or set arbitrary functions/values in your own code";
        }

        public UnityEvent action;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target, bool unscaledTime,
            Vector3? interactionDirection = null)
        {
            var cp = new InvokeUnityEvent
            {
                action = action,
            };
            
            cp.Init(origin, target, unscaledTime, interactionDirection);
            return DeepCopy(cp);
        }

        protected override bool ExecuteTick()
        {
            if (action != null)
            {
                action.Invoke();
            }

            return true;
        }
    }
}