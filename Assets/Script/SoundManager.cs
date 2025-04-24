using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource musicSource; // Nguồn nhạc nền
    public AudioSource sfxSource; // Nguồn hiệu ứng âm thanh
    public AudioClip pickupBallClip; // Âm thanh nhặt bóng
    public AudioClip throwBallClip; // Âm thanh ném bóng
    public AudioClip scoreClip; // Âm thanh khi vào rổ
    public AudioClip buttonClickClip; // Âm thanh nhấn nút UI
    public AudioClip backgroundMusic; // Nhạc nền

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

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
}
