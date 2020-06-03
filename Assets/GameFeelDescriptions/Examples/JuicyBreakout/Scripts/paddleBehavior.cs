using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paddleBehavior : MonoBehaviour
{
    public bool ballFollow;
    public ballBehavior ball;

    private float targetX;
    private bool shouldFindNewTargetX;
    
    // Start is called before the first frame update
    void Start()
    {
        Input.simulateMouseWithTouches = true;
        targetX = ball.transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeSinceLevelLoad <= 1) return;
        
        if (ballFollow || (ball.ballReady && Input.anyKeyDown || Input.GetMouseButtonDown(0))) ball.ballReady = false;
        
        if (ballFollow && ball != null)
        {
            if (shouldFindNewTargetX)
            {
                targetX = (Random.value * transform.localScale.x) / 2f - transform.localScale.x / 4f;
                shouldFindNewTargetX = false;
            }

            var posX = Mathf.Lerp(transform.position.x,ball.transform.position.x + ball.velocity.normalized.x + targetX, 0.1f);
            posX = Mathf.Clamp(posX, -7.4f, 7.4f);
            transform.position = transform.position.withX(posX);
        }
        else
        {
            var posX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            posX = Mathf.Clamp(posX, -7.4f, 7.4f);
            transform.position = transform.position.withX(posX);    
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("ball"))
        {
            shouldFindNewTargetX = true;    
        }
    }
}
