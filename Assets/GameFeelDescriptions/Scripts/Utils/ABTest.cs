using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;


namespace GameFeelDescriptions
{
    public class ABTest : MonoBehaviour
    {
        public bool displayCurrent = true;
        [FormerlySerializedAs("rotateKey")] 
        public KeyCode rotateNextKey = KeyCode.Plus;
        public KeyCode rotatePrevKey = KeyCode.Minus;
        public List<GameObject> ABGroups;

        public int currentGroup = 0;

        private void OnEnable()
        {
            Initialize();
        }

        public void Initialize()
        {
            //Find active, or set the first one active!
            for (var index = 0; index < ABGroups.Count; index++)
            {
                var abGroup = ABGroups[index];
                if (!abGroup.activeSelf) continue;
                
                currentGroup = index;
                break;
            }
            
            //Disable everyone but the first/currently active one.
            for (var index = 0; index < ABGroups.Count; index++)
            {
                var abGroup = ABGroups[index];
                if (currentGroup == index)
                {    
                    abGroup.SetActive(true);
                    
                }
                else
                {
                    abGroup.SetActive(false);    
                }
            }
        }

        private void OnGUI()
        {
            if (displayCurrent)
            {
                GUI.Label(new Rect(Screen.width / 2f - 75, 0, 150, 100), "Current active effect tree: " + ABGroups[currentGroup].name);    
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(rotateNextKey))
            {
                SwitchToNextGroup();
            }
            else if (Input.GetKeyDown(rotatePrevKey))
            {
                SwitchToPrevGroup();
            }
        }
        
        public void SwitchToNextGroup()
        {
            currentGroup = ++currentGroup % ABGroups.Count;

            SwitchToGroup(currentGroup);
        }
        
        public void SwitchToPrevGroup()
        {
            currentGroup = (--currentGroup + ABGroups.Count) % ABGroups.Count;

            SwitchToGroup(currentGroup);
        }
        
        public void SwitchToGroup(int groupIndex)
        {
            currentGroup = groupIndex;
            
            for (var index = 0; index < ABGroups.Count; index++)
            {
                var abGroup = ABGroups[index];
                if (currentGroup == index)
                {    
                    abGroup.SetActive(true);
                }
                else
                {
                    abGroup.SetActive(false);    
                }
            }
        }
    }
}