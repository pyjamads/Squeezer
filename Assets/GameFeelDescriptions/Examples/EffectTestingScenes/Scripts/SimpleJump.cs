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

        private Rigidbody rigid;
        private Rigidbody2D rigid2D;
        
        private void Start()
        {
            rigid = GetComponent<Rigidbody>();
            rigid2D = GetComponent<Rigidbody2D>();

            //Schedule the first jump to 0.1s after scene load.
            lastJumpTime = 0.1f - timeBetweenJumps;
        }


        // Update is called once per frame
        void Update()
        {
            var doJump = Input.GetKeyDown(JumpKey);

            if (autoJump && Time.time > lastJumpTime + timeBetweenJumps)
            {
                doJump = true;
                if (JumpForce > 10)
                {
                    if (rigid)
                    {
                        Physics.gravity = Vector3.down * 20;
                    }
                    else if (rigid2D)
                    {
                        rigid2D.gravityScale = 2;
                    }
                }
                else
                {
                    if (rigid)
                    {
                        Physics.gravity = Vector3.down * 9.82f;
                    }
                    else if (rigid2D)
                    {
                        rigid2D.gravityScale = 1;
                    }
                }
            }

            if (doJump)
            {
                lastJumpTime = Time.time;

                if (rigid)
                {
                    rigid.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);
                }
                else if (rigid2D)
                {
                    rigid2D.AddForce(Vector3.up * JumpForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
