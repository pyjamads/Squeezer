using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFeelDescriptions
{
    public class OnCustomEventTrigger : GameFeelTrigger
    {
        public enum EventTriggerSources
        {
            Anywhere,
            Self,
            Sources,
        }
        
        public OnCustomEventTrigger() : base(GameFeelTriggerType.OnCustomEvent) { }

        public Component triggeringComponent; 
        
        public string EventName = "*";

        public EventTriggerSources AllowFrom;

        [HideFieldIf("AllowFrom", EventTriggerSources.Sources, negate: true)]
        public string[] Sources;


        public override void Attach(GameFeelDescription description, List<GameObject> attachTo, int triggerIndex)
        {
            
            //TODO: sign up to an event through Reflection? 05/05/2020 
//            if (triggeringComponent != null)
//            {
//                var type = triggeringComponent.GetType();
//                var triggeringObjects = (Component[])GameObject.FindObjectsOfType(type);
//
//                var eventInfo = type.GetEvent(EventName);

//                for (int i = 0; i < triggeringObjects.Length; i++)
//                {
                    //var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, methodInfo); 
                    //GameFeelEffectExecutor.Instance.TriggerCustomEvent(triggeringObjects[i].gameObject, EventName);
                    
//                    DynamicMethod handler =
//                        new DynamicMethod("",
//                            null,
//                            new []{typeof(GameObject), typeof(string), typeof(Vector3?)},
//                            typeof(OnCustomEventTrigger));
                    
//                    var addHandler = eventInfo.GetAddMethod();
//                    object[] addHandlerArgs = { handler, triggeringObjects[i], EventName, null};
//                    
//                    addHandler.Invoke(this, addHandlerArgs);
//                    //eventInfo.AddEventHandler(triggeringObjects[i], handler);    
//                }
//                
//            }

            foreach (GameObject gameObject in attachTo)
            {
                var component = gameObject.AddComponent<GameFeelCustomEventScript>();
                component.TriggerType = TriggerType;
                component.Description = description;
                component.TriggerIndex = triggerIndex;
                component.EventName = EventName;
                component.AllowFrom = AllowFrom;
                component.Sources = Sources;
                description.attachedTriggers.Add(component);
            }
        }
    }
}