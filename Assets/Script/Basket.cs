using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public GameManager gameManager;
    public int points = 2; // Điểm khi bóng vào rổ

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            gameManager.AddScore(points);
            gameManager.ResetBall();
        }
    }
}
