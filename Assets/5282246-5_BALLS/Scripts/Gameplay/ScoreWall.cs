using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
//        Debug.Log(other.name);
        if (other.CompareTag("Ball")) { 
            Ball tBall = other.GetComponent<Ball>();
            if (tBall != null) {
                tBall.DestroyBall();
            }
        }
    }
}
