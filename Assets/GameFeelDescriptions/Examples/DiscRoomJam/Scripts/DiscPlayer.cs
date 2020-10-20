using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameFeelDescriptions.Examples
{
    public class DiscPlayer : MonoBehaviour
    {
        public float maxSpeed = 6f;
        public float maxDirectionalSpeed = 4f;
        
        public Animator animator;

        public float Speed;
        
        // Start is called before the first frame update
        void Start()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();

                if (animator == null)
                {
                    Debug.Log("Failed to find Animator on game object: "+gameObject.name);
                    Destroy(this);
                }
            }
        }
    
        // Update is called once per frame
        void Update()
        {
            var hSpeed = Input.GetAxis("Horizontal") * maxDirectionalSpeed;
            var vSpeed = Input.GetAxis("Vertical") * maxDirectionalSpeed;
            
            var speed = new Vector2(hSpeed, vSpeed);
            speed = Vector2.ClampMagnitude(speed, maxSpeed);

            animator.SetFloat("Speed", speed.magnitude);
            Speed = speed.magnitude;
            
            var direction = speed.AsVector3() * Time.deltaTime;

            if (transform.position.x > 4.5f)
            {
                direction.x = -Mathf.Abs(direction.x);
            }
            else if (transform.position.x < -4.5f)
            {
                direction.x = Mathf.Abs(direction.x);
            }
            
            if (transform.position.y > 4.5f)
            {
                direction.y = -Mathf.Abs(direction.y);
            }
            else if (transform.position.y < -4.5f)
            {
                direction.y = Mathf.Abs(direction.y);
            }

            transform.position += direction;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            var disc = other.gameObject.GetComponent<NormalDisc>();
            if (disc != null)
            {
                if (disc.Inactive == false)
                {
                    //We DIE!!
                    Destroy(gameObject);  
                }
            }
            else //This is a wall disc!!
            {
                //We DIE!!
                Destroy(gameObject);
            }
        }
    }
}