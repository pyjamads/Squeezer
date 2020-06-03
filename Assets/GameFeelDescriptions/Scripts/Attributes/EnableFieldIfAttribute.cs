using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class EnableFieldIfAttribute : PropertyAttribute
    {
        public List<EnableFieldIfAttribute> allEnableFieldIfAttributes;
        
        public string property;
        public object value;
        public bool negate;
        
        public EnableFieldIfAttribute(string property, object value, bool negate = false)
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