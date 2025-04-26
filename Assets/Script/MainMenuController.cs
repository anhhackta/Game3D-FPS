using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider loadingBar;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Toggle fpsToggle;
    public Toggle vSyncToggle;
    public Dropdown languageDropdown;
    public Slider mouseSpeedSlider;

    private Resolution[] resolutions;

    private void Start()
    {
        InitializeResolutions();
        LoadSettings();
    }

    public void OnStartButton()
    {
        StartCoroutine(LoadSceneAsync("SceneGame"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        loadingScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingBar.value = progress;
            yield return null;
        }
    }

    public void OnSettingsButton()
    {
        settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsButton()
    {
        settingsPanel.SetActive(false);
        SaveSettings();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        foreach (var res in resolutions)
        {
            resolutionDropdown.options.Add(new Dropdown.OptionData(res.width + " x " + res.height));
        }
        resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        resolutionDropdown.RefreshShownValue();
    }

    public void ApplyResolution()
    {
        int index = resolutionDropdown.value;
        Resolution resolution = resolutions[index];
        Screen.SetResolution(resolution.width, resolution.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void ApplyFullscreen()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
    }

    public void ApplyQuality()
    {
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        PlayerPrefs.SetInt("QualityLevel", qualityDropdown.value);
    }

    public void ApplyMusicVolume()
    {
        // Assume there's a music manager handling this
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
    }

    public void ApplySFXVolume()
    {
        // Assume there's an SFX manager handling this
        PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider.value);
    }

    public void ToggleFPS()
    {
        // Implement an FPS display toggle
        PlayerPrefs.SetInt("ShowFPS", fpsToggle.isOn ? 1 : 0);
    }

    public void ToggleVSync()
    {
        QualitySettings.vSyncCount = vSyncToggle.isOn ? 1 : 0;
        PlayerPrefs.SetInt("VSync", vSyncToggle.isOn ? 1 : 0);
    }

    public void ApplyMouseSpeed()
    {
        // Implement mouse speed application
        PlayerPrefs.SetFloat("MouseSpeed", mouseSpeedSlider.value);
    }

    public void ChangeLanguage()
    {
        // Implement language switching logic
        PlayerPrefs.SetInt("Language", languageDropdown.value);
    }

    private void LoadSettings()
    {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        qualityDropdown.value = PlayerPrefs.GetInt("QualityLevel", 2);
        musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1.0f);
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1.0f);
        fpsToggle.isOn = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        mouseSpeedSlider.value = PlayerPrefs.GetFloat("MouseSpeed", 1.0f);
        languageDropdown.value = PlayerPrefs.GetInt("Language", 0);
    }

    private void SaveSettings()
    {
        ApplyResolution();
        ApplyFullscreen();
        ApplyQuality();
        ApplyMusicVolume();
        ApplySFXVolume();
        ToggleFPS();
        ToggleVSync();
        ApplyMouseSpeed();
        ChangeLanguage();
    }
}
