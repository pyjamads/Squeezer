using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{

    [CustomPropertyDrawer(typeof(PropertyTweenEffect))]
    public class PropertyTweenEffectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DrawPropertyTweenEffect(property);
        }

        public static void DrawPropertyTweenEffect(SerializedProperty property)
        {
//GET THE object!
            var propertyTweenEffect = FindPropertyTweenEffectObject(property);

            var iter = property.GetEnumerator();
            int counter = 0;
            if (propertyTweenEffect.Component == null)
            {
                while (iter.MoveNext() && counter < 11)
                {
                    counter++;
                }

                //Just draw Component
                EditorGUILayout.PropertyField((SerializedProperty) iter.Current);
                propertyTweenEffect.Field = null;
                return;
            }

            if (string.IsNullOrEmpty(propertyTweenEffect.Field))
            {
                while (iter.MoveNext() && counter < 11)
                {
                    counter++;
                }

                //Just draw Component
                EditorGUILayout.PropertyField((SerializedProperty) iter.Current);
                iter.MoveNext();
                //and Field
                EditorGUILayout.PropertyField((SerializedProperty) iter.Current);
                return;
            }

            Type typeOfToAndFrom = null;
            var member = propertyTweenEffect.Component.GetType().GetMember(propertyTweenEffect.Field)[0];
            var memberType = member.MemberType;

            switch (memberType)
            {
                case MemberTypes.Field:
                    typeOfToAndFrom = ((FieldInfo) member).FieldType;
                    break;
                case MemberTypes.Property:
                    typeOfToAndFrom = ((PropertyInfo) member).PropertyType;
                    break;
            }


            while (iter.MoveNext())
            {
                var prop = (SerializedProperty) iter.Current;
                //prop.NextVisible(false);
                if (counter > 10)
                {
                    EditorGUI.BeginChangeCheck();
                }

                EditorGUILayout.PropertyField(prop);

                if (counter > 10)
                {
                    if (EditorGUI.EndChangeCheck())
                    {
                        propertyTweenEffect.@from = null;
                        propertyTweenEffect.to = null;
                    }
                }

                //TODO: Draw To and From with the correct fields!!
                if (counter == 7 && propertyTweenEffect.setFromValue)
                {
                    //Insert From field!
                    DrawFieldBasedOnType(typeOfToAndFrom, propertyTweenEffect, true,
                        member.GetValue(propertyTweenEffect.Component));
                    property.serializedObject.ApplyModifiedProperties();
                }
                else if (counter == 8)
                {
                    //Insert To field!
                    DrawFieldBasedOnType(typeOfToAndFrom, propertyTweenEffect, false,
                        member.GetValue(propertyTweenEffect.Component));
                    property.serializedObject.ApplyModifiedProperties();
                }

                counter++;
            }
        }


        private static void DrawFieldBasedOnType(Type type, PropertyTweenEffect propertyTweenEffect, bool drawFrom, object currentValue)
        {
            if (drawFrom)
            {
                const string label = "From";
                if (type == typeof(float))
                {
                    var floatContainer = new FloatContainer{ Float = (float)currentValue };
                    if (propertyTweenEffect.@from != null)
                    {
                        floatContainer = (FloatContainer) JsonUtility.FromJson(propertyTweenEffect.@from, typeof(FloatContainer));
                    }
                    
                    propertyTweenEffect.@from = JsonUtility.ToJson(new FloatContainer{ Float = EditorGUILayout.FloatField(label, floatContainer.Float)});    
                }
                else if (type == typeof(int))
                {
                    var intContainer = new IntContainer{ Int = (int)currentValue };
                    if (propertyTweenEffect.@from != null)
                    {
                        intContainer = (IntContainer) JsonUtility.FromJson(propertyTweenEffect.@from, typeof(IntContainer));
                    }
                    
                    propertyTweenEffect.@from = JsonUtility.ToJson(new IntContainer{ Int = EditorGUILayout.IntField(label, intContainer.Int)});    
                }
                else if (type == typeof(Vector2))
                {
                    propertyTweenEffect.@from = JsonUtility.ToJson(EditorGUILayout.Vector2Field(label, (Vector2) (JsonUtility.FromJson(propertyTweenEffect.@from, type) ?? currentValue)));    
                }
                else if (type == typeof(Vector3))
                {
                    propertyTweenEffect.@from = JsonUtility.ToJson(EditorGUILayout.Vector3Field(label, (Vector3) (JsonUtility.FromJson(propertyTweenEffect.@from, type) ?? currentValue)));    
                }
                else if (type == typeof(Vector4) || type == typeof(Quaternion))
                {
                    propertyTweenEffect.@from = JsonUtility.ToJson(EditorGUILayout.Vector4Field(label, (Vector4) (JsonUtility.FromJson(propertyTweenEffect.@from, type) ?? currentValue)));    
                }
                else if (type == typeof(Color))
                {
                    propertyTweenEffect.@from = JsonUtility.ToJson(EditorGUILayout.ColorField(label, (Color) (JsonUtility.FromJson(propertyTweenEffect.@from, type) ?? currentValue)));    
                }
            }
            else
            {
                const string label = "To";
                if (type == typeof(float))
                {
                    var floatContainer = new FloatContainer{ Float = (float)currentValue };
                    if (propertyTweenEffect.to != null)
                    {
                        floatContainer = (FloatContainer) JsonUtility.FromJson(propertyTweenEffect.to, typeof(FloatContainer));
                    }
                    
                    propertyTweenEffect.to = JsonUtility.ToJson(new FloatContainer{ Float = EditorGUILayout.FloatField(label, floatContainer.Float)});    
                }
                else if (type == typeof(int))
                {
                    var intContainer = new IntContainer{ Int = (int)currentValue };
                    if (propertyTweenEffect.to != null)
                    {
                        intContainer = (IntContainer) JsonUtility.FromJson(propertyTweenEffect.to, typeof(IntContainer));
                    }
                    
                    propertyTweenEffect.to = JsonUtility.ToJson(new IntContainer{ Int = EditorGUILayout.IntField(label, intContainer.Int)});    
                }
                else if (type == typeof(Vector2))
                {
                    propertyTweenEffect.to = JsonUtility.ToJson(EditorGUILayout.Vector2Field(label, (Vector2) (JsonUtility.FromJson(propertyTweenEffect.to, type) ?? currentValue)));    
                }
                else if (type == typeof(Vector3))
                {
                    propertyTweenEffect.to = JsonUtility.ToJson(EditorGUILayout.Vector3Field(label, (Vector3) (JsonUtility.FromJson(propertyTweenEffect.to, type) ?? currentValue)));    
                }
                else if (type == typeof(Vector4) || type == typeof(Quaternion))
                {
                    propertyTweenEffect.to = JsonUtility.ToJson(EditorGUILayout.Vector4Field(label, (Vector4) (JsonUtility.FromJson(propertyTweenEffect.to, type) ?? currentValue)));    
                }
                else if (type == typeof(Color))
                {
                    propertyTweenEffect.to = JsonUtility.ToJson(EditorGUILayout.ColorField(label, (Color) (JsonUtility.FromJson(propertyTweenEffect.to, type) ?? currentValue)));    
                }
            }
        }

        
        private static PropertyTweenEffect FindPropertyTweenEffectObject(SerializedProperty property)
        {
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[0]
            var path = property.propertyPath.Split('.');
            int position = 0;
            int layer = 1;

            var obj = (object)property.serializedObject.targetObject;
            
            while(layer < property.depth)
            {
                if (obj is GameFeelDescription desc)
                {
                    position = 2;
                    layer++;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    obj = desc.TriggerList[index];
                }
                else if (obj is GameFeelTrigger trigger)
                {
                    position = 5;
                    layer++;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    obj = trigger.EffectGroups[index];
                }
                else if (obj is GameFeelEffectGroup group)
                {
                    position = 8;
                    layer++;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    obj = group.EffectsToExecute[index];
                }
                else if (obj is GameFeelEffect effect)
                {
                    layer++;
                    //We got to the bottom of this path!
                    if (property.depth == layer)
                    {
                        break;
                    }

                    position += 3;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    //TODO: deal with CustomTrail?...
                    obj = effect.ExecuteAfterCompletion[index];
                }
            }

            return (PropertyTweenEffect)obj;
        }
    }
}