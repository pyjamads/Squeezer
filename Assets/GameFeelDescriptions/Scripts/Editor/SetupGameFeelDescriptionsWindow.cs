using System;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class SetupGameFeelDescriptionsWindow : EditorWindow
    {
        [MenuItem("GameFeelDescriptions/Setup")]
        public static void ShowWindow() => GetWindow<SetupGameFeelDescriptionsWindow>("Setup Descriptions");

        // private void OnEnable()
        // {
        //     //setup Repainting, while the window is not selected, if needed!
        //     Selection.selectionChanged += Repaint;
        // }
        //
        // private void OnDisable()
        // {
        //     Selection.selectionChanged -= Repaint;
        // }

        private void OnGUI()
        {
            //NOTE consider using DisableScope,  for something
            // var someBOol = true;
            // using (new EditorGUI.DisabledScope(someBOol))
            // {
            //     //Anything in here is disabled if someBOol is true!
            // }

            GUILayout.Label("Select Tags to setup descriptions for!", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                //Things inside here are layouted horizontally.
                using (new GUILayout.HorizontalScope())
                {
                    
                }
            }
        }
    }
}