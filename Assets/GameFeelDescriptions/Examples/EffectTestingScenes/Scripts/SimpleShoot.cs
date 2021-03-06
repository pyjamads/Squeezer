using UnityEngine;

namespace GameFeelDescriptions.Examples
{
    public class SimpleShoot : MonoBehaviour
    {
        public GameObject projectilePrefab;
        
        public float projectileSpeed = 5f;
        public KeyCode ShootKey = KeyCode.Space;

        public bool autoShoot = true;
        public float timeBetweenShots = 0.3f;

        public float accuracy = 0.3f;
        
        //private float lastShootTime;

        public float charge;

        void Update()
        {
            var doShoot = Input.GetKeyDown(ShootKey);

            charge += Time.deltaTime;
            
            if (autoShoot && charge > timeBetweenShots)
            {
                doShoot = true;
                charge = 0;
            }

            if (doShoot)
            {
                //lastShootTime = Time.time;
                
                var go = Instantiate(projectilePrefab);
                
                var projectileVelocity = Vector3.zero;

                var rigid = go.GetComponent<Rigidbody>();
                if (rigid)
                {
                    projectileVelocity = transform.forward * projectileSpeed + Random.insideUnitCircle.AsVector3() * (2 * Random.value - 1f) * accuracy;
                    rigid.velocity = projectileVelocity;
                    
                    go.transform.position = transform.position + transform.forward * 0.25f;
                }
                else
                {
                    var rigid2D = go.GetComponent<Rigidbody2D>();
                    if (rigid2D)
                    {
                        projectileVelocity = Vector3.right * projectileSpeed + Vector3.up * (2 * Random.value - 1f) * accuracy;
                        rigid2D.velocity = projectileVelocity;
                        
                        go.transform.position = transform.position + Vector3.right * 0.25f;
                    }
                }
                
                GameFeelEffectExecutor.Instance.TriggerCustomEvent(gameObject, "Shoot", new PositionalData(go.transform.position,  projectileVelocity){Origin = gameObject});
            }
        }
    }
}