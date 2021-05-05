using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class SimpleMove : MonoBehaviour
    {

        public bool staticPositions;
        public Vector3 initialPosition;
        public Vector3 targetPosition;
        private Vector3 currentVelocity;

        public bool MovementOnly2D;
        public float SmoothTime = 0.7f;
        public float PauseTime = 0.7f;

        public float movementScale = 3f;
        
        private float pauseStartTime;
        
        // Start is called before the first frame update
        void Start()
        {
            if (!staticPositions)
            {
                initialPosition = transform.position;
                targetPosition = initialPosition +
                                 (MovementOnly2D ? Random.insideUnitCircle.AsVector3() : Random.onUnitSphere) *
                                 movementScale;
            }
            else
            {
                transform.position = initialPosition;
            }
        }

        // Update is called once per frame
        void Update()
        {
            //If close to target, find new target
            if ((transform.position - targetPosition).magnitude < 0.2f)
            {
                pauseStartTime = Time.time;
                //currentVelocity = Vector3.zero;
                if (staticPositions)
                {
                    //Swap them around.
                    var temp = targetPosition;
                    targetPosition = initialPosition;
                    initialPosition = temp;
                }
                else
                {
                    //Find new target!
                    targetPosition = initialPosition +
                                     (MovementOnly2D
                                         ? Random.insideUnitCircle.normalized.AsVector3()
                                         : Random.insideUnitSphere) * movementScale;
                }
            }

            //Move after a short pause
            if (Time.time > pauseStartTime + PauseTime)
            {
                //Smooth damp follow target!
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, SmoothTime);    
            }
        }
    }
}

