using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public GameObject pausePanel;
    public Button resumeButton;    
    public Button restartButton;  
    public Button homeButton;    
    public Button settingsButton;  
    public Button exitButton;      
    public string startSceneName = "StartScene"; 
    public Image spinningImage;
    public float spinSpeed = 30f;
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("PausePanel initial state: " + pausePanel.activeSelf);
        }
        else
        {
            Debug.LogError("pausePanel is not assigned!");
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }
        else
        {
            Debug.LogError("resumeButton is not assigned!");
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("restartButton is not assigned!");
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(BackToMenu);
        }
        else
        {
            Debug.LogError("homeButton is not assigned!");
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OpenSettings);
        }
        else
        {
            Debug.LogError("settingsButton is not assigned!");
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ResumeGame);
        }
        else
        {
            Debug.LogError("exitButton is not assigned!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (isPaused && spinningImage != null)
        {
            spinningImage.transform.Rotate(0, 0, -spinSpeed * Time.unscaledDeltaTime);
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        AudioManager.Instance?.StopCrowd();
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Debug.Log("Paused game, PausePanel active: " + pausePanel.activeSelf);
            CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.interactable = true;
            }
            else
            {
                pausePanel.AddComponent<CanvasGroup>().blocksRaycasts = true;
                pausePanel.GetComponent<CanvasGroup>().interactable = true;
            }
        }
        else
        {
            Debug.LogError("pausePanel is null!");
        }
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        AudioManager.Instance?.PlayCrowdSound("CrowdCheer");
        if (pausePanel != null)
        {
            CanvasGroup canvasGroup = pausePanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
            pausePanel.SetActive(false);
            Debug.Log("Resumed game, PausePanel active: " + pausePanel.activeSelf);
        }
        else
        {
            Debug.LogError("pausePanel is null!");
        }

        // Close Settings if open
        if (SettingsPanelManager.Instance != null && SettingsPanelManager.Instance.panelRoot != null && SettingsPanelManager.Instance.panelRoot.activeSelf)
        {
            SettingsPanelManager.Instance.CloseSettings();
            Debug.Log("Closed SettingsPanel from ResumeGame");
        }
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false; // Đảm bảo trạng thái pause được reset
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Debug.Log("Restarting game");
    }

    void BackToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false; // Đảm bảo trạng thái pause được reset
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
        AudioManager.Instance?.StopCrowd();
        SceneManager.LoadScene(startSceneName);
        Debug.Log("Returning to MainMenu");
    }

    void OpenSettings()
    {
        Debug.Log("Opening Settings from PauseManager");
        if (SettingsPanelManager.Instance != null)
        {
            SettingsPanelManager.Instance.OpenSettings();
        }
        else
        {
            Debug.LogError("SettingsPanelManager.Instance is null!");
        }
    }
}