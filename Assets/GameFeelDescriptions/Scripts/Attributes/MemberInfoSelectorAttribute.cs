using System;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MemberInfoSelectorAttribute : PropertyAttribute
    {
        public string getTypeRelative;
        public Type type;

        /// <summary>
        /// Lookup a the type of a relative property.
        /// </summary>
        /// <param name="property"></param>
        public MemberInfoSelectorAttribute(string getTypeRelative)
        {
            this.getTypeRelative = getTypeRelative;
        }
        
        /// <summary>
        /// Define a type to be used 
        /// </summary>
        /// <param name="type"></param>
        public MemberInfoSelectorAttribute(Type type)
        {
            this.type = type;
        }
    }
}