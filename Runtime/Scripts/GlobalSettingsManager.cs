using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Singleton manager that provides access to GlobalSettings.
    /// Automatically loads settings from Resources BEFORE scene load.
    /// Access via GlobalSettingsManager.Instance
    /// </summary>
    public class GlobalSettingsManager : MonoBehaviour
    {
        private static GlobalSettingsManager _instance;
        private static GlobalSettings _settings;
        private static GlobalSettingsRuntimePanel _runtimePanel;

        private const string SETTINGS_RESOURCE_PATH = "GlobalSettings";

        // ═════════════════════════════════════════════════════════════════════════════
        // SINGLETON INSTANCE
        // ═════════════════════════════════════════════════════════════════════════════

        public static GlobalSettingsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    InitializeBeforeSceneLoad();
                }
                return _instance;
            }
        }

        public static GlobalSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    LoadSettings();
                }
                return _settings;
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // EVENTS
        // ═════════════════════════════════════════════════════════════════════════════

        public static UnityEvent OnAdminPanelSaved = new UnityEvent();

        // ═════════════════════════════════════════════════════════════════════════════
        // INITIALIZATION - RUNS BEFORE SCENE LOAD
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// This runs BEFORE any scene loads, ensuring settings are available in Awake() of any script.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            if (_instance != null) return;

            // Create persistent GameObject
            GameObject managerObject = new GameObject("[GlobalSettingsManager]");
            DontDestroyOnLoad(managerObject);

            _instance = managerObject.AddComponent<GlobalSettingsManager>();

            // Load settings
            LoadSettings();

            Debug.Log($"[GlobalSettingsManager] Initialized before scene load. Settings loaded: {_settings != null}");
        }

        private static void LoadSettings()
        {
            _settings = Resources.Load<GlobalSettings>(SETTINGS_RESOURCE_PATH);

            if (_settings == null)
            {
                Debug.LogError($"[GlobalSettingsManager] Failed to load GlobalSettings from Resources/{SETTINGS_RESOURCE_PATH}. " +
                               $"Please create a GlobalSettings asset and place it in Assets/Resources/ folder.");
            }
            else
            {
                Debug.Log($"[GlobalSettingsManager] GlobalSettings loaded successfully from Resources/{SETTINGS_RESOURCE_PATH}");
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // CONVENIENCE METHODS
        // ═════════════════════════════════════════════════════════════════════════════

        public static int GetInt(string key, int defaultValue = 0)
        {
            if (Settings != null && !Settings.HasInt(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Int settings.");
            return Settings != null ? Settings.GetInt(key, defaultValue) : defaultValue;
        }
        public static void SetInt(string key, int value) { if (Settings != null) Settings.SetInt(key, value); }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            if (Settings != null && !Settings.HasFloat(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Float settings.");
            return Settings != null ? Settings.GetFloat(key, defaultValue) : defaultValue;
        }
        public static void SetFloat(string key, float value) { if (Settings != null) Settings.SetFloat(key, value); }

        public static string GetString(string key, string defaultValue = "")
        {
            if (Settings != null && !Settings.HasString(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in String settings.");
            return Settings != null ? Settings.GetString(key, defaultValue) : defaultValue;
        }
        public static void SetString(string key, string value) { if (Settings != null) Settings.SetString(key, value); }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (Settings != null && !Settings.HasBool(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Bool settings.");
            return Settings != null ? Settings.GetBool(key, defaultValue) : defaultValue;
        }
        public static void SetBool(string key, bool value) { if (Settings != null) Settings.SetBool(key, value); }

        public static Color GetColor(string key, Color? defaultValue = null)
        {
            if (Settings != null && !Settings.HasColor(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Color settings.");
            return Settings != null ? Settings.GetColor(key, defaultValue) : (defaultValue ?? Color.white);
        }
        public static void SetColor(string key, Color value) { if (Settings != null) Settings.SetColor(key, value); }

        public static Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            if (Settings != null && !Settings.HasVector2(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Vector2 settings.");
            return Settings != null ? Settings.GetVector2(key, defaultValue) : defaultValue;
        }
        public static void SetVector2(string key, Vector2 value) { if (Settings != null) Settings.SetVector2(key, value); }

        public static Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            if (Settings != null && !Settings.HasVector3(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Vector3 settings.");
            return Settings != null ? Settings.GetVector3(key, defaultValue) : defaultValue;
        }
        public static void SetVector3(string key, Vector3 value) { if (Settings != null) Settings.SetVector3(key, value); }

        public static AnimationCurve GetCurve(string key, AnimationCurve defaultValue = null)
        {
            if (Settings != null && !Settings.HasCurve(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Curve settings.");
            return Settings != null ? Settings.GetCurve(key, defaultValue) : (defaultValue ?? AnimationCurve.Linear(0, 0, 1, 1));
        }
        public static void SetCurve(string key, AnimationCurve value) { if (Settings != null) Settings.SetCurve(key, value); }

        public static T GetEnum<T>(string key, T defaultValue = default) where T : Enum
        {
            if (Settings != null && !Settings.HasEnum(key))
                Debug.LogError($"[GlobalSettingsManager] Key '{key}' not found in Enum settings.");
            return Settings != null ? Settings.GetEnum(key, defaultValue) : defaultValue;
        }
        public static void SetEnum<T>(string key, T value) where T : Enum { if (Settings != null) Settings.SetEnum(key, value); }

        // ═════════════════════════════════════════════════════════════════════════════
        // RUNTIME UI PANEL
        // ═════════════════════════════════════════════════════════════════════════════

        [Debug]
        /// <summary>
        /// Shows the runtime settings panel (works in Play Mode and Builds).
        /// Creates the panel if it doesn't exist.
        /// </summary>
        public static void ShowRuntimePanel()
        {
            if (_runtimePanel == null)
            {
                CreateRuntimePanel();
            }

            _runtimePanel?.Show();
        }

        /// <summary>
        /// Hides the runtime settings panel.
        /// </summary>
        public static void HideRuntimePanel()
        {
            _runtimePanel?.Hide();
        }

        /// <summary>
        /// Toggles the runtime settings panel visibility.
        /// </summary>
        public static void ToggleRuntimePanel()
        {
            if (_runtimePanel == null)
            {
                ShowRuntimePanel();
            }
            else
            {
                _runtimePanel.Toggle();
            }
        }

        private static void CreateRuntimePanel()
        {
            if (_instance == null)
            {
                Debug.LogError("[GlobalSettingsManager] Manager instance not initialized!");
                return;
            }

            GameObject panelObj = new GameObject("GlobalSettingsRuntimePanel");
            panelObj.transform.SetParent(_instance.transform, false);
            _runtimePanel = panelObj.AddComponent<GlobalSettingsRuntimePanel>();

            Debug.Log("[GlobalSettingsManager] Runtime panel created!");
        }

        void OnDestroy()
        {
            OnAdminPanelSaved?.RemoveAllListeners();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // EDITOR WINDOW TRIGGER (EDITOR ONLY)
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Opens the GlobalSettings editor window (Editor only).
        /// </summary>
        public static void OpenSettingsWindow()
        {
#if UNITY_EDITOR
            // Use reflection to call editor window from runtime assembly
            var editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Alzaki.GlobalSettings.Editor");

            if (editorAssembly != null)
            {
                var windowType = editorAssembly.GetType("Alzaki.GlobalSettings.GlobalSettingsWindow");
                if (windowType != null)
                {
                    var showWindowMethod = windowType.GetMethod("ShowWindow",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    showWindowMethod?.Invoke(null, null);
                }
                else
                {
                    Debug.LogError("[GlobalSettingsManager] Could not find GlobalSettingsWindow type.");
                }
            }
            else
            {
                Debug.LogError("[GlobalSettingsManager] Could not find Alzaki.GlobalSettings.Editor assembly.");
            }
#else
            Debug.LogWarning("[GlobalSettingsManager] Settings window is only available in the Unity Editor.");
#endif
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═════════════════════════════════════════════════════════════════════════════

        private void Awake()
        {
            // Ensure singleton pattern
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
