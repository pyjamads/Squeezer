using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class SimpleCameraRotate : MonoBehaviour
    {
        public KeyCode Left = KeyCode.LeftArrow;
        public KeyCode Right = KeyCode.RightArrow;

        public bool autoRotate = true;
        public float rotationSpeed = 20f;

        public bool doRotationInTicks = false;

        private float lastTickTime;
        
        void Update()
        {
            var rotateLeft = false;
            if (autoRotate)
            {
                rotateLeft = true;
            }
            else if (Input.GetKeyDown(Left))
            {
                rotateLeft = true;
                autoRotate = false;
            }

            var rotateRight = false;
            if (Input.GetKeyDown(Right))
            {
                rotateRight = true;
                autoRotate = false;
            }
            
            //Step once per second, if stepping in ticks!
            if (autoRotate && doRotationInTicks && Time.time < lastTickTime + 1f) return;

            lastTickTime = Time.time;
            
            var angle = doRotationInTicks ? rotationSpeed : rotationSpeed * Time.deltaTime;
            
            if (rotateLeft)
            {
                // Spin the object around the target at rotationSpeed degrees/second.
                transform.RotateAround(Vector3.zero, Vector3.up, angle);
                transform.LookAt(Vector3.zero);
            }
            
            if (rotateRight)
            {
                // Spin the object around the target at rotationSpeed degrees/second.
                transform.RotateAround(Vector3.zero, Vector3.up, -angle);   
                transform.LookAt(Vector3.zero);
            }
        }
    }
}