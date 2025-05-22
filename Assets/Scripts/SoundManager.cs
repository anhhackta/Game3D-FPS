using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup crowdMixerGroup;

    [Header("Sound Settings")]
    [SerializeField] private Sound[] musicTracks;
    [SerializeField] private Sound[] crowdSounds;
    [SerializeField] private Sound[] sfxSounds;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private const int INITIAL_SFX_POOL_SIZE = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Khôi phục trạng thái âm thanh khi bắt đầu
        RestoreSoundState();
    }

    private void Initialize()
    {
        // Load volume settings từ PlayerPrefs
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        float crowdVolume = PlayerPrefs.GetFloat("CrowdVolume", 0.8f);

        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        SetCrowdVolume(crowdVolume);
        // Khởi tạo dictionary
        RegisterSounds(musicTracks, musicMixerGroup);
        RegisterSounds(crowdSounds, crowdMixerGroup);
        RegisterSounds(sfxSounds, sfxMixerGroup);

        // Khởi tạo SFX pool
        InitializeSFXPool();
    }

    private void RegisterSounds(Sound[] sounds, AudioMixerGroup mixerGroup)
    {
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
            sound.source.outputAudioMixerGroup = mixerGroup;

            if (!soundDictionary.ContainsKey(sound.name))
            {
                soundDictionary.Add(sound.name, sound);
            }
        }
    }

    private void InitializeSFXPool()
    {
        for (int i = 0; i < INITIAL_SFX_POOL_SIZE; i++)
        {
            CreateNewSFXSource();
        }
    }

    private AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource source in sfxPool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return CreateNewSFXSource();
    }

    private AudioSource CreateNewSFXSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.outputAudioMixerGroup = sfxMixerGroup;
        sfxPool.Add(newSource);
        return newSource;
    }

    #region Public Methods
    public void PlayMusic(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            if (!sound.source.isPlaying)
            {
                sound.source.Play();
            }
        }
        else
        {
            Debug.LogWarning("Music not found: " + name);
        }
    }

    public void PlayCrowdSound(string name)
    {
        // Đổi tên cho phù hợp với tên thực sự trong mảng
        string correctName = name;
        
        // Ánh xạ tên từ code sang tên trong Inspector
        if (name == "CrowdAmbient") correctName = "CrowdCheer";
        
        if (soundDictionary.TryGetValue(correctName, out Sound sound))
        {
            if (!sound.source.isPlaying)
            {
                sound.source.Play();
                Debug.Log($"Playing crowd sound: {name} (mapped to {correctName})");
            }
        }
        else
        {
            Debug.LogWarning($"Crowd sound not found: {name} (tried to map to {correctName})");
        }
    }

    public void PlaySFX(string name)
    {
        // Đổi tên cho phù hợp với tên thực sự trong mảng
        string correctName = name;
        
        // Ánh xạ tên từ code sang tên trong Inspector
        if (name == "GameStart") correctName = "StartGame";
        else if (name == "GameOver") correctName = "Lose";
        else if (name == "Victory") correctName = "Win";
        else if (name == "BallBounce") correctName = "Bounce";
        
        if (soundDictionary.TryGetValue(correctName, out Sound sound))
        {
            AudioSource source = GetAvailableSFXSource();
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.pitch = sound.pitch;
            source.loop = sound.loop;
            source.Play();
            Debug.Log($"Playing SFX: {name} (mapped to {correctName})");
        }
        else
        {
            Debug.LogWarning($"SFX not found: {name} (tried to map to {correctName})");
        }
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in sfxPool)
        {
            source.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        try
        {
            if (musicMixerGroup != null && musicMixerGroup.audioMixer != null)
            {
                // Tránh logarithm của 0 bằng cách đặt giá trị tối thiểu
                float safeVolume = Mathf.Max(0.0001f, volume);
                musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(safeVolume) * 20);
            }
            else
            {
                Debug.LogWarning("musicMixerGroup is null or its audioMixer is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting music volume: {e.Message}");
        }
    }

    public void SetSFXVolume(float volume)
    {
        try
        {
            if (sfxMixerGroup != null && sfxMixerGroup.audioMixer != null)
            {
                float safeVolume = Mathf.Max(0.0001f, volume);
                sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(safeVolume) * 20);
            }
            else
            {
                Debug.LogWarning("sfxMixerGroup is null or its audioMixer is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting SFX volume: {e.Message}");
        }
    }

    public void SetCrowdVolume(float volume)
    {
        try
        {
            if (crowdMixerGroup != null && crowdMixerGroup.audioMixer != null)
            {
                float safeVolume = Mathf.Max(0.0001f, volume);
                crowdMixerGroup.audioMixer.SetFloat("CrowdVolume", Mathf.Log10(safeVolume) * 20);
            }
            else
            {
                Debug.LogWarning("crowdMixerGroup is null or its audioMixer is null");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting crowd volume: {e.Message}");
        }
    }

    // Lưu trạng thái âm thanh trước khi chuyển Scene
    public void SaveSoundState()
    {
        // Lưu tên các âm thanh đang phát để khôi phục sau khi chuyển Scene
        List<string> activeCrowdSounds = new List<string>();
        List<string> activeMusics = new List<string>();
        
        // Kiểm tra âm thanh đám đông đang phát
        foreach (Sound sound in crowdSounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                activeCrowdSounds.Add(sound.name);
                Debug.Log($"Saving crowd sound: {sound.name}");
            }
        }
        
        // Kiểm tra nhạc đang phát
        foreach (Sound sound in musicTracks)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                activeMusics.Add(sound.name);
                Debug.Log($"Saving music: {sound.name}");
            }
        }
        
        // Lưu danh sách bằng cách nối các tên với dấu phẩy
        PlayerPrefs.SetString("ActiveCrowdSounds", string.Join(",", activeCrowdSounds));
        PlayerPrefs.SetString("ActiveMusics", string.Join(",", activeMusics));
        PlayerPrefs.Save();
    }

    public void RestoreSoundState()
    {
        Debug.Log("Restoring sound state...");
        
        try
        {
            // Dừng tất cả âm thanh đang chạy trước khi khôi phục
            StopAllAudio();

            // Khôi phục âm thanh đám đông
            string activeCrowdSoundsStr = PlayerPrefs.GetString("ActiveCrowdSounds", "");
            if (!string.IsNullOrEmpty(activeCrowdSoundsStr))
            {
                string[] activeCrowdSounds = activeCrowdSoundsStr.Split(',');
                foreach (string soundName in activeCrowdSounds)
                {
                    if (!string.IsNullOrEmpty(soundName))
                    {
                        PlayCrowdSound(soundName);
                        Debug.Log($"Restoring crowd sound: {soundName}");
                    }
                }
            }

            // Khôi phục nhạc
            string activeMusicsStr = PlayerPrefs.GetString("ActiveMusics", "");
            if (!string.IsNullOrEmpty(activeMusicsStr))
            {
                string[] activeMusics = activeMusicsStr.Split(',');
                foreach (string musicName in activeMusics)
                {
                    if (!string.IsNullOrEmpty(musicName))
                    {
                        PlayMusic(musicName);
                        Debug.Log($"Restoring music: {musicName}");
                    }
                }
            }
            Debug.Log("Sound state restored successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error restoring sound state: {ex.Message}\n{ex.StackTrace}");
        }
    }

    // Thêm các method này để dễ dàng thêm âm thanh mới từ Editor
    public void AddNewMusic(Sound newSound)
    {
        AddSoundToArray(ref musicTracks, newSound);
    }

    public void AddNewCrowdSound(Sound newSound)
    {
        AddSoundToArray(ref crowdSounds, newSound);
    }

    public void AddNewSFX(Sound newSound)
    {
        AddSoundToArray(ref sfxSounds, newSound);
    }

    private void AddSoundToArray(ref Sound[] array, Sound newSound)
    {
        List<Sound> tempList = new List<Sound>(array);
        tempList.Add(newSound);
        array = tempList.ToArray();
        RegisterSounds(array, GetMixerGroupForType(newSound));
    }

    private AudioMixerGroup GetMixerGroupForType(Sound sound)
    {
        // Implement logic để xác định mixer group dựa trên loại âm thanh
        // Cần cập nhật dựa trên cách bạn tổ chức trong Inspector
        return sfxMixerGroup;
    }
    #endregion

    public void StopAllAudio()
    {
        // Dừng SFX
        StopAllSFX();

        // Dừng âm thanh đám đông
        foreach (Sound sound in crowdSounds)
        {
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }

        // Dừng nhạc
        foreach (Sound sound in musicTracks)
        {
            if (sound.source != null)
            {
                sound.source.Stop();
            }
        }
    }
}