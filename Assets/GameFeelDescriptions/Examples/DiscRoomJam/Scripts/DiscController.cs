using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFeelDescriptions.Examples
{
    public class DiscController : MonoBehaviour
    {
        public float TimeBetweenDiscs = 2f;

        public bool limitDiscCount;
        public int maxDiscs = 7;

        public int InitialDiscs = 4;
        public float TimeBetweenInitialDiscs = 0.2f;

        public GameObject DiscPrefab;
        public GameObject WallDiscPrefab;
        
        private List<GameObject> Discs = new List<GameObject>();

        //TODO: sound effects!
        //TODO: Display Time, drop bones on death, effects with Squeezer!!
        public float TimeInLevel;
        
        private float timeSinceLastSpawn;

        private DiscPlayer player;
        
        private void Start()
        {
            
            Instantiate(WallDiscPrefab, new Vector3(-4.5f, 4.5f,0), Quaternion.identity);
            player = FindObjectOfType<DiscPlayer>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.R))
            {
                Restart();
            }
            
            if (!player) return;
                
            TimeInLevel += Time.deltaTime;
            timeSinceLastSpawn += Time.deltaTime;

            if (Discs.Count < InitialDiscs)
            {
                if (timeSinceLastSpawn > TimeBetweenInitialDiscs)
                {
                    Discs.Add(Instantiate(DiscPrefab));
                    timeSinceLastSpawn = 0;
                }
            }
            else
            {
                if (timeSinceLastSpawn > TimeBetweenDiscs)
                {
                    Discs.Add(Instantiate(DiscPrefab));
                    timeSinceLastSpawn = 0;
                }
            }
        }

        public void Restart()
        {
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name, LoadSceneMode.Single);
        }
    }
}