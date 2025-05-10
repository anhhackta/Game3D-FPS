using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VideoSettings : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Dropdown qualityDropdown;
    public Toggle vSyncToggle;
    public Slider brightnessSlider;
    public Toggle showFPSToggle;

    private Resolution[] resolutions;
    private UniversalRenderPipelineAsset urpAsset;

    void Start()
    {
        // Load URP asset for brightness
        urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

        // Load resolutions
        resolutions = GetFilteredResolutions();
        resolutionDropdown.ClearOptions();
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resolutionDropdown.options.Add(new Dropdown.OptionData($"{res.width} x {res.height} @ {res.refreshRate}Hz"));
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
                resolutionDropdown.value = i;
        }
        resolutionDropdown.RefreshShownValue();

        // Load saved settings
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
        showFPSToggle.isOn = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        // Apply initial settings
        ApplyBrightness(brightnessSlider.value);
        ApplyShowFPS(showFPSToggle.isOn);
    }

    Resolution[] GetFilteredResolutions()
    {
        var allRes = Screen.resolutions;
        var filtered = new List<Resolution>();
        var seen = new HashSet<string>();

        foreach (var res in allRes)
        {
            string key = $"{res.width}x{res.height}";
            if (!seen.Contains(key))
            {
                seen.Add(key);
                filtered.Add(res);
            }
        }
        return filtered.ToArray();
    }

    public void ApplyResolution()
    {
        int idx = resolutionDropdown.value;
        var res = resolutions[idx];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn);
        PlayerPrefs.SetInt("ResolutionIndex", idx);
    }

    public void ApplyFullscreen(bool isFull)
    {
        Screen.fullScreen = isFull;
        PlayerPrefs.SetInt("Fullscreen", isFull ? 1 : 0);
    }

    public void ApplyQuality(int q)
    {
        QualitySettings.SetQualityLevel(q);
        PlayerPrefs.SetInt("Quality", q);
    }

    public void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        PlayerPrefs.SetInt("VSync", on ? 1 : 0);
    }

    public void ApplyBrightness(float b)
    {
        PlayerPrefs.SetFloat("Brightness", b);
        // Use Post-Processing for brightness
        if (urpAsset != null)
        {
            var profile = FindPostProcessingProfile();
            if (profile != null && profile.TryGet<ColorAdjustments>(out var colorAdjust))
            {
                colorAdjust.postExposure.value = b * 2f - 1f; // Map 0-1 to -1 to 1
            }
        }
    }

    public void ApplyShowFPS(bool show)
    {
        PlayerPrefs.SetInt("ShowFPS", show ? 1 : 0);
        var fpsCounter = FindObjectOfType<FPSCounter>();
        if (fpsCounter != null)
            fpsCounter.enabled = show;
    }

    public void ResetDefaults()
    {
        resolutionDropdown.value = 0;
        fullscreenToggle.isOn = true;
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        vSyncToggle.isOn = true;
        brightnessSlider.value = 1f;
        showFPSToggle.isOn = false;

        ApplyResolution();
        ApplyFullscreen(true);
        ApplyQuality(QualitySettings.GetQualityLevel());
        ApplyVSync(true);
        ApplyBrightness(1f);
        ApplyShowFPS(false);

        PlayerPrefs.DeleteKey("ResolutionIndex");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.DeleteKey("Quality");
        PlayerPrefs.DeleteKey("VSync");
        PlayerPrefs.DeleteKey("Brightness");
        PlayerPrefs.DeleteKey("ShowFPS");
    }

    private VolumeProfile FindPostProcessingProfile()
    {
        var volume = FindObjectOfType<Volume>();
        return volume != null ? volume.sharedProfile : null;
    }
}