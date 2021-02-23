using UnityEngine;

namespace GameFeelDescriptions
{
    [ExecuteAlways]
    public class SimplePhysicsMovement : MonoBehaviour
    {
        public Vector3 Velocity;
        public float Drag;
        public bool ApplyGravity;
        public bool TimeScaleIndependent;
        public bool use2DPhysics;

        private Vector3 combinedVelocity;

        private bool initialized = false;
        
        private void Update()
        {
            if (!initialized)
            {
                combinedVelocity = Velocity;
                initialized = true;
            }
            
            var deltaTime = Time.unscaledDeltaTime;

            //Manually scaling time, if it's not independent of time!
            deltaTime *= TimeScaleIndependent ? 1f : Time.timeScale;

            transform.position += combinedVelocity * deltaTime;

            //Percentage reduction based on Drag (negative values = acceleration)
            combinedVelocity *= 1f - (Drag * deltaTime);

            if (ApplyGravity)
            {
                combinedVelocity += (use2DPhysics ? (Vector3)Physics2D.gravity : Physics.gravity) * deltaTime;
            }
        }
    }
}