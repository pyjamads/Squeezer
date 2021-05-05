using System;
using System.Reflection;
using UnityEngine;

namespace GameFeelDescriptions
{
    [Serializable]
    public struct FloatContainer
    {
        public float Float;
    }
        
    [Serializable]
    public struct IntContainer
    {
        public int Int;
    }
    
    //TODO: make one that works on the target type, instead of the given type. 20/05/2020
    public class PropertyTweenEffect : TweenEffect<object>
    {
        public PropertyTweenEffect()
        {
            Description = "Tween a property on a component.";
        }

        //The script
        public Component Component;

        //The field
        [TweenableMemberInfoSelector("Component")]
        public string Field;

        [HideInInspector]
        public new string from;
        [HideInInspector]
        public new string to;
        
        public override GameFeelEffect CopyAndSetElapsed(GameObject origin, GameObject target,
            GameFeelTriggerData triggerData, bool ignoreCooldown = false)
        {
            var cp = new PropertyTweenEffect();
            cp.Init(origin, target, triggerData);
            cp.Component = Component;
            cp.Field = Field;

            cp = DeepCopy(cp, ignoreCooldown);

            cp.@from = @from;
            cp.to = to;
            return cp;
        }

        protected override void SetValue(GameObject target, object value)
        {
            Component.GetType().GetMember(Field)[0].SetValue(Component, value);
        }

        protected override object GetValue(GameObject target)
        {
            return Component.GetType().GetMember(Field)[0].GetValue(Component);
        }
        
        protected override object GetStartValue()
        {
            Type typeOfToAndFrom = null;
            var member = Component.GetType().GetMember(Field)[0];
            var memberType = member.MemberType;

            switch (memberType)
            {
                case MemberTypes.Field:
                    typeOfToAndFrom = ((FieldInfo) member).FieldType;
                    break;
                case MemberTypes.Property:
                    typeOfToAndFrom = ((PropertyInfo) member).PropertyType;
                    break;
            }

            if (typeOfToAndFrom == null)
            {
                return GetValue(target);
            }


            var storedValue = GetValue(target);
            if(!string.IsNullOrEmpty(@from))
            {
                if (typeOfToAndFrom == typeof(float))
                {
                    storedValue = ((FloatContainer) JsonUtility.FromJson(@from, typeof(FloatContainer))).Float;
                }
                else if (typeOfToAndFrom == typeof(int))
                {
                    storedValue = ((IntContainer) JsonUtility.FromJson(@from, typeof(IntContainer))).Int;
                }
                else
                {
                    storedValue = JsonUtility.FromJson(@from, typeOfToAndFrom);
                }
            }
            
            return setFromValue ? storedValue : GetValue(target);
        }

        protected override object GetEndValue()
        {
            Type typeOfToAndFrom = null;
            var member = Component.GetType().GetMember(Field)[0];
            var memberType = member.MemberType;

            switch (memberType)
            {
                case MemberTypes.Field:
                    typeOfToAndFrom = ((FieldInfo) member).FieldType;
                    break;
                case MemberTypes.Property:
                    typeOfToAndFrom = ((PropertyInfo) member).PropertyType;
                    break;
            }

            if (typeOfToAndFrom == null)
            {
                return GetValue(target);
            }
            
            var storedValue = GetValue(target);
            if (!string.IsNullOrEmpty(to))
            {
                if (typeOfToAndFrom == typeof(float))
                {
                    storedValue = ((FloatContainer) JsonUtility.FromJson(to, typeof(FloatContainer))).Float;
                }
                else if (typeOfToAndFrom == typeof(int))
                {
                    storedValue = ((IntContainer) JsonUtility.FromJson(to, typeof(IntContainer))).Int;
                }
                else
                {
                    storedValue = JsonUtility.FromJson(to, typeOfToAndFrom);
                }
            }

            return relative ? GetRelativeValue(GetValue(target), storedValue) : storedValue;
        }

        protected override object GetRelativeValue(object fromValue, object addValue)
        {
            return TweenHelper.GetRelativeValue<object>(fromValue, addValue);
        }

        protected override object GetDifference(object fromValue, object toValue)
        {
            return TweenHelper.GetDifference<object>(fromValue, toValue);
        }

        protected override bool TickTween()
        {
            if(target == null || Component == null || Field == null)
            {
                //disable looping!
                repeat = 0;
                loopType = LoopType.None;
                //signal effect is done!
                return true;
            }
            
            //TODO: Add relative value change, which would remove the singleton need!!! 2021-05-05T17:49:45+02:00
            /*
             var easeFunc = GetEaseFunc();
             if (relative)
             {
                 var progress = elapsed / Duration;
                 var prevProgress = oldElapsed / Duration;
 
                 if (reverse)
                 {
                     progress = 1 - progress;
                     prevProgress = 1 - prevProgress;
                 }
 
                 
                 var prev = diffAmount * easeFunc.Invoke(prevProgress);
                 var current = diffAmount * easeFunc.Invoke(progress);
                 
                 //amount = end - start;
                 //current + (amount * easing(t1)) - (amount * - easing(t0));
                 SetValue(target, GetValue(target) + (reverse ? -1 : 1) * (current - prev));
             }
             else
             {
                 //@from  + (to - @from) * easing(t);
                 SetValue(target, TweenHelper.Interpolate(start, elapsed / Duration, end, easeFunc));    
             }
             */
            
            SetValue(target, TweenHelper.Interpolate<object>(start, elapsed / Duration, end, GetEaseFunc()));

            return false;
        }
    }
}