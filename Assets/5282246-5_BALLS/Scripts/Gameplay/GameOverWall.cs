using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverWall : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ball")) { 
            Ball tBall = other.GetComponent<Ball>();
            if (tBall != null 
                && tBall.ballStatus == BallStatus.PartOfLevel 
                && GameManager.gameState == GameState.Gameplay
                ) {
                GameplayManager.Instance.GameOver();
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball tBall = other.GetComponent<Ball>();
            if (
                tBall != null 
                && tBall.ballStatus == BallStatus.PartOfLevel
                && GameManager.gameState == GameState.Gameplay
                )
            {
                GameplayManager.Instance.GameOver();
            }
        }
    }
}
