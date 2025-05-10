using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    // Game objects để thông báo
    public GameObject winNotificationObject; // Container thông báo thắng
    public GameObject loseNotificationObject; // Container thông báo thua
    
    public GameObject ball; // Đối tượng bóng
    public GameObject ballSpawnPosition; // Vị trí spawn bóng (GameObject)
    public float resetDelay = 3f; // Thời gian delay trước khi reset màn chơi
    public Image fadeOverlay; // Hình ảnh đen để làm mờ màn hình khi thua

    private bool isGameOver = false;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Tắt các thành phần UI ban đầu
        if (winNotificationObject != null)
            winNotificationObject.SetActive(false);
            
        if (loseNotificationObject != null)
            loseNotificationObject.SetActive(false);
            
        if (fadeOverlay != null) {
            Color startColor = fadeOverlay.color;
            startColor.a = 0f; // Hoàn toàn trong suốt
            fadeOverlay.color = startColor;
            fadeOverlay.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // Tắt nhạc nền khi bắt đầu trò chơi (nếu không phải scene 1)
        if (SceneManager.GetActiveScene().buildIndex != 0) { // Giả sử scene 1 có buildIndex = 0
            if (AudioManager.Instance != null)
                AudioManager.Instance.StopMusic();
        }
        
        // Phát âm thanh khi bắt đầu trò chơi
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySFX("StartGame");
            // Phát âm thanh khán giả sau 5 giây
            StartCoroutine(PlayCrowdSoundAfterDelay(5.0f));
        }
    }

    // Coroutine để phát âm thanh khán giả sau khi bắt đầu trò chơi
    private IEnumerator PlayCrowdSoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Phát âm thanh khán giả
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCrowd("CrowdCheer");
        }
    }
    
    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // Hiển thị thông báo thắng
        if (winNotificationObject != null)
            winNotificationObject.SetActive(true);
        
        // Phát âm thanh chiến thắng và âm thanh ghi điểm
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSFX();
            AudioManager.Instance.PlaySFX("Score"); // Âm thanh bóng vào rổ
            AudioManager.Instance.PlaySFX("Win");   // Âm thanh chiến thắng
        }
        
        // Reset màn chơi sau một khoảng thời gian
        Invoke(nameof(ResetLevel), resetDelay);
    }

    public void LoseGame(string reason = "Bạn đã thua!")
    {
        if (isGameOver) return;
        isGameOver = true;
        
        // Hiển thị thông báo thua
        if (loseNotificationObject != null)
            loseNotificationObject.SetActive(true);
        
        // Phát âm thanh thua cuộc
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSFX();
            AudioManager.Instance.PlaySFX("Lose");
        }
        
        // Làm mờ dần màn hình
        StartCoroutine(FadeToBlack());
        
        // Reset màn chơi sau một khoảng thời gian
        Invoke(nameof(ResetLevel), resetDelay);
    }
    
    IEnumerator FadeToBlack()
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);
            Color overlayColor = fadeOverlay.color;
            float elapsedTime = 0;
            float fadeTime = resetDelay * 0.8f; // Hoàn thành trước thời gian reset
            
            while (elapsedTime < fadeTime)
            {
                float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeTime);
                overlayColor.a = alpha;
                fadeOverlay.color = overlayColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Đảm bảo alpha = 1 khi kết thúc
            overlayColor.a = 1;
            fadeOverlay.color = overlayColor;
        }
    }

    public void ResetBall()
    {
        if (ball != null && ballSpawnPosition != null)
        {
            ball.transform.SetParent(null); // Đảm bảo bóng không gắn vào tay
            ball.transform.position = ballSpawnPosition.transform.position;
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = false;
            }
            
            // Phát âm thanh khi reset bóng
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("BallReset");
        }
    }
    
    void ResetLevel()
    {
        // Tải lại scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Xử lý khi BOT bắt được bóng
    public void BotCaughtBall()
    {
        LoseGame("Bot đã bắt được bóng!");
    }
    
    // Xử lý khi ném bóng vào rổ
    public void BallInBasket()
    {
        WinGame();
    }
}