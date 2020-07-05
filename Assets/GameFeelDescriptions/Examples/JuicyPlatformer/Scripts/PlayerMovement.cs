using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class PlayerMovement : MonoBehaviour
    {
        private const float gravity = -9.82f;
        public bool isJumping;

        public float JumpForce = 10f;

        public float moveSpeed = 5f;

        public Vector3 velocity;

        // Update is called once per frame
        void Update()
        {
            if (isJumping)
            {
                velocity = velocity.withY(velocity.y + gravity * 2 * Time.deltaTime);
            }
            else
            {
                velocity = velocity.withY(0);
            }

            var leftOrRight = false;
            if (Input.GetKey(KeyCode.RightArrow))
            {
                leftOrRight = true;
                velocity = velocity.withX(moveSpeed);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                leftOrRight = true;
                velocity = velocity.withX(-moveSpeed);
            }

            if (!leftOrRight)
            {
                velocity = velocity.withX(0);
            }

            if (!isJumping && Input.GetKey(KeyCode.Space))
            {
                isJumping = true;
                velocity = velocity.withY(JumpForce);
            }

            transform.position += velocity * Time.deltaTime;
        }

        private void OnCollisionEnter(Collision other)
        {
            isJumping = false;
        }
    }
}
