using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(AudioSynthPlayEffect.CopyToSynthParametersButtonAttribute))]
    public class CopyToSynthParametersButtonAttributeDrawer : PropertyDrawer
    {
        private SfxrSynth synth = new SfxrSynth();
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var audioSynthEffect = SynthGenerateFromBaseButtonAttributeDrawer.FindAudioSynthPlayEffectObject(property);
            
            EditorGUI.BeginProperty(position, label, property);

            //[Label]    [Dropdown][GenerateAndPlayButton]
            var standardLabelPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            var buttonPosition = position;
            buttonPosition.width = 50;
            buttonPosition.x = position.x + position.width - buttonPosition.width;
            buttonPosition.height = EditorGUIUtility.singleLineHeight;
            
            var buttonPosition2 = position;
            buttonPosition2.width = 50;
            buttonPosition2.x = buttonPosition.x - buttonPosition2.width - 1 * EditorGUIUtility.standardVerticalSpacing;
            buttonPosition2.height = EditorGUIUtility.singleLineHeight;

            var typeLabelPosition = position;
            typeLabelPosition.x += EditorGUIUtility.labelWidth + 1 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.width = position.width - buttonPosition.width - buttonPosition2.width - EditorGUIUtility.labelWidth - 3 * EditorGUIUtility.standardVerticalSpacing;
            typeLabelPosition.height = EditorGUIUtility.singleLineHeight;
            
            var storedIndent = EditorGUI.indentLevel;
           
            
            EditorGUI.LabelField(standardLabelPosition, label);    
           
            EditorGUI.indentLevel = 0;
            
            EditorGUI.PropertyField(typeLabelPosition, property, GUIContent.none, true);

            if (GUI.Button(buttonPosition, "play"))
            {
                synth.parameters.SetSettingsString(property.stringValue);
                synth.Play();
            }
            else if(GUI.Button(buttonPosition2, "copy"))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Copy Synth values");
                audioSynthEffect.synthParameters = property.stringValue;
                audioSynthEffect.LoadSynthParameters(true);
                property.serializedObject.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = storedIndent;
            
            EditorGUI.EndProperty();
        }
    }
}