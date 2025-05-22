using UnityEngine;

public class BallSound : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("Bounce");
            }
        }

        if (collision.gameObject.CompareTag("Ring"))
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("Score");
            }
        }
    }
}
