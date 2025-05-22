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
    public Dropdown shadowQualityDropdown;
    public Dropdown antiAliasingDropdown;
    public Dropdown textureQualityDropdown;

    private Resolution[] resolutions;
    private UniversalRenderPipelineAsset[] urpAssets; // URP-Performant, Balanced, HighFidelity
    private UniversalRenderPipelineAsset currentUrpAsset;

    void Start()
    {
        // Load URP assets
        urpAssets = new UniversalRenderPipelineAsset[3];
        urpAssets[0] = Resources.Load<UniversalRenderPipelineAsset>("Settings/URP-Performant");
        urpAssets[1] = Resources.Load<UniversalRenderPipelineAsset>("Settings/URP-Balanced");
        urpAssets[2] = Resources.Load<UniversalRenderPipelineAsset>("Settings/URP-HighFidelity");

        // Log URP asset loading
        for (int i = 0; i < urpAssets.Length; i++)
        {
            Debug.Log($"URP Asset at index {i}: {(urpAssets[i] != null ? urpAssets[i].name : "null")}");
        }

        currentUrpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (currentUrpAsset == null)
        {
            Debug.LogError("Current Render Pipeline is not a UniversalRenderPipelineAsset!");
        }

        // Load resolutions
        resolutions = GetFilteredResolutions();
        resolutionDropdown.ClearOptions();
        for (int i = 0; i < resolutions.Length; i++)
        {
            var res = resolutions[i];
            resolutionDropdown.options.Add(new Dropdown.OptionData($"{res.width}x{res.height} @ {res.refreshRateRatio.value.ToString("F1")}Hz"));
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height && Mathf.Approximately((float)res.refreshRateRatio.value, (float)Screen.currentResolution.refreshRateRatio.value))
                resolutionDropdown.value = i;
        }
        resolutionDropdown.RefreshShownValue();

        // Setup quality dropdown
        qualityDropdown.ClearOptions();
        qualityDropdown.options.Add(new Dropdown.OptionData("Low (Performant)"));
        qualityDropdown.options.Add(new Dropdown.OptionData("Medium (Balanced)"));
        qualityDropdown.options.Add(new Dropdown.OptionData("High (HighFidelity)"));
        qualityDropdown.value = PlayerPrefs.GetInt("Quality", 1); // Default: Balanced
        qualityDropdown.RefreshShownValue();

        // Setup shadow quality dropdown
        shadowQualityDropdown.ClearOptions();
        shadowQualityDropdown.options.Add(new Dropdown.OptionData("Off"));
        shadowQualityDropdown.options.Add(new Dropdown.OptionData("Low"));
        shadowQualityDropdown.options.Add(new Dropdown.OptionData("Medium"));
        shadowQualityDropdown.options.Add(new Dropdown.OptionData("High"));
        shadowQualityDropdown.value = PlayerPrefs.GetInt("ShadowQuality", 2); // Default: Medium
        shadowQualityDropdown.RefreshShownValue();

        // Setup anti-aliasing dropdown
        antiAliasingDropdown.ClearOptions();
        antiAliasingDropdown.options.Add(new Dropdown.OptionData("Off"));
        antiAliasingDropdown.options.Add(new Dropdown.OptionData("2x MSAA"));
        antiAliasingDropdown.options.Add(new Dropdown.OptionData("4x MSAA"));
        antiAliasingDropdown.options.Add(new Dropdown.OptionData("8x MSAA"));
        antiAliasingDropdown.value = PlayerPrefs.GetInt("AntiAliasing", 1); // Default: 2x MSAA
        antiAliasingDropdown.RefreshShownValue();

        // Setup texture quality dropdown
        textureQualityDropdown.ClearOptions();
        textureQualityDropdown.options.Add(new Dropdown.OptionData("Low"));
        textureQualityDropdown.options.Add(new Dropdown.OptionData("Medium"));
        textureQualityDropdown.options.Add(new Dropdown.OptionData("High"));
        textureQualityDropdown.value = PlayerPrefs.GetInt("TextureQuality", 2); // Default: High
        textureQualityDropdown.RefreshShownValue();

        // Load saved settings
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        vSyncToggle.isOn = PlayerPrefs.GetInt("VSync", 1) == 1;
        brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);
        showFPSToggle.isOn = PlayerPrefs.GetInt("ShowFPS", 0) == 1;

        // Apply initial settings
        ApplyResolution(resolutionDropdown.value);
        ApplyQuality(qualityDropdown.value);
        ApplyShadowQuality(shadowQualityDropdown.value);
        ApplyAntiAliasing(antiAliasingDropdown.value);
        ApplyTextureQuality(textureQualityDropdown.value);
        ApplyFullscreen(fullscreenToggle.isOn);
        ApplyVSync(vSyncToggle.isOn);
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

    public void ApplyResolution(int index)
    {
        var res = resolutions[index];
        Screen.SetResolution(res.width, res.height, fullscreenToggle.isOn ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed, res.refreshRateRatio);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        Debug.Log($"Applied resolution: {res.width}x{res.height} @ {res.refreshRateRatio.value:F1}Hz");
    }

    public void ApplyFullscreen(bool isFull)
    {
        Screen.fullScreenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        PlayerPrefs.SetInt("Fullscreen", isFull ? 1 : 0);
        Debug.Log($"Fullscreen: {isFull}");
    }

    public void ApplyQuality(int index)
    {
        if (index >= 0 && index < urpAssets.Length && urpAssets[index] != null)
        {
            GraphicsSettings.renderPipelineAsset = urpAssets[index];
            currentUrpAsset = urpAssets[index];
            PlayerPrefs.SetInt("Quality", index);
            Debug.Log($"Applied URP Quality: {urpAssets[index].name}");
        }
        else
        {
            Debug.LogError($"URP Asset at index {index} is null or invalid! Falling back to current URP Asset.");
            if (currentUrpAsset != null)
            {
                GraphicsSettings.renderPipelineAsset = currentUrpAsset;
                qualityDropdown.value = PlayerPrefs.GetInt("Quality", 1); // Revert to saved value
            }
        }
    }

    public void ApplyShadowQuality(int index)
    {
        if (currentUrpAsset != null)
        {
            switch (index)
            {
                case 0: // Off
                    currentUrpAsset.shadowDistance = 0;
                    break;
                case 1: // Low
                    currentUrpAsset.shadowDistance = 20;
                    break;
                case 2: // Medium
                    currentUrpAsset.shadowDistance = 50;
                    break;
                case 3: // High
                    currentUrpAsset.shadowDistance = 100;
                    break;
            }
            PlayerPrefs.SetInt("ShadowQuality", index);
            Debug.Log($"Applied Shadow Quality: {index}, Shadow Distance: {currentUrpAsset.shadowDistance}");
        }
    }

    public void ApplyAntiAliasing(int index)
    {
        if (currentUrpAsset != null)
        {
            currentUrpAsset.msaaSampleCount = index switch
            {
                0 => 0, // Off
                1 => 2, // 2x MSAA
                2 => 4, // 4x MSAA
                3 => 8, // 8x MSAA
                _ => 2
            };
            PlayerPrefs.SetInt("AntiAliasing", index);
            Debug.Log($"Applied Anti-Aliasing: {index}");
        }
    }

    public void ApplyTextureQuality(int index)
    {
        QualitySettings.globalTextureMipmapLimit = 2 - index; // 0: High, 1: Medium, 2: Low
        PlayerPrefs.SetInt("TextureQuality", index);
        Debug.Log($"Applied Texture Quality: {index}");
    }

    public void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
        PlayerPrefs.SetInt("VSync", on ? 1 : 0);
        Debug.Log($"VSync: {on}");
    }

    public void ApplyBrightness(float b)
    {
        PlayerPrefs.SetFloat("Brightness", b);
        if (currentUrpAsset != null)
        {
            var profile = FindPostProcessingProfile();
            if (profile != null && profile.TryGet<ColorAdjustments>(out var colorAdjust))
            {
                colorAdjust.postExposure.value = b * 2f - 1f; // Map 0-1 to -1 to 1
                Debug.Log($"Applied Brightness: {b}");
            }
        }
    }

    public void ApplyShowFPS(bool show)
    {
        PlayerPrefs.SetInt("ShowFPS", show ? 1 : 0);
        var fpsCounter = FindObjectOfType<FPSCounter>();
        if (fpsCounter != null)
            fpsCounter.enabled = show;
        Debug.Log($"Show FPS: {show}");
    }

    public void ResetDefaults()
    {
        resolutionDropdown.value = 0;
        fullscreenToggle.isOn = true;
        qualityDropdown.value = 1; // Balanced
        shadowQualityDropdown.value = 2; // Medium
        antiAliasingDropdown.value = 1; // 2x MSAA
        textureQualityDropdown.value = 2; // High
        vSyncToggle.isOn = true;
        brightnessSlider.value = 1f;
        showFPSToggle.isOn = false;

        ApplyResolution(0);
        ApplyFullscreen(true);
        ApplyQuality(1);
        ApplyShadowQuality(2);
        ApplyAntiAliasing(1);
        ApplyTextureQuality(2);
        ApplyVSync(true);
        ApplyBrightness(1f);
        ApplyShowFPS(false);

        PlayerPrefs.DeleteKey("ResolutionIndex");
        PlayerPrefs.DeleteKey("Fullscreen");
        PlayerPrefs.DeleteKey("Quality");
        PlayerPrefs.DeleteKey("ShadowQuality");
        PlayerPrefs.DeleteKey("AntiAliasing");
        PlayerPrefs.DeleteKey("TextureQuality");
        PlayerPrefs.DeleteKey("VSync");
        PlayerPrefs.DeleteKey("Brightness");
        PlayerPrefs.DeleteKey("ShowFPS");
        Debug.Log("Reset Video Settings to Defaults");
    }

    private VolumeProfile FindPostProcessingProfile()
    {
        var volume = FindObjectOfType<Volume>();
        return volume != null ? volume.sharedProfile : null;
    }
}