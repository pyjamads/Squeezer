using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    [CustomPropertyDrawer(typeof(AudioSynthPlayEffect.MutateSynthParametersButtonAttribute))]
    public class MutateSynthParametersButtonAttributeDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.singleLineHeight + 4 * EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var audioSynthEffect = SynthGenerateFromBaseButtonAttributeDrawer.FindAudioSynthPlayEffectObject(property);



            using (new EditorGUI.PropertyScope(position, label, property))
            {



                //EditorGUI.BeginProperty(position, label, property);

                //[Label]    [params][mutate+play][play]
                position.y += 5;
                var standardLabelPosition =
                    new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

                var showButtons = !string.IsNullOrEmpty(audioSynthEffect.synthParameters);

                var buttonPosition = position;
                buttonPosition.width = showButtons ? 50 : 0;
                buttonPosition.x = position.x + position.width - buttonPosition.width;
                buttonPosition.height = EditorGUIUtility.singleLineHeight;

                var buttonPosition2 = position;
                buttonPosition2.width = showButtons ? 50 : 0;
                buttonPosition2.x = buttonPosition.x - buttonPosition2.width -
                                    1 * EditorGUIUtility.standardVerticalSpacing;
                buttonPosition2.height = EditorGUIUtility.singleLineHeight;

                var typeLabelPosition = position;
                typeLabelPosition.x += EditorGUIUtility.labelWidth + 1 * EditorGUIUtility.standardVerticalSpacing;
                typeLabelPosition.width = position.width - buttonPosition.width - buttonPosition2.width -
                                          EditorGUIUtility.labelWidth -
                                          3 * EditorGUIUtility.standardVerticalSpacing;
                typeLabelPosition.height = EditorGUIUtility.singleLineHeight;

                var storedIndent = EditorGUI.indentLevel;

                EditorGUI.LabelField(standardLabelPosition, label);

                EditorGUI.indentLevel = 0;
                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(typeLabelPosition, property, GUIContent.none, true);


                if (EditorGUI.EndChangeCheck())
                {
                    audioSynthEffect.LoadSynthParameters();
                    property.serializedObject.ApplyModifiedProperties();
                }

                if (showButtons && GUI.Button(buttonPosition, "play"))
                {
                    audioSynthEffect.PlaySound();
                }
                else if (showButtons && GUI.Button(buttonPosition2, "mutate"))
                {
                    Undo.RecordObject(property.serializedObject.targetObject, "Mutate Synth values");
                    audioSynthEffect.MutateSynthParameters(true);

                    property.serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel = storedIndent;

                var exportButtonPosition = EditorGUI.IndentedRect(position);
                exportButtonPosition.y +=
                    EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                exportButtonPosition.height = EditorGUIUtility.singleLineHeight;

                var editButtonPosition = exportButtonPosition;
                editButtonPosition.width =
                    (exportButtonPosition.width / 2f) - EditorGUIUtility.standardVerticalSpacing;

                exportButtonPosition.width = editButtonPosition.width;
                exportButtonPosition.x += editButtonPosition.width + 2 * EditorGUIUtility.standardVerticalSpacing;

                if (GUI.Button(exportButtonPosition, "Export wav file!"))
                {
                    var path = EditorUtility.SaveFilePanel("Export as WAV", "", "sound.wav", "wav");
                    if (path.Length != 0)
                    {
                        SfxrSynth synth = new SfxrSynth();
                        synth.parameters.SetSettingsString(audioSynthEffect.synthParameters);
                        File.WriteAllBytes(path, synth.GetWavFile());
                    }
                }

                if (GUI.Button(editButtonPosition, "Edit with usfxr!"))
                {
                    var window = ScriptableObject.CreateInstance<SfxrGenerator>();
                    window.name = "Sound Effects";
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
    			window.title = window.name;
#else
                    window.titleContent = new GUIContent(window.name);
#endif

                    //Copy our values
                    EditorGUIUtility.systemCopyBuffer = audioSynthEffect.synthParameters;
                    //And paste them into the editor.
                    window.PasteValues();
                    
                    //This throws an error when it closes,
                    //probably due to calling this through multiple layers of drawers.
                    //However the copy/pasting of the parameters work.
                    window.ShowModal();

                    Undo.RecordObject(property.serializedObject.targetObject, "Modify Synth Settings");
                    
                    window.CopyValues();
                    
                    audioSynthEffect.synthParameters = EditorGUIUtility.systemCopyBuffer;
                    audioSynthEffect.LoadSynthParameters();
                    property.serializedObject.ApplyModifiedProperties();

                    Debug.Log(
                        "Please ignore the stack exception, it's thrown due to using the window in modal mode. The Synth Parameters have been updated correctly!");
                }
            }


            //EditorGUI.EndProperty();
        }

        
        
        
        /*
         *var window = ScriptableObject.CreateInstance<SfxrGenerator>();
		window.name = "Sound Effects";
		#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
			window.title = window.name;
		#else
			window.titleContent = new GUIContent(window.name);
		#endif
		window.Show();
         * 
         */
    }
}