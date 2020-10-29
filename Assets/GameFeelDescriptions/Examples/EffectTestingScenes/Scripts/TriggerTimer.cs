using System.Collections;
using System.Collections.Generic;
using GameFeelDescriptions;
using UnityEngine;

namespace GameFeelDescriptions.Examples
{


    public class TriggerTimer : MonoBehaviour
    {
        [Header("Trigger Interval in seconds")]
        public float Interval = 1f;

        [Header("Event name to pass along")] public string EventName = "OnTimerTick";

        [Header("Whether to pass a position with the direction.")]
        public bool OnlyPassDirection;

        private float lastTriggerTime = 0f;

        // Update is called once per frame
        void Update()
        {
            //NOTE: using unscaled time, because some effects may change time scale...
            //Check whether enough time has passed since triggering last time!
            if (Time.unscaledTime < lastTriggerTime + Interval) return;
            lastTriggerTime = Time.unscaledTime;

            //Make random direction
            var randomDirection = Random.onUnitSphere;

            //Set position to transform.position.
            var pos = transform.position;

            //If a collider exists, instead find the closest point on the bounds.
            var col = GetComponent<Collider>();
            if (col)
            {
                pos = col.ClosestPointOnBounds(transform.position + randomDirection);
            }
            else
            {
                var col2D = GetComponent<Collider2D>();
                if (col2D)
                {
                    //For 2D colliders, make the random direction 2D.
                    randomDirection = Random.insideUnitCircle.normalized;

                    pos = col2D.ClosestPoint(transform.position + randomDirection);
                }
            }

            //Trigger a custom event with the settings, and scale the direction randomly between 0.3 and 2.
            GameFeelEffectExecutor.Instance.TriggerCustomEvent(gameObject, EventName,
                OnlyPassDirection
                    ? new DirectionalData(randomDirection * Random.Range(0.3f, 2f))
                    {
                        Origin = gameObject
                    }
                    : new PositionalData(pos, randomDirection * Random.Range(0.3f, 2f))
                    {
                        Origin = gameObject
                    }
            );
        }
    }
}