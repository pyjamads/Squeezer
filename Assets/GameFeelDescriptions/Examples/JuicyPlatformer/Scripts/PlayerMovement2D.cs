using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class PlayerMovement2D : MonoBehaviour
    {
        public CharacterController2D characterController2D;
        
        public float runSpeed = 40f;
        private float horizontalMove = 0f;

        private bool jump = false;

        private void Start()
        {
            if (characterController2D == null)
            {
                characterController2D = GetComponent<CharacterController2D>();
            }
        }

        private void Update()
        {
            horizontalMove = Input.GetAxis("Horizontal") * runSpeed;

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                jump = true;
            }
        }

        private void FixedUpdate()
        {
            characterController2D.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
            jump = false;
        }
    }
}