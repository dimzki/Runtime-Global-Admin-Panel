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
        private static bool _isQuitting;

        private const string SETTINGS_RESOURCE_PATH = "GlobalSettings";

        // ═════════════════════════════════════════════════════════════════════════════
        // SINGLETON INSTANCE
        // ═════════════════════════════════════════════════════════════════════════════

        public static GlobalSettingsManager Instance
        {
            get
            {
                if (_isQuitting)
                {
                    return null;
                }

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
                if (_isQuitting)
                {
                    return _settings;
                }

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
            _isQuitting = false;

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
                LoadFromDisk();
            }
        }

        public static void SaveToDisk()
        {
            if (_settings == null) return;

            try
            {
                string json = JsonUtility.ToJson(_settings);
                string path = System.IO.Path.Combine(Application.persistentDataPath, "GlobalSettingsSave.json");
                System.IO.File.WriteAllText(path, json);
                Debug.Log($"[GlobalSettingsManager] Saved settings to disk: {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[GlobalSettingsManager] Failed to save settings to disk: {e.Message}");
            }
        }

        public static void LoadFromDisk()
        {
            if (_settings == null) return;

            string path = System.IO.Path.Combine(Application.persistentDataPath, "GlobalSettingsSave.json");
            if (System.IO.File.Exists(path))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(path);

                    // Create a temporary instance to deserialize the saved JSON without destroying the fresh build asset structure
                    GlobalSettings savedData = ScriptableObject.CreateInstance<GlobalSettings>();
                    JsonUtility.FromJsonOverwrite(json, savedData);
                    savedData.RefreshDictionaries();

                    // Iterate through the fresh build asset categories and only apply values for matching keys
                    foreach (var cat in _settings.categories)
                    {
                        foreach (var s in cat.intSettings) { if (savedData.HasInt(s.key)) s.value = savedData.GetInt(s.key); }
                        foreach (var s in cat.floatSettings) { if (savedData.HasFloat(s.key)) s.value = savedData.GetFloat(s.key); }
                        foreach (var s in cat.stringSettings) { if (savedData.HasString(s.key)) s.value = savedData.GetString(s.key); }
                        foreach (var s in cat.boolSettings) { if (savedData.HasBool(s.key)) s.value = savedData.GetBool(s.key); }
                        foreach (var s in cat.colorSettings) { if (savedData.HasColor(s.key)) s.value = savedData.GetColor(s.key); }
                        foreach (var s in cat.vector2Settings) { if (savedData.HasVector2(s.key)) s.value = savedData.GetVector2(s.key); }
                        foreach (var s in cat.vector3Settings) { if (savedData.HasVector3(s.key)) s.value = savedData.GetVector3(s.key); }
                        foreach (var s in cat.curveSettings) { if (savedData.HasCurve(s.key)) s.value = savedData.GetCurve(s.key); }
                        foreach (var s in cat.enumSettings)
                        {
                            if (savedData.HasEnum(s.key))
                            {
                                var savedEnum = savedData.GetEnumSetting(s.key);
                                if (savedEnum != null && savedEnum.enumTypeName == s.enumTypeName)
                                {
                                    s.intValue = savedEnum.intValue;
                                }
                            }
                        }
                    }

                    _settings.RefreshDictionaries();

                    if (Application.isPlaying)
                    {
                        Destroy(savedData);
                    }
                    else
                    {
                        DestroyImmediate(savedData);
                    }

                    Debug.Log($"[GlobalSettingsManager] Loaded settings from disk (matching keys only): {path}");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GlobalSettingsManager] Failed to load settings from disk: {e.Message}");
                }
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // CONVENIENCE METHODS
        // ═════════════════════════════════════════════════════════════════════════════

        public static int GetInt(string key, int defaultValue = 0) => Settings?.GetInt(key, defaultValue) ?? defaultValue;
        public static void SetInt(string key, int value) => Settings?.SetInt(key, value);

        public static float GetFloat(string key, float defaultValue = 0f) => Settings?.GetFloat(key, defaultValue) ?? defaultValue;
        public static void SetFloat(string key, float value) => Settings?.SetFloat(key, value);

        public static string GetString(string key, string defaultValue = "") => Settings?.GetString(key, defaultValue) ?? defaultValue;
        public static void SetString(string key, string value) => Settings?.SetString(key, value);

        public static bool GetBool(string key, bool defaultValue = false) => Settings?.GetBool(key, defaultValue) ?? defaultValue;
        public static void SetBool(string key, bool value) => Settings?.SetBool(key, value);

        public static Color GetColor(string key, Color? defaultValue = null) => Settings?.GetColor(key, defaultValue) ?? Color.white;
        public static void SetColor(string key, Color value) => Settings?.SetColor(key, value);

        public static Vector2 GetVector2(string key, Vector2 defaultValue = default) => Settings?.GetVector2(key, defaultValue) ?? defaultValue;
        public static void SetVector2(string key, Vector2 value) => Settings?.SetVector2(key, value);

        public static Vector3 GetVector3(string key, Vector3 defaultValue = default) => Settings?.GetVector3(key, defaultValue) ?? defaultValue;
        public static void SetVector3(string key, Vector3 value) => Settings?.SetVector3(key, value);

        public static AnimationCurve GetCurve(string key, AnimationCurve defaultValue = null) => Settings?.GetCurve(key, defaultValue) ?? AnimationCurve.Linear(0, 0, 1, 1);
        public static void SetCurve(string key, AnimationCurve value) => Settings?.SetCurve(key, value);

        public static T GetEnum<T>(string key, T defaultValue = default) where T : System.Enum => Settings != null ? Settings.GetEnum(key, defaultValue) : defaultValue;
        public static void SetEnum<T>(string key, T value) where T : System.Enum => Settings?.SetEnum(key, value);

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

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            _isQuitting = true;
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
