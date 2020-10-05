using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class SimpleMove : MonoBehaviour
    {
        private Vector3 initialPosition;
        private Vector3 targetPosition;
        private Vector3 currentVelocity;

        public bool MovementOnly2D;
        public float SmoothTime = 0.7f;
        public float PauseTime = 0.7f;

        public float movementScale = 3f;
        
        private float pauseStartTime;
        
        // Start is called before the first frame update
        void Start()
        {
            initialPosition = transform.position;
            targetPosition = initialPosition + (MovementOnly2D ? Random.insideUnitCircle.AsVector3() : Random.onUnitSphere) * movementScale;
        }

        // Update is called once per frame
        void Update()
        {
            //If close to target, find new target
            if ((transform.position - targetPosition).magnitude < 0.2f)
            {
                pauseStartTime = Time.unscaledTime;
                //currentVelocity = Vector3.zero;
                targetPosition = initialPosition + (MovementOnly2D ? Random.insideUnitCircle.normalized.AsVector3() : Random.insideUnitSphere) * movementScale;
            }

            //Move after a short pause
            if (Time.unscaledTime > pauseStartTime + PauseTime)
            {
                //Smooth damp follow target!
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, SmoothTime);    
            }
        }
    }
}

