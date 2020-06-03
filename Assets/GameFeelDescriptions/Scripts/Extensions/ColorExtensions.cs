using UnityEngine;

namespace GameFeelDescriptions
{
    public static class ColorExtensions
    {
        public static Color withR(this Color color, float val)
        {
            return new Color(val, color.g, color.b, color.a);    
        }
    
        public static Color withG(this Color color, float val)
        {
            return new Color(color.r, val, color.b, color.a);    
        }

        public static Color withB(this Color color, float val)
        {
            return new Color(color.r, color.g, val, color.a);    
        }
        
        public static Color withA(this Color color, float a)
        {
            return new Color(color.r, color.g, color.b, a);    
        }
    }
}