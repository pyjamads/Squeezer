using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class WallDisc : MonoBehaviour
    {
        public float maxSpeed = 8f;
        public float rotationsPerSecond = 5f;

        
        public List<Vector3> path = new List<Vector3>
        {
            new Vector3(-4.7f,4.7f,0),
            new Vector3(-4.7f,-4.7f,0),
            new Vector3(4.7f,-4.7f,0),
            new Vector3(4.7f,4.7f,0)
        };
        
        private int pathIndex;

        
        private void Update()
        {
            var direction = path[pathIndex] - transform.position;
            if (direction.magnitude < 0.1f)
            {
                pathIndex = ++pathIndex % path.Count;
                direction = path[pathIndex] - transform.position;
            }
            
            transform.position += direction.normalized * (maxSpeed * Time.deltaTime);
            transform.Rotate(Vector3.forward, (360 * rotationsPerSecond) * Time.deltaTime);
        }
    }
}