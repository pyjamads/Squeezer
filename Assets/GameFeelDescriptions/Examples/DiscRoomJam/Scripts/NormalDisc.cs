using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class NormalDisc : MonoBehaviour
    {
        public float maxSpeed = 3.5f;
        public float rotationsPerSecond = 5f;

        public float InactiveTime = 1.5f;
        public bool Inactive = true;
        
        private float timeSinceSpawn;

        private Vector3 direction;

        private void Start()
        {
            direction = Random.insideUnitCircle.normalized;
            var renderer = GetComponent<SpriteRenderer>();
            renderer.color = renderer.color.withA(0.4f);
        }

        private void Update()
        {
            timeSinceSpawn += Time.deltaTime;

            if (timeSinceSpawn > InactiveTime)
            {
                if (Inactive)
                {
                    var renderer = GetComponent<SpriteRenderer>();
                    renderer.color = renderer.color.withA(1f);
                    Inactive = false;
                }
                
                transform.position += direction * (maxSpeed * Time.deltaTime);
                transform.Rotate(Vector3.forward, (360 * rotationsPerSecond) * Time.deltaTime);
            }
            else
            {
                transform.position += direction * (0.1f * Time.deltaTime);
            }

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
        }
    }
}