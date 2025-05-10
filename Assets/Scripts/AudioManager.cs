using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource crowdSource;
    public AudioSource sfxSource;

    public AudioClip[] musicTracks;
    public AudioClip[] crowdSounds;
    public AudioClip[] sfxSounds;

    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSounds();
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSounds()
    {
        foreach (var clip in sfxSounds)
        {
            sfxDict[clip.name] = clip;
        }

        foreach (var clip in crowdSounds)
        {
            sfxDict[clip.name] = clip;
        }

        foreach (var clip in musicTracks)
        {
            sfxDict[clip.name] = clip;
        }
    }

    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out AudioClip clip))
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(string name)
    {
        if (sfxDict.TryGetValue(name, out AudioClip clip))
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayCrowd(string name)
    {
        if (sfxDict.TryGetValue(name, out AudioClip clip))
        {
            crowdSource.clip = clip;
            crowdSource.loop = true;
            crowdSource.Play();
        }
    }

    // Thêm phương thức StopMusic
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }
    // Thêm phương thức StopCrowd
    public void StopCrowd()
    {
        if (crowdSource != null)
        {
            crowdSource.Stop();
        }
    }
    // Thêm phương thức StopAllSFX
    public void StopAllSFX()
    {
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
    }

    // Thêm phương thức để thiết lập âm lượng đám đông
    public void SetCrowdVolume(float volume)
    {
        if (crowdSource != null)
        {
            crowdSource.volume = volume;
            PlayerPrefs.SetFloat("CrowdVolume", volume);
            PlayerPrefs.Save();
        }
    }

    // Thêm phương thức phát âm thanh đám đông
    public void PlayCrowdSound(string name)
    {
        PlayCrowd(name);
    }

    public void SetVolume(string type, float value)
    {
        switch (type)
        {
            case "Music":
                musicSource.volume = value;
                PlayerPrefs.SetFloat("MusicVolume", value);
                break;
            case "Crowd":
                crowdSource.volume = value;
                PlayerPrefs.SetFloat("CrowdVolume", value);
                break;
            case "SFX":
                sfxSource.volume = value;
                PlayerPrefs.SetFloat("SFXVolume", value);
                break;
        }
        PlayerPrefs.Save();
    }

    void LoadVolumeSettings()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        crowdSource.volume = PlayerPrefs.GetFloat("CrowdVolume", 1f);
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }
}
