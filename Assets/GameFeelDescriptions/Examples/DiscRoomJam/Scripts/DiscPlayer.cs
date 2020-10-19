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
            transform.position += speed.AsVector3() * Time.deltaTime;
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