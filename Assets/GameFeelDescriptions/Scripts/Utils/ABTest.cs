using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GameFeelDescriptions
{
    public class ABTest : MonoBehaviour
    {
        public bool enabled;
        public KeyCode rotateKey;
        public List<GameObject> ABGroups;

        private int currentGroup = 0;

        private void Update()
        {
            if (enabled && Input.GetKeyDown(rotateKey))
            {
                currentGroup = ++currentGroup % ABGroups.Count;
                
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
}