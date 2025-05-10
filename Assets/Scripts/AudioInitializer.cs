using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioInitializer : MonoBehaviour
{
    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (SoundManager.Instance != null)
        {
            
            if (sceneName == "SceneStart" || sceneName == "MainMenu")
            {
                SoundManager.Instance.PlayMusic("Background");
            }
            else if (sceneName == "SceneGame")
            {
                SoundManager.Instance.PlayCrowdSound("CrowdCheer");
                SoundManager.Instance.PlaySFX("StartGame");
            }
        }
        else
        {
            Debug.LogError("SoundManager.Instance is null! Can't initialize audio.");
        }
    }
}