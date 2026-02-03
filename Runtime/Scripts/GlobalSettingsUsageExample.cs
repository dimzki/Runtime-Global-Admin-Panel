using Alzaki.GlobalSettings;
using UnityEngine;

/// <summary>
/// Example script demonstrating how to use GlobalSettings in your code.
/// Attach this to a GameObject to test the system.
/// </summary>
public class GlobalSettingsUsageExample : MonoBehaviour
{
    [Header("Runtime Values (Read from GlobalSettings)")]
    [SerializeField] private int _version;
    [SerializeField] private float _idleTimer;
    [SerializeField] private string _playerName;
    [SerializeField] private bool _debugMode;
    [SerializeField] private Color _themeColor;

    // ═════════════════════════════════════════════════════════════════════════════
    // LIFECYCLE - Settings are available in Awake()
    // ═════════════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        Debug.Log("[GlobalSettingsUsageExample] Awake() - Loading settings...");

        // Settings are GUARANTEED to be loaded here because GlobalSettingsManager
        // initializes with RuntimeInitializeOnLoadMethod(BeforeSceneLoad)

        LoadAllSettings();

        Debug.Log($"[GlobalSettingsUsageExample] Settings loaded successfully!");
        Debug.Log($"  Version: {_version}");
        Debug.Log($"  IdleTimer: {_idleTimer}");
        Debug.Log($"  PlayerName: {_playerName}");
        Debug.Log($"  DebugMode: {_debugMode}");
        Debug.Log($"  ThemeColor: {_themeColor}");
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // READING SETTINGS
    // ═════════════════════════════════════════════════════════════════════════════

    private void LoadAllSettings()
    {
        // Basic types with default fallback values
        _version = GlobalSettingsManager.GetInt("Version", 1);
        _idleTimer = GlobalSettingsManager.GetFloat("IdleTimer", 10f);
        _playerName = GlobalSettingsManager.GetString("PlayerName", "Guest");
        _debugMode = GlobalSettingsManager.GetBool("DebugMode", false);
        _themeColor = GlobalSettingsManager.GetColor("ThemeColor", Color.blue);
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // PUBLIC API (for testing)
    // ═════════════════════════════════════════════════════════════════════════════

    public void OpenSettingsWindow()
    {
        GlobalSettingsManager.OpenSettingsWindow();
    }

    public void SetExampleValues()
    {
        GlobalSettingsManager.SetInt("Version", 2);
        GlobalSettingsManager.SetFloat("IdleTimer", 15f);
        GlobalSettingsManager.SetString("PlayerName", "TestPlayer");
        GlobalSettingsManager.SetBool("DebugMode", true);
        GlobalSettingsManager.SetColor("ThemeColor", Color.red);

        Debug.Log("[GlobalSettingsUsageExample] Example values set!");
        LoadAllSettings();
    }

    public void ReloadSettings()
    {
        LoadAllSettings();
        Debug.Log("[GlobalSettingsUsageExample] Settings reloaded!");
    }

    public void ExampleDirectAccess()
    {
        // Access the ScriptableObject directly for advanced usage
        GlobalSettings settings = GlobalSettingsManager.Settings;

        if (settings != null)
        {
            // You can use any method from GlobalSettings
            bool hasVersion = settings.HasInt("Version");
            Debug.Log($"Has 'Version' setting: {hasVersion}");
        }
    }
}
