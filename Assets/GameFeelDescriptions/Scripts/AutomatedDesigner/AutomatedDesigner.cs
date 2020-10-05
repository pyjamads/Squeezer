using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameFeelDescriptions
{
    public static class AutomatedDesigner
    {

        public static bool AutomatedEffectDesigner;
        public static Action<GameFeelDescription, GameFeelTrigger, GameFeelEffectGroup, object[]> OnDesignerEvent;

        public static void TriggerDesignerEvent(GameFeelDescription description, GameFeelTrigger trigger,
            GameFeelEffectGroup effectGroup, params object[] context)
            => OnDesignerEvent?.Invoke(description, trigger, effectGroup, context);


//        //TODO: implement this 25/05/2020
//        [MenuItem("GameFeelDescriptions/EnterPlayModeWithAutomaticDesign")]
//        static void EnterPlayModeAndStartTheAutomatedEffectDesigner()
//        {
//            //TODO: Consider a semi automated designer (mixed-initiative) as a first step,
//            //TODO: basically suggest 3 options, and user selects one or cancels. 26/04/2020
//            
//            AutomatedEffectDesigner = true;
//            EditorApplication.isPaused = true;
//            EditorApplication.EnterPlaymode();
//
//            if (!EditorUtility.DisplayDialog("Limit Automatic Designer Access?",
//                "Only allow automation on Descriptions/EffectGroups with StepThroughMode enabled.",
//                "Yes", "No"))
//            {
//                //Set all Descriptions to StepThroughMode    
//            }
//            
//            //Add handling of AutomataOn[Start, Collision, Destroy, etc...].
//            OnDesignerEvent += DesignerEventHandler;
//            
//            /* Experience your game getting "wilder" and "wilder" as you go,
//               pause playing to edit some effects, 
//               and exit play mode when you want to stop the automated effect designer. */
//        }

        //Subscribe to those events with the AutomatedDesigner.
        private static void DesignerEventHandler(GameFeelDescription description, GameFeelTrigger trigger,
            GameFeelEffectGroup effectGroup, params object[] context)
        {
            //When an event happens, choose to add an effect, or an effectGroup(with a new AppliesTo)+effect.
            if (allEffects == null)
            {
                allEffects = FindAllDerivedTypes<GameFeelEffect>();
            }

            //TODO: select effect based on attributes, add effect, or effect group with new target. 27/04/2020
            //TODO: finish AED

            //Find or create

            switch (trigger.TriggerType)
            {
                case GameFeelTriggerType.OnCollision:
                    break;
                case GameFeelTriggerType.OnMove:
                    break;
                case GameFeelTriggerType.OnRotate:
                    break;
                case GameFeelTriggerType.OnStart:
                    break;
                case GameFeelTriggerType.OnDestroy:
                    break;
                case GameFeelTriggerType.OnDisable:
                    break;
                case GameFeelTriggerType.OnCustomEvent:
                    break;
                case GameFeelTriggerType.OnStateChanged:
                    break;
            }


        }

        //TODO: Cleanup the 3 different ways of getting these, there's two in GameFeelDescriptionEditor, and one here!
        private static List<Type> allEffects;

        public static List<Type> FindAllDerivedTypes<T>()
        {
            return FindAllDerivedTypes<T>(Assembly.GetAssembly(typeof(T)));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            var derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                ).ToList();

        }
    }
}