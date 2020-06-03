using UnityEngine;

namespace GameFeelDescriptions
{
    public static class Helpers
    {
        /// <summary>
        /// Check if a tag exists, remember to store the result, because calling this often is bad!
        /// </summary>
        /// <param name="tag">The tag to check.</param>
        /// <returns></returns>
        public static bool DoesTagExist(string tag)
        {
            try
            {
                GameObject.FindGameObjectWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}