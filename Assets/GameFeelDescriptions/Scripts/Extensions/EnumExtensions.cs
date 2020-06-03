using System;

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
        public static T GetRandomValue<T>() where T : Enum
        {
            return Enum.GetValues(typeof(T)).GetRandomElement<T>();
        }
    }
}