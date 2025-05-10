using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket : MonoBehaviour
{
    public GameManager gameManager;
    public int points = 1; // Điểm khi bóng vào rổ


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            //GameManager.Instance.ShowNotification("Goal!");
        }
    }
}
