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