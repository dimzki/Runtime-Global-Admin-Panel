using Alzaki.GlobalSettings;
using UnityEngine;

/// <summary>
/// Example script demonstrating the RUNTIME SETTINGS PANEL.
/// Attach this to a GameObject and press F12 during Play Mode to see the panel!
/// </summary>
public class GlobalSettingsRuntimeExample : MonoBehaviour
{
    [Header("Runtime Panel Demo")]
    [Tooltip("Press F12 during Play Mode to toggle the runtime settings panel!")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F12;
    [SerializeField] private bool showOnStart = false;

    private int _idleTimerDuration;
    private float _lastUpdateTime;

    // ═════════════════════════════════════════════════════════════════════════════
    // LIFECYCLE
    // ═════════════════════════════════════════════════════════════════════════════

    private void Start()
    {
        LoadSettings();

        if (showOnStart)
        {
            GlobalSettingsManager.ShowRuntimePanel();
        }

        Debug.Log("[RuntimeExample] Press F12 to toggle settings panel!");
    }

    private void Update()
    {
        // Toggle panel with hotkey
        if (Input.GetKeyDown(toggleKey))
        {
            GlobalSettingsManager.ToggleRuntimePanel();
        }

        // Reload settings every second to see changes
        if (Time.time - _lastUpdateTime > 1f)
        {
            LoadSettings();
            _lastUpdateTime = Time.time;
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // SETTINGS MANAGEMENT
    // ═════════════════════════════════════════════════════════════════════════════

    private void LoadSettings()
    {
        _idleTimerDuration = GlobalSettingsManager.GetInt("IDLE_TIMER_DURATION", 10);
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // PUBLIC API (for UI buttons)
    // ═════════════════════════════════════════════════════════════════════════════

    public void ShowRuntimePanel()
    {
        GlobalSettingsManager.ShowRuntimePanel();
        Debug.Log("Runtime panel shown! Edit values and click Save.");
    }

    public void HideRuntimePanel()
    {
        GlobalSettingsManager.HideRuntimePanel();
        Debug.Log("Runtime panel hidden!");
    }

    public void ToggleRuntimePanel()
    {
        GlobalSettingsManager.ToggleRuntimePanel();
    }

    public void ReloadSettings()
    {
        LoadSettings();
        Debug.Log($"Settings reloaded! Idle Timer: {_idleTimerDuration}");
    }

    public void TestSetValue()
    {
        int newValue = Random.Range(5, 30);
        GlobalSettingsManager.SetInt("IDLE_TIMER_DURATION", newValue);
        LoadSettings();
        Debug.Log($"Set IDLE_TIMER_DURATION to: {newValue}");
    }
}
