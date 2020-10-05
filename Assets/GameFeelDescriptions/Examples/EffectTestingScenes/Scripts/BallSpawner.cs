using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameFeelDescriptions.Examples
{
    public class BallSpawner : MonoBehaviour
    {
        public GameObject BallPrefab;
        
        public float SpawnForce = 2f;
        public KeyCode SpawnKey = KeyCode.Space;

        public bool autoSpawn = true;
        public float timeBetweenSpawning = 2f;

        private float lastSpawnTime;
        
        void Update()
        {
            var spawnBall = Input.GetKeyDown(SpawnKey);

            if (autoSpawn && Time.time > lastSpawnTime + timeBetweenSpawning)
            {
                spawnBall = true;
            }

            if (spawnBall)
            {
                lastSpawnTime = Time.time;

                var ball = Instantiate(BallPrefab);
                
                var rigid = ball.GetComponent<Rigidbody>();
                if (rigid)
                {
                    var dir = Random.onUnitSphere;
                    if (dir.Dot(Vector3.down) < 0)
                    {
                        dir = -dir;
                    }
                    
                    rigid.AddForce((Vector3.up + dir) * SpawnForce, ForceMode.Impulse);
                }
                else
                {
                    var rigid2D = ball.GetComponent<Rigidbody2D>();
                    if (rigid2D)
                    {
                        var dir = Random.insideUnitCircle.normalized.AsVector3();
                        if (dir.Dot(Vector3.down) < 0)
                        {
                            dir = -dir;
                        }
                        rigid2D.AddForce((Vector3.up + dir) * SpawnForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}