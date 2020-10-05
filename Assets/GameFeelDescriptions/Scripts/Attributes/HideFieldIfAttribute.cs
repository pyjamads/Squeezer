using System;
using JetBrains.Annotations;
using UnityEngine;

namespace GameFeelDescriptions
{
    //TODO: look at https://github.com/Deadcows/MyBox ConditionalField, and more... 2020-10-01
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class HideFieldIfAttribute : PropertyAttribute
    {   
        public string property;
        public object value;
        public bool negate;

        public HideFieldIfAttribute(string property, object value, bool negate = false)
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