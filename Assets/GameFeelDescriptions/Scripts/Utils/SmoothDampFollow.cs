using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions
{
    public class SmoothDampFollow : MonoBehaviour
    {
        public GameObject follow;
        
        public GameObject[] multiple;
        public float[] weights;
        
        public Vector3 offset;

        public float SmoothTime = 0.1f;

        private Vector3 lastPos;

        private Vector3 currentVelocity;

        // Update is called once per frame
        void Update()
        {
            if (follow == null && multiple.Length == 0)
            {
                Destroy(gameObject);
                return;
            }
            
            // var diffSinceLastPos = follow.transform.position - lastPos;
            //
             var currentOffset = offset;
            // if (diffSinceLastPos.x < 0)
            // {
            //     currentOffset.x *= -1;
            // }
            //
            // if (diffSinceLastPos.y < 0)
            // {
            //     currentOffset.y *= -1;
            // }

            //var diffPos = follow.transform.position + currentOffset - transform.position;

            var targetPosition = Vector3.zero;
            if (follow != null)
            {
                targetPosition = follow.transform.position;
            }
            else 
            {
                for (int i = 0; i < multiple.Length; i++)
                {
                    if(multiple[i] == null) continue;
                    
                    if (multiple.Length != weights.Length)
                    {
                        targetPosition += multiple[i].transform.position;
                    }
                    else
                    {
                        targetPosition += multiple[i].transform.position * weights[i];
                    }
                }
                
                //Average the length
                targetPosition /= multiple.Length;
            }
            
            transform.position = Vector3.SmoothDamp(transform.position,  targetPosition + currentOffset, ref currentVelocity, SmoothTime);
            //transform.position += diffPos * lerpAmount * Time.deltaTime;

            // lastPos = follow.transform.position;
        }
    }
}