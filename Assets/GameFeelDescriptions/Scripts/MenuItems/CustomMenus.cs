using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameFeelDescriptions
{
    public static class CustomMenus
    {
//         //TODO: make this into an editor window, with check boxes etc... 31/05/2020
//         [MenuItem("GameFeelDescriptions/Setup From Tags")]
//         static void SetupGameFeelDescriptionsFromTags()
//         {
//             var defaultTags = new List<string>
//             {
//                 "Untagged",
//                 "Respawn",
//                 "Finish",
//                 "EditorOnly",
// //                "MainCamera",
//                 "Player",
//                 "GameController",
//             };
//             
//             var tags = UnityEditorInternal.InternalEditorUtility.tags;
//             
//             //Find or make Description parent GameObject
//             var parent = GameObject.Find("GameFeelDescriptions");
//             if(!parent) parent = new GameObject("GameFeelDescriptions");
//             
//             var created = new List<GameFeelDescription>();
//
//             //TODO: make this into a checkbox list, that displays all tags... 31/05/2020
//             var dontIncludeDefaultTags = EditorUtility.DisplayDialog("Include Default tags", 
//                 "Would you like to include the default tags: Untagged, Respawn, Finish, EditorOnly, GameController, Player? " +
//                 "(MainCamera always included)",
//                 "No", "Yes");
//
//             var stepThroughEnabledAlready = false;
//
//             var posOffset = 0;
//             for (var index = 0; index < tags.Length; index++)
//             {
//                 var tag = tags[index];
//                 
//                 //Filter out default tags.
//                 if(dontIncludeDefaultTags && defaultTags.Contains(tag)) continue;
//                 
//                 //Make popup where the user can select which tags to add
//                 //(simple version just makes a popup for each tag, and user says yes/no).
//                 var dialogResult = EditorUtility.DisplayDialogComplex("Add GameFeelDescriptions for [" + tag + "]?",
//                     "Would you like game objects with the tag [" + tag + "] to react with Game Feel Effects?",
//                     "Yes", "Yes, with StepThroughMode enabled!", "No");
//                 if (dialogResult != 2)
//                 {
//                     var desc = EditorHelpers.CreateGameFeelDescription(tag, parent, ref posOffset);
//
//                     //Set GameFeelDescription StepThroughMode to true based on first result
//                     if (dialogResult != 1) continue;
//                     
//                     //Popup asking about StepThroughMode (yes/no)?
//                     if (stepThroughEnabledAlready)
//                     {
//                         if (EditorUtility.DisplayDialog("Caution!", 
//                             "It is not recommended to run StepThroughMode on multiple Descriptions at once.", 
//                             "Do it anyway!", "Cancel"))
//                         {
//                             desc.StepThroughMode = true;
//                         }
//                     }
//                     else
//                     {
//                         desc.StepThroughMode = true;
//                         stepThroughEnabledAlready = true;
//                     }
//                     
//                     created.Add(desc);
//                 }
//             }
//             
//             if (parent.transform.childCount == 0)
//             {
//                 //No descriptions were created, so just destroy the parent.
//                 GameObject.Destroy(parent);
//             }
//             else if(created.Count > 0)
//             {
//                 //Doing this will probably not be useful, but for now it's fine to ping. 27/04/2020
//                 EditorGUIUtility.PingObject(created[0]);
//                 Selection.activeObject = created[0];
//             }
//         }

        [MenuItem("GameFeelDescriptions/Triggers/Attach To GameObjects")]
        static void AttachAllTriggers()
        {
            var descriptions = GameObject.FindObjectsOfType<GameFeelDescription>();
            for (int i = 0; i < descriptions.Length; i++)
            {
                descriptions[i].AttachTriggers();
            }
        }
        
        [MenuItem("GameFeelDescriptions/Triggers/Remove from GameObjects")]
        static void RemoveAllTriggers()
        {
            var triggers = GameObject.FindObjectsOfType<GameFeelBehaviorBase>();
            for (int i = 0; i < triggers.Length; i++)
            {
                Object.DestroyImmediate(triggers[i]);
            }
        }
    }
}