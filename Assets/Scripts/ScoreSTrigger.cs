// ScoreTrigger.cs
using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            GameManager.Instance?.BallInBasket();
        }
    }
}