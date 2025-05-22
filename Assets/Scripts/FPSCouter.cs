using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public Text fpsText;
    private float deltaTime = 0f;

    void Start()
    {
        // Load initial state
        bool showFPS = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        enabled = showFPS;
        if (fpsText != null)
            fpsText.enabled = showFPS;
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if (fpsText != null)
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
    }
}