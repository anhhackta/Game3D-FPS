using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsPanelManager : MonoBehaviour
{
    public static SettingsPanelManager Instance;

    public GameObject panelRoot;
    public GameObject[] tabContents; // 0: Video, 1: Audio, 2: Tutorial, 3: Info
    public Button[] tabButtons; // Buttons to switch tabs

    private int currentTab = 0;

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
            return;
        }

        // Ensure panel is initially hidden
        if (panelRoot != null)
        {
            Debug.Log("SettingsPanel initial state: " + panelRoot.activeSelf);
            panelRoot.SetActive(false);
        }
        else
        {
            Debug.LogError("panelRoot is not assigned!");
        }
    }

    void Update()
    {
        // Only handle Escape in MainMenu scene
        if (SceneManager.GetActiveScene().name == "StartScene" && Input.GetKeyDown(KeyCode.Escape))
        {
            if (panelRoot.activeSelf)
                CloseSettings();
            else
                OpenSettings();
        }
    }

    public void OpenSettings()
    {
        if (panelRoot == null)
        {
            Debug.LogError("panelRoot is null in SettingsPanelManager!");
            return;
        }
        Debug.Log("Activating SettingsPanel: " + panelRoot.name);
        panelRoot.SetActive(true);
        Debug.Log("SettingsPanel active after SetActive: " + panelRoot.activeSelf);
        Debug.Log("SettingsCanvas active: " + panelRoot.transform.root.gameObject.activeSelf);
        SwitchTab(currentTab);
        Time.timeScale = 0f; // Pause game
    }

    public void CloseSettings()
    {
        if (panelRoot == null)
        {
            Debug.LogError("panelRoot is null in SettingsPanelManager!");
            return;
        }
        Debug.Log("Deactivating SettingsPanel: " + panelRoot.name);
        panelRoot.SetActive(false);
        // Only resume Time.timeScale in MainMenu
        if (SceneManager.GetActiveScene().name == "StartScene")
            Time.timeScale = 1f;
    }

    public void SwitchTab(int index)
    {
        if (index < 0 || index >= tabContents.Length)
        {
            Debug.LogError("Invalid tab index: " + index);
            return;
        }
        Debug.Log("Switching to tab: " + index);
        currentTab = index;
        for (int i = 0; i < tabContents.Length; i++)
        {
            if (tabContents[i] != null)
                tabContents[i].SetActive(i == index);
            else
                Debug.LogError("tabContents[" + i + "] is null!");
        }

        // Highlight tab button
        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
                tabButtons[i].interactable = (i != index);
            else
                Debug.LogError("tabButtons[" + i + "] is null!");
        }
    }

    public void ResetTab(int index)
    {
        Debug.Log("Resetting tab: " + index);
        switch (index)
        {
            case 0: // Video
                var videoSettings = tabContents[0]?.GetComponent<VideoSettings>();
                if (videoSettings != null)
                    videoSettings.ResetDefaults();
                else
                    Debug.LogError("VideoSettings component not found!");
                break;
            case 1: // Audio
                var audioSettings = tabContents[1]?.GetComponent<AudioSettings>();
                if (audioSettings != null)
                    audioSettings.ResetAudioSettings();
                else
                    Debug.LogError("AudioSettings component not found!");
                break;
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game");
        Application.Quit();
    }
}