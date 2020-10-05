using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFeelDescriptions.Examples
{
    public class SimpleJump : MonoBehaviour
    {
        public float JumpForce = 6f;
        public KeyCode JumpKey = KeyCode.Space;

        public bool autoJump = true;
        public float timeBetweenJumps = 2f;

        private float lastJumpTime;

        // Update is called once per frame
        void Update()
        {
            var doJump = Input.GetKeyDown(JumpKey);

            if (autoJump && Time.time > lastJumpTime + timeBetweenJumps)
            {
                doJump = true;
            }

            if (doJump)
            {
                lastJumpTime = Time.time;

                var rigid = GetComponent<Rigidbody>();
                if (rigid)
                {
                    rigid.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                }
                else
                {
                    var rigid2D = GetComponent<Rigidbody2D>();
                    if (rigid2D)
                    {
                        rigid2D.AddForce(Vector3.up * JumpForce, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }
}
