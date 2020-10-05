using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class DestroyOnCollision : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            Destroy(gameObject);
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            Destroy(gameObject);
        }
    }
}