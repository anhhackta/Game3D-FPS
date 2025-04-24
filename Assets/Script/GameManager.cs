using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Text scoreText; // Text hiển thị điểm số
    public Text notificationText; // Text thông báo ghi điểm hoặc thua
    public GameObject ball; // Đối tượng bóng
    public Transform ballSpawnPoint; // Vị trí spawn lại bóng
    public float notificationDuration = 2f; // Thời gian hiển thị thông báo

    private int score = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateScoreUI();
        notificationText.gameObject.SetActive(false);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
        ShowNotification($"Ghi điểm! +{points}");
    }

    public void ShowNotification(string message)
    {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);
        Invoke(nameof(HideNotification), notificationDuration);
    }

    void UpdateScoreUI()
    {
        scoreText.text = $"Score: {score}";
    }

    void HideNotification()
    {
        notificationText.gameObject.SetActive(false);
    }

    public void ResetBall()
    {
        ball.transform.position = ballSpawnPoint.position;
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
    }
}