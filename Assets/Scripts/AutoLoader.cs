using UnityEngine;

public class SettingsAutoLoader : MonoBehaviour
{
    public GameObject settingsPrefab;

    void Awake()
    {
        if (SettingsPanelManager.Instance == null)
        {
            Instantiate(settingsPrefab);
        }
    }
}
