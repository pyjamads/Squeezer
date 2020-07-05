using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class MovingPlatform : MonoBehaviour
    {
        public Vector3 endPointOffset;
        public float speed = 1f;
        
        public Vector3 origin;
        public Vector3 goal;
        private bool reverse = false;
        
        void Start()
        {
            origin = transform.position;
            goal = origin + endPointOffset;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, transform.position + endPointOffset);
        }

        // Update is called once per frame
        void Update()
        {
            //TODO: fix!!
            if (reverse)
            {
                transform.position += (transform.position - goal) * Time.deltaTime * speed;
            }
            else
            {
                
            }
        }
    }
}