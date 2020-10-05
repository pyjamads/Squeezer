using System;
using System.Collections.Generic;

namespace GameFeelDescriptions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Returns the name of the enum value as a string
        /// </summary>
        /// <param name="enumValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetName<T>(this T enumValue) where T : Enum
        {
            return Enum.GetName(typeof(T), enumValue);
        }
        
        /// <summary>
        /// Returns the name of the enum value as a string
        /// </summary>
        /// <param name="enumValue"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static int GetValueIndex(Type T, string name)
        {
            //If type is not an enum, return -1.
            if (!T.IsSubclassOf(typeof(Enum))) return -1;
            
            var names = Enum.GetNames(T);
            for (int i = 0; i < names.Length; i++)
            {
                if (string.Compare(names[i], name) == 0)
                {
                    return i;
                }
            }

            //We didn't find the name!
            return -1;
        }
        
        public static int GetValueIndex<T>(this T enumValue) where T : Enum
        {
            var values = Enum.GetValues(enumValue.GetType());
            int i = 0;
            foreach (var value in values)
            {
                if (value.Equals(enumValue))
                {
                    return i;
                }

                i++;
            }

            //We didn't find the name! This shouldn't happen!
            return -1;
        }
        
        /// <summary>
        /// Returns a random value from the enum specified!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRandomValue<T>(List<T> except = null) where T : Enum
        {
            if (except != null)
            {
                var val = Enum.GetValues(typeof(T)).GetRandomElement<T>();
                while (except.Contains(val))
                {
                    val = Enum.GetValues(typeof(T)).GetRandomElement<T>();
                }
                return val;
            }
            
            return Enum.GetValues(typeof(T)).GetRandomElement<T>();
        }
    }
}