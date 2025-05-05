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

    public int score = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        UpdateScoreUI();
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
        else
            Debug.LogError("NotificationText is not assigned in GameManager!");
    }

    public void AddScore(int points)
    {
        // Không thêm điểm nếu game bị pause
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        score += points;
        UpdateScoreUI();
        ShowNotification($"Ghi điểm! +{points}");
    }

    public void ShowNotification(string message)
    {
        // Cho phép thông báo "Game Over" ngay cả khi pause
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            Invoke(nameof(HideNotification), notificationDuration);
        }
        else
            Debug.LogError("Cannot show notification because NotificationText is not assigned!");
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
        else
            Debug.LogError("ScoreText is not assigned in GameManager!");
    }

    void HideNotification()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    public void ResetBall()
    {
        // Không reset bóng nếu game bị pause
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        if (ball != null && ballSpawnPoint != null)
        {
            ball.transform.position = ballSpawnPoint.position;
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }
            else
                Debug.LogError("Ball Rigidbody is not assigned in GameManager!");
        }
        else
            Debug.LogError("Ball or BallSpawnPoint is not assigned in GameManager!");
    }
}