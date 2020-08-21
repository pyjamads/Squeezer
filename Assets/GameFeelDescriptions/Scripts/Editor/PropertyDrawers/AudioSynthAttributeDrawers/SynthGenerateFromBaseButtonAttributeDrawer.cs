using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(AudioSynthPlayEffect.SynthGenerateFromBaseButtonAttribute))]
    public class SynthGenerateFromBaseButtonAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var audioSynthEffect = FindAudioSynthPlayEffectObject(property);
            
            EditorGUI.BeginProperty(position, label, property);

            //[Label]    [Dropdown][GenerateAndPlayButton]
            var standardLabelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            var buttonPosition = position;
            buttonPosition.width = 65;
            buttonPosition.x = position.x + position.width - buttonPosition.width;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            
            var typeLabelPosition = position;
            typeLabelPosition.x += EditorGUIUtility.labelWidth + 1 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.width = position.width - buttonPosition.width - EditorGUIUtility.labelWidth - 2 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.height = EditorGUIUtility.singleLineHeight;
            
            var storedIndent = EditorGUI.indentLevel;
           
            
            EditorGUI.LabelField(standardLabelPosition, label);    
           
            EditorGUI.indentLevel = 0;
            
            EditorGUI.PropertyField(typeLabelPosition, property, GUIContent.none, true);

            if (GUI.Button(buttonPosition, "generate"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Generate Synth values");
                audioSynthEffect.GenerateSynthParameters(true);

                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = storedIndent;
            
            EditorGUI.EndProperty();
        }
        
        public static AudioSynthPlayEffect FindAudioSynthPlayEffectObject(SerializedProperty property)
        {
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[0].soundGeneratorBase //property.depth = 6
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[0].latestSynthParameters.Array.data[0] //property.depth = 7
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[2].ExecuteAfterCompletion.Array.data[0].soundGeneratorBase //property.depth = 8
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[2].ExecuteAfterCompletion.Array.data[0].ExecuteOnOffspring.Array.data[1].soundGeneratorBase //property.depth = 10
            //TriggerList.Array.data[0].EffectGroups.Array.data[1].EffectsToExecute.Array.data[2].ExecuteAfterCompletion.Array.data[0].ExecuteOnOffspring.Array.data[1].latestSynthParameters.Array.data[0] //property.depth = 11
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
                    layer += 2;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    obj = trigger.EffectGroups[index];
                }
                else if (obj is GameFeelEffectGroup group)
                {
                    position = 8;
                    layer += 2;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    obj = group.EffectsToExecute[index];
                }
                else if (obj is GameFeelEffect effect)
                {
                    layer += 2;
                    //We got to the bottom of this path!
                    if (string.Equals(property.name, "data") && property.depth == layer - 1)
                    {
                        break;
                    }

                    position += 3;
                    var before = path[position].IndexOf('[');
                    var after = path[position].IndexOf(']');
                    var index = int.Parse(path[position].Substring(before+1, after-before-1));
                    
                    //Check if this is a spawner effect and whether the we're on the offspring path.
                    if (effect is SpawningGameFeelEffect spawner && string.Compare(path[position - 2], "ExecuteOnOffspring") == 0)
                    {    
                        obj = spawner.ExecuteOnOffspring[index];
                    }
                    else
                    {
                        obj = effect.ExecuteAfterCompletion[index];
                    }
                }
            }

            return (AudioSynthPlayEffect)obj;
        }
    }
}