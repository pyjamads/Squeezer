using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions.Examples
{

    public class CameraFollow : MonoBehaviour
    {
        public GameObject follow;
        public Vector3 offset;

        public float lerpAmount = 0.1f;

        private Vector3 lastPos;

        private Vector3 currentVelocity;

        // Update is called once per frame
        void Update()
        {
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

            transform.position = Vector3.SmoothDamp(transform.position, follow.transform.position + currentOffset, ref currentVelocity, 0.2f);
            //transform.position += diffPos * lerpAmount * Time.deltaTime;

            // lastPos = follow.transform.position;
        }
    }
}