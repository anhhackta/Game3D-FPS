using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingBar;

    private void Start()
    {
        // Gắn sự kiện cho các Button
        if (playButton != null)
            playButton.onClick.AddListener(OnStartButton);
        else
            Debug.LogError("playButton is not assigned!");

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButton);
        else
            Debug.LogError("settingsButton is not assigned!");

        if (quitButton != null)
            quitButton.onClick.AddListener(OnExitButton);
        else
            Debug.LogError("quitButton is not assigned!");

        // Phát nhạc nền
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayMusic("Background");
        else
            Debug.LogError("AudioManager.Instance is null!");
    }

    public void OnStartButton()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        AsyncOperation operation = null;

        try
        {
            operation = SceneManager.LoadSceneAsync("SceneGame");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading scene: {ex.Message}");
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
            yield break;
        }

        if (operation != null)
        {
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                float progress = Mathf.Clamp01(operation.progress / 0.9f);
                if (loadingBar != null)
                    loadingBar.value = progress;
                else
                    Debug.Log($"Loading progress: {progress * 100}%");
                yield return null;
            }

            operation.allowSceneActivation = true;
        }
        else
        {
            Debug.LogError("Failed to load scene: SceneGame");
            if (loadingScreen != null)
                loadingScreen.SetActive(false);
        }
    }

    public void OnSettingsButton()
    {
        Debug.Log("Clicked Settings!");
        if (SettingsPanelManager.Instance != null)
        {
            Debug.Log("SettingsPanelManager found!");
            SettingsPanelManager.Instance.OpenSettings();
        }
        else
        {
            Debug.LogError("SettingsPanelManager.Instance is null!");
        }
    }

    public void OnExitButton()
    {
        Debug.Log("Clicked Exit!");
        Application.Quit();
    }
}