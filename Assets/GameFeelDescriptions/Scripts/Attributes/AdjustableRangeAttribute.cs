using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class AdjustableRangeAttribute : PropertyAttribute
    {
        public float min;
        public float max;
        
        public bool lockMin;
        public bool lockMax;
        
        public float clampMin = float.NegativeInfinity;
        public float clampMax = float.PositiveInfinity; 

        public AdjustableRangeAttribute(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }
}