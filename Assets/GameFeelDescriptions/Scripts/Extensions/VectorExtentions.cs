using System.Xml;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class VectorExtentions
    {
        public static Vector2 withX(this Vector2 vec, float x)
        {
            return new Vector2(x, vec.y);
        }

        public static Vector2 withY(this Vector2 vec, float y)
        {
            return new Vector2(vec.x, y);
        }

        public static Vector3 AsVector3(this Vector2 vec)
        {
            return new Vector3(vec.x, vec.y);
        }

        public static bool CompareTo(this Vector2 self, Vector2 other, float epsilon = Vector2.kEpsilon)
        {
            return Vector2.Distance(self, other) > epsilon;
        }


        public static Vector3 withX(this Vector3 vec, float x)
        {
            return new Vector3(x, vec.y, vec.z);
        }

        public static Vector3 withY(this Vector3 vec, float y)
        {
            return new Vector3(vec.x, y, vec.z);
        }

        public static Vector3 withZ(this Vector3 vec, float z)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        public static Vector3 multiplyElements(this Vector3 vec, Vector3 scaleBy)
        {
            return new Vector3(vec.x * scaleBy.x, vec.y * scaleBy.y, vec.z * scaleBy.z);
        }
        
        public static float getAvg(this Vector3 vec)
        {
            return (Mathf.Abs(vec.x) + Mathf.Abs(vec.y) + Mathf.Abs(vec.z)) / 3f;
        }

        public static float Dot(this Vector3 lhs, Vector3 rhs)
        {
            return Vector3.Dot(lhs, rhs);
        }
        
        public static Vector3 Cross(this Vector3 lhs, Vector3 rhs)
        {
            return Vector3.Cross(lhs, rhs);
        }

        public static Vector2 AsVector2(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.y);
        }

        public static bool CompareTo(this Vector3 self, Vector3 other, float epsilon = Vector3.kEpsilon)
        {
            return Vector3.Distance(self, other) < epsilon;
        }
    }
}