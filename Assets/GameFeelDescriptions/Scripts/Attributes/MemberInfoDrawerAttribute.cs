using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [AttributeUsage(AttributeTargets.Field)]
    public class MemberInfoDrawerAttribute : PropertyAttribute
    {
        public string getTypeRelative;
        public Type type;
        public string member;
        
        /// <summary>
        /// Lookup a the type of a relative property.
        /// </summary>
        /// <param name="property"></param>
        public MemberInfoDrawerAttribute(string getTypeRelative, string member)
        {
            this.getTypeRelative = getTypeRelative;
            this.member = member;
        }
        
        /// <summary>
        /// Define a type to be used 
        /// </summary>
        /// <param name="type"></param>
        public MemberInfoDrawerAttribute(Type type, string member)
        {
            this.type = type;
            this.member = member;
        }
    }
}