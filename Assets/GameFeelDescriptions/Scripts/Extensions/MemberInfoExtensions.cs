using System;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace GameFeelDescriptions
{
    public static class MemberInfoExtensions
    {
        public static T [] GetCustomAttributes<T>(this SerializedProperty property) where T: Attribute {
            var path = property.propertyPath.Split ('.');

            var failed = false;
            var type = property.serializedObject.targetObject.GetType();
            FieldInfo field = null;
            for (var i = 0; i < path.Length; i++) {
                field = type.GetField(path[i],BindingFlags.Public 
                    | BindingFlags.NonPublic
                    | BindingFlags.DeclaredOnly 
                    | BindingFlags.FlattenHierarchy
                    | BindingFlags.Instance);
                type = field.FieldType;

                // for non-arrays: .fieldName
                // for arrays: .fieldName.Array.data[0]
                var next = i + 1;
                if (next < path.Length && path[next] == "Array") {
                    i += 2;
                    if (type.IsArray) {
                        type = type.GetElementType();
                    }
                    else {
                        type = type.GetGenericArguments()[0];
                    }
                }
            }

            return field.GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .ToArray();
        }
        
        public static void SetValue(this MemberInfo member, object property, object value)
        {
            if (member.MemberType == MemberTypes.Property)
                ((PropertyInfo)member).SetValue(property, value, null);
            else if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(property, value);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static object GetValue(this MemberInfo member, object property)
        {
            if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(property, null);
            else if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(property);
            else
                throw new Exception("Property must be of type FieldInfo or PropertyInfo");
        }

        public static Type GetType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }
    }
}