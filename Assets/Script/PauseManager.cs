using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance; // Singleton
    public GameObject pausePanel; // Panel chứa menu pause
    public Button resumeButton; // Nút Tiếp tục
    public Button restartButton; // Nút Chơi lại
    public Button backToMenuButton; // Nút Quay lại StartScene
    public Canvas gameUICanvas; // Canvas chứa UI game (scoreText, notificationText)
    public string startSceneName = "StartScene"; // Tên scene menu chính
    private bool isPaused = false; // Trạng thái pause

    public bool IsPaused => isPaused; // Property để kiểm tra trạng thái pause

    void Awake()
    {
        // Thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ PauseManager qua các scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ẩn panel pause khi bắt đầu
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("PausePanel is not assigned in PauseManager!");
        }

        // Gán sự kiện cho các nút
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        else
        {
            Debug.LogError("ResumeButton is not assigned in PauseManager!");
        }
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("RestartButton is not assigned in PauseManager!");
        }
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.RemoveAllListeners();
            backToMenuButton.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("BackToMenuButton is not assigned in PauseManager!");
        }

       
    }

    void Update()
    {
        // Nhấn Esc để pause/resume
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Esc pressed, isPaused: " + isPaused);
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Tạm dừng game
        Cursor.lockState = CursorLockMode.None; // Mở khóa con trỏ
        Cursor.visible = true; // Hiển thị con trỏ

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            else
            {
                Debug.LogWarning("PausePanel is missing CanvasGroup! Adding one...");
                pausePanel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                pausePanel.GetComponent<CanvasGroup>().interactable = true;
            }
        }
        else
        {
            Debug.LogError("Cannot show PausePanel because it is not assigned!");
        }

        // Vô hiệu hóa UI game
        if (gameUICanvas != null)
        {
            CanvasGroup canvasGroup = gameUICanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        // Phát âm thanh nhấn nút
        if (SoundManager.Instance != null && SoundManager.Instance.buttonClickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.buttonClickClip);
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Tiếp tục game
        Cursor.lockState = CursorLockMode.Locked; // Khóa con trỏ
        Cursor.visible = false; // Ẩn con trỏ

        if (pausePanel != null)
        {
            CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("Cannot hide PausePanel because it is not assigned!");
        }

        // Kích hoạt lại UI game
        if (gameUICanvas != null)
        {
            CanvasGroup canvasGroup = gameUICanvas.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
        }

        // Phát âm thanh nhấn nút
        if (SoundManager.Instance != null && SoundManager.Instance.buttonClickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.buttonClickClip);
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f; // Khôi phục time scale trước khi reload
        /*if (SoundManager.Instance != null && SoundManager.Instance.buttonClickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.buttonClickClip);
        }*/
        if (GameManager.Instance != null)
        {
            GameManager.Instance.score = 0; // Reset điểm số
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void BackToMenu()
    {
        Time.timeScale = 1f; // Khôi phục time scale trước khi chuyển scene
        if (GameManager.Instance != null)
        {
            PlayerPrefs.SetInt("LastScore", GameManager.Instance.score);
        }
        /*if (SoundManager.Instance != null && SoundManager.Instance.buttonClickClip != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.buttonClickClip);
        }*/
        SceneManager.LoadScene(startSceneName);
    }
}