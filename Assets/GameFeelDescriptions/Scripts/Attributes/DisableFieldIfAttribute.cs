using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class DisableFieldIfAttribute : PropertyAttribute
    {   
        public string property;
        public object value;
        public bool negate;

        public DisableFieldIfAttribute(string property, object value, bool negate = false)
        {
            if (value is Enum)
            {
                this.value = (int) value;
            }
            else
            {
                this.value = value;
            }    
            
            this.property = property;
            this.negate = negate;
        }
    }
}