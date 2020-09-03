using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class PlayerMovement2D : MonoBehaviour
    {
        public CharacterController2D characterController2D;
        
        public float runSpeed = 40f;
        private float horizontalMove = 0f;

        public bool autoJump = false;
        private bool jump = false;
        private float timeSinceLastJump;
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

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || (autoJump && Time.time - timeSinceLastJump > 1))
            {
                jump = true;
                timeSinceLastJump = Time.time;
            }
        }

        private void FixedUpdate()
        {
            characterController2D.Move(horizontalMove * Time.fixedDeltaTime, false, jump);
            jump = false;
        }
    }
}