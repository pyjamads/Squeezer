using System;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    /// <summary>
    ///   <para>Use this PropertyAttribute to add some spacing in the Inspector.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public class IndentAttribute : PropertyAttribute
    {
        /// <summary>
        ///   <para>The spacing in pixels.</para>
        /// </summary>
        public readonly float width;

        public IndentAttribute()
        {
            width = EditorGUIUtility.standardVerticalSpacing;
        }

        /// <summary>
        ///   <para>Use this DecoratorDrawer to add some indentation in the Inspector.</para>
        /// </summary>
        /// <param name="width">The indentation in pixels.</param>
        public IndentAttribute(float width)
        {
            this.width = width;
        }
    }
    
    [CustomPropertyDrawer(typeof(IndentAttribute))]
    public class IndentAttributeDrawer : PropertyDrawer
    {
        IndentAttribute indent => ((IndentAttribute)attribute);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var newPosition = new Rect(position.x+indent.width, position.y, position.width -indent.width, position.height);
            EditorGUI.PropertyField(newPosition, property, label);
        }
    }
}