using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions
{
    public static class TweenHelper
    {
        public static object GetRandomValue<T>(T type, float scale=1f) where T : Type
        {
            if (type == typeof(float))
            {
                return Random.value * scale;
            }
            else if (type == typeof(Vector3))
            {
                return Random.insideUnitSphere * scale;
            }
            else if (type == typeof(Vector2))
            {
                return Random.insideUnitCircle * scale;
            }
            else if (type == typeof(int))
            {
                return Random.Range(0, Mathf.CeilToInt(scale) + 1);
            }
            else if (type == typeof(Vector4))
            {
                var vect = new Vector4(Random.value, Random.value, Random.value, Random.value); 
                return vect * scale;
            }
            else if (type == typeof(Quaternion))
            {
                return new Quaternion(Random.value, Random.value, Random.value, Random.value);
            }
            else if (type == typeof(Color))
            {
                return Random.ColorHSV();
            }
            else if(type == typeof(bool))
            {
                return Random.value > 0.5f;
            }
    
            //Anything else will probably just return null.
            return default;
        }
        
        
        
        public static int GetRelativeValue(int fromValue, int addValue)
        {
            return fromValue + addValue;
        }

        public static int GetDifference(int fromValue, int toValue)
        {
            return toValue - fromValue;
        }
        
        public static int Interpolate(int a, float t, int b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;
            
            return a + Mathf.RoundToInt(((float)(b - a)) * easing(t));
        }

        public static float InverseLerp(int a, int value, int b, bool clamp01 = true)
        {
            if (a != b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01((float) (((double) value - (double) a) / ((double) b - (double) a)));
                }

                return (float) (((double) value - (double) a) / ((double) b - (double) a));
            }
            return 0.0f;
        }
        
        /*
         * /// <summary>
         * ///   <para>Linearly interpolates between a and b by t with no limit to t.</para>
         * /// </summary>
         * /// <param name="a">The start value.</param>
         * /// <param name="b">The end value.</param>
         * /// <param name="t">The interpolation between the two floats.</param>
         * /// <returns>
         * ///   <para>The float value as a result from the linear interpolation.</para>
         * /// </returns>
         * public static float LerpUnclamped(float a, float b, float t)
         * {
         *   return a + (b - a) * t;
         * }
         */
        
        public static float GetRelativeValue(float fromValue, float addValue)
        {
            return fromValue + addValue;
        }

        public static float GetDifference(float fromValue, float toValue)
        {
            return toValue - fromValue;
        }
        
        public static float Interpolate(float a, float t, float b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;

            return Mathf.LerpUnclamped(a, b, easing(t));
            //return a  + (b - a) * easing(t);
        }

        public static float InverseLerp(float a, float value, float b, bool clamp01 = true)
        {
            if ((double)a != (double)b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01((float) (((double) value - (double) a) / ((double) b - (double) a)));
                }

                return (float) (((double) value - (double) a) / ((double) b - (double) a));
            }
            return 0.0f;
        }
        
        
        public static Vector2 GetRelativeValue(Vector2 fromValue, Vector2 addValue)
        {
            return fromValue + addValue;
        }

        public static Vector2 GetDifference(Vector2 fromValue, Vector2 toValue)
        {
            return toValue - fromValue;
        }
        
        public static Vector2 Interpolate(Vector2 a, float t, Vector2 b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;

            return Vector2.LerpUnclamped(a, b, easing(t));
            //return @from  + (to - @from) * easing(t);
        }
        
        public static float InverseLerp(Vector2 a, Vector2 value, Vector2 b, bool clamp01 = true)
        {
            if (a != b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01(Vector2.Dot(value - a, b - a));
                }

                return Vector2.Dot(value - a, b - a);
            }
            return 0.0f;
        }
        
        
        public static Vector3 GetRelativeValue(Vector3 fromValue, Vector3 addValue)
        {
            return fromValue + addValue;
        }

        public static Vector3 GetDifference(Vector3 fromValue, Vector3 toValue)
        {
            return toValue - fromValue;
        }
        
        public static Vector3 Interpolate(Vector3 a, float t, Vector3 b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;

            return Vector3.LerpUnclamped(a, b, easing(t));
            //return @from  + (to - @from) * easing(t);
        }
        
        public static float InverseLerp(Vector3 a, Vector3 value, Vector3 b, bool clamp01 = true)
        {
            if (a != b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01(Vector3.Dot(value - a, b - a));
                }

                return Vector3.Dot(value - a, b - a);
            }
            return 0.0f;
        }
        
        public static Vector4 GetRelativeValue(Vector4 fromValue, Vector4 addValue)
        {
            return fromValue + addValue;
        }

        public static Vector4 GetDifference(Vector4 fromValue, Vector4 toValue)
        {
            return toValue - fromValue;
        }
        
        public static Vector4 Interpolate(Vector4 a, float t, Vector4 b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;
            
            return Vector4.LerpUnclamped(a, b, easing(t));
            //return @from + (to - @from) * easing(t);
        }
        
        public static float InverseLerp(Vector4 a, Vector4 value, Vector4 b, bool clamp01 = true)
        {
            if (a != b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01(Vector4.Dot(value - a, b - a));
                }

                return Vector4.Dot(value - a, b - a);
            }
            return 0.0f;
        }
        
        public static Quaternion GetRelativeValue(Quaternion fromValue, Quaternion addValue)
        {
            return fromValue * addValue;
        }

        public static Quaternion GetDifference(Quaternion fromValue, Quaternion toValue)
        {
            return Quaternion.FromToRotation(fromValue.eulerAngles, toValue.eulerAngles);
        }
        
        public static Quaternion Interpolate(Quaternion a, float t, Quaternion b,
            Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;

            return Quaternion.LerpUnclamped(a, b, easing(t));
        }
        
        public static float InverseLerp(Quaternion a, Quaternion value, Quaternion b, bool clamp01 = true)
        {
            if (a != b)
            {
                var valueDiff = Quaternion.FromToRotation(a.eulerAngles, value.eulerAngles);
                var abDiff = Quaternion.FromToRotation(a.eulerAngles, b.eulerAngles);
                
                if (clamp01)
                {
                    return Mathf.Clamp01(Quaternion.Dot(valueDiff, abDiff));
                }

                return Quaternion.Dot(valueDiff, abDiff);
            }
            return 0.0f;
        }
        
        
        public static Color GetRelativeValue(Color fromValue, Color addValue)
        {
            return fromValue + addValue;
        }

        public static Color GetDifference(Color fromValue, Color toValue)
        {
            return toValue - fromValue;
        }
        
        public static Color Interpolate(Color a, float t, Color b, Func<float, float> easing)
        {
            easing = easing ?? EasingHelper.Linear;

            return Color.LerpUnclamped(a, b, easing(t));
            //return a + (b - a) * easing(t);
        }
        
        public static float InverseLerp(Color a, Color value, Color b, bool clamp01 = true)
        {
            if (a != b)
            {
                if (clamp01)
                {
                    return Mathf.Clamp01(Vector4.Dot(value - a, b - a));
                }

                return Vector4.Dot(value - a, b - a);
            }
            return 0.0f;
        }
        
        
        
        public static object GetRelativeValue<T>(object fromValue, object addValue)
        {
            switch (fromValue)
            {
                case int from:
                    return GetRelativeValue(from, (int)addValue);
                case float from:
                    return GetRelativeValue(from, (float)addValue);
                case Vector2 from:
                    return GetRelativeValue(from, (Vector2)addValue);
                case Vector3 from:
                    return GetRelativeValue(from, (Vector3)addValue);
                case Vector4 from:
                    return GetRelativeValue(from, (Vector4)addValue);
                case Quaternion from:
                    return GetRelativeValue(from, (Quaternion)addValue);
                case Color from:
                    return GetRelativeValue(from, (Color)addValue);
            }
            
            Debug.LogWarning("GetRelativeValue object type ["+fromValue.GetType()+"] not supported!");
            return fromValue;
        }

        public static object GetDifference<T>(object fromValue, object toValue)
        {
            switch (fromValue)
            {
                case int from:
                    return GetDifference(from, (int)toValue);
                case float from:
                    return GetDifference(from, (float)toValue);
                case Vector2 from:
                    return GetDifference(from, (Vector2)toValue);
                case Vector3 from:
                    return GetDifference(from, (Vector3)toValue);
                case Vector4 from:
                    return GetDifference(from, (Vector4)toValue);
                case Quaternion from:
                    return GetDifference(from, (Quaternion)toValue);
                case Color from:
                    return GetDifference(from, (Color)toValue);
            }
            
            Debug.LogWarning("GetDifference object type ["+fromValue.GetType()+"] not supported!");
            return toValue;
        }
        
        public static float InverseLerp<T>(object a, object val, object b, bool clamp01 = true)
        {
            switch (a)
            {
                case int from:
                    return InverseLerp(from, (int)val, (int)b, clamp01);
                case float from:
                    return InverseLerp(from, (float)val, (float)b, clamp01);
                case Vector2 from:
                    return InverseLerp(from, (Vector2)val, (Vector2)b, clamp01);
                case Vector3 from:
                    return InverseLerp(from, (Vector3)val, (Vector3)b, clamp01);
                case Vector4 from:
                    return InverseLerp(from, (Vector4)val, (Vector4)b, clamp01);
                case Quaternion from:
                    return InverseLerp(from, (Quaternion)val, (Quaternion)b, clamp01);
                case Color from:
                    return InverseLerp(from, (Color)val, (Color)b, clamp01);
            }
            
            Debug.LogWarning("InverseLerp object type ["+a.GetType()+"] not supported!");
            return 0f;
        }
        
        public static object Interpolate<T>(object a, float t, object b,  Func<float, float> easing)
        {
            switch (a)
            {
                case int from:
                    return Interpolate(from, t, (int)b, easing);
                case float from:
                    return Interpolate(from, t, (float)b, easing);
                case Vector2 from:
                    return Interpolate(from, t, (Vector2)b, easing);
                case Vector3 from:
                    return Interpolate(from, t, (Vector3)b, easing);
                case Vector4 from:
                    return Interpolate(from, t, (Vector4)b, easing);
                case Quaternion from:
                    return Interpolate(from, t, (Quaternion)b, easing);
                case Color from:
                    return Interpolate(from, t, (Color)b, easing);
            }
            
            Debug.LogWarning("InverseLerp object type ["+a.GetType()+"] not supported!");
            return default;
        }

    }
}
