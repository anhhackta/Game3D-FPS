using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject ball; // Đối tượng bóng
    public Transform ballSpawnPoint; // Vị trí xuất hiện lại của bóng
    public Text scoreText; // Text hiển thị điểm số
    public Text notificationText; // Text thông báo ghi điểm
    public float notificationDuration = 2f; // Thời gian hiển thị thông báo

    private int score = 0; // Điểm số
    private Rigidbody ballRigidbody;

    void Start()
    {
        ballRigidbody = ball.GetComponent<Rigidbody>();
        UpdateScoreText();
        notificationText.gameObject.SetActive(false);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreText();
        StartCoroutine(ShowNotification($"Ghi điểm! +{points}"));
    }

    public void ResetBall()
    {
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.angularVelocity = Vector3.zero;
        ballRigidbody.isKinematic = true;
        ball.transform.position = ballSpawnPoint.position;
        ballRigidbody.isKinematic = false;
    }

    void UpdateScoreText()
    {
        scoreText.text = $"Điểm: {score}";
    }

    IEnumerator ShowNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(notificationDuration);
        notificationText.gameObject.SetActive(false);
    }
}
