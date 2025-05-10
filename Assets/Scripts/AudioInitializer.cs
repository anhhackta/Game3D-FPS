using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioInitializer : MonoBehaviour
{
    void Start()
    {
        Debug.Log("AudioInitializer: Starting initialization");
        
        // Khởi tạo âm thanh dựa trên scene hiện tại
        string sceneName = SceneManager.GetActiveScene().name;
        
        if (SoundManager.Instance != null)
        {
            
            if (sceneName == "SceneStart" || sceneName == "MainMenu")
            {
                // Phát nhạc nền cho menu
                SoundManager.Instance.PlayMusic("Background");
            }
            else if (sceneName == "SceneGame")
            {
                // Phát âm thanh đám đông cho scene game
                SoundManager.Instance.PlayCrowdSound("CrowdCheer");
                // Phát âm thanh bắt đầu game
                SoundManager.Instance.PlaySFX("StartGame");
            }
        }
        else
        {
            Debug.LogError("SoundManager.Instance is null! Can't initialize audio.");
        }
    }
}