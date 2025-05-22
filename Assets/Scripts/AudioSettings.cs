using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public Slider musicSlider, crowdSlider, sfxSlider;

    void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager.Instance is null!");
            return;
        }

        // Load saved volumes
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        crowdSlider.value = PlayerPrefs.GetFloat("CrowdVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Attach listeners
        musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetVolume("Music", val));
        crowdSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetVolume("Crowd", val));
        sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetVolume("SFX", val));
    }

    public void ResetAudioSettings()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.SetVolume("Music", 1f);
        AudioManager.Instance.SetVolume("Crowd", 1f);
        AudioManager.Instance.SetVolume("SFX", 1f);

        musicSlider.value = 1f;
        crowdSlider.value = 1f;
        sfxSlider.value = 1f;

        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", 1f);
        PlayerPrefs.SetFloat("CrowdVolume", 1f);
        PlayerPrefs.SetFloat("SFXVolume", 1f);
    }
}