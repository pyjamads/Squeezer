using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DynamicRangeAttribute : PropertyAttribute
    {
        public object minValue;
        public object maxValue;

        public DynamicRangeAttribute(object minValue, object maxValue)
        {
            if (minValue is Enum)
            {
                this.minValue = (int) minValue;
            }
            else
            {
                this.minValue = minValue;
            }

            if (maxValue is Enum)
            {
                this.maxValue = (int) maxValue;
            }
            else
            {
                this.maxValue = maxValue;
            }
        }
    }
}