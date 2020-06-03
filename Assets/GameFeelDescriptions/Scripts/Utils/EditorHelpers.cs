
#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class EditorHelpers
    {
        public static double LastHighlightTime = -5;
        public static int HighlightedTriggerIndex = -1;
        
        public static void CreateFolder(string path, bool recursive = true)
        {
            var pathElements = path.Split(Path.DirectorySeparatorChar);

            var existingPath = pathElements[0];

            if (!AssetDatabase.IsValidFolder(existingPath))
            {
                existingPath = AssetDatabase.CreateFolder("Assets", existingPath);
            }
            
            for (int i = 1; i < pathElements.Length; i++)
            {
                var newPath = Path.Combine(existingPath, pathElements[i]);
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(existingPath,pathElements[i]);
                }

                existingPath = newPath;
            }
        }
        
        public static void SetIconForObject(GameObject gameObject, int idx)
        {
            var largeIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            var icon = largeIcons[idx];
            var egu = typeof(EditorGUIUtility);
            var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            var args = new object[] { gameObject, icon.image };
            var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[]{typeof(UnityEngine.Object), typeof(Texture2D)}, null);
            setIcon.Invoke(null, args);
        }
        private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] array = new GUIContent[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = EditorGUIUtility.IconContent(baseName + (startIndex + i) + postFix);
            }
            return array;
        }
        
        
        
        private static readonly GUIStyle style = new GUIStyle
        {
            normal = new GUIStyleState { background = EditorGUIUtility.whiteTexture }
        };

        public static void DrawColoredRect(Rect position, Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, style);
            GUI.backgroundColor = backgroundColor;
        }
        
        
        public static SerializedProperty ActualFindPropertyRelative(SerializedProperty property, string name)
        {
            var path = property.propertyPath.Substring(0, property.propertyPath.LastIndexOf("."));
            return property.serializedObject.FindProperty(path+"."+name);
        }
    }
}

#endif