using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SERIALIZABLE WRAPPERS (Unity can't serialize Dictionary directly)
    // ═══════════════════════════════════════════════════════════════════════════

    [Serializable]
    public class IntSetting
    {
        public string key;
        public int value;
    }

    [Serializable]
    public class FloatSetting
    {
        public string key;
        public float value;
    }

    [Serializable]
    public class StringSetting
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class BoolSetting
    {
        public string key;
        public bool value;
    }

    [Serializable]
    public class ColorSetting
    {
        public string key;
        public Color value = Color.white;
    }

    [Serializable]
    public class Vector2Setting
    {
        public string key;
        public Vector2 value;
    }

    [Serializable]
    public class Vector3Setting
    {
        public string key;
        public Vector3 value;
    }

    [Serializable]
    public class AnimationCurveSetting
    {
        public string key;
        public AnimationCurve value = AnimationCurve.Linear(0, 0, 1, 1);
    }

    /// <summary>
    /// Serializable wrapper for enum values.
    /// Stores the enum type name and value as integer for Unity serialization.
    /// </summary>
    [Serializable]
    public class EnumSetting
    {
        public string key;
        public string enumTypeName;  // Assembly-qualified type name
        public int intValue;         // Enum value as integer

        /// <summary>
        /// Gets the actual enum value using reflection.
        /// </summary>
        public object GetEnumValue()
        {
            if (string.IsNullOrEmpty(enumTypeName)) return null;

            Type enumType = Type.GetType(enumTypeName);
            if (enumType == null || !enumType.IsEnum) return null;

            return Enum.ToObject(enumType, intValue);
        }

        /// <summary>
        /// Gets the enum value as the specified type.
        /// </summary>
        public T GetEnumValue<T>() where T : Enum
        {
            return (T)Enum.ToObject(typeof(T), intValue);
        }

        /// <summary>
        /// Sets the enum value from any enum type.
        /// </summary>
        public void SetEnumValue<T>(T value) where T : Enum
        {
            enumTypeName = typeof(T).AssemblyQualifiedName;
            intValue = Convert.ToInt32(value);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GLOBAL SETTINGS (Odin-Free)
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Global settings container using Unity's native serialization.
    /// Supports common Unity types with inspector editing.
    /// Automatically loaded from Resources folder before scene load.
    /// </summary>
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Alzaki/Global Settings", order = 0)]
    public class GlobalSettings : ScriptableObject
    {
        // ───────────────────────────────────────────────────────────────────────
        // SERIALIZED DATA (Lists for Unity serialization)
        // ───────────────────────────────────────────────────────────────────────

        [SerializeField] private List<IntSetting> intSettings = new List<IntSetting>();
        [SerializeField] private List<FloatSetting> floatSettings = new List<FloatSetting>();
        [SerializeField] private List<StringSetting> stringSettings = new List<StringSetting>();
        [SerializeField] private List<BoolSetting> boolSettings = new List<BoolSetting>();
        [SerializeField] private List<ColorSetting> colorSettings = new List<ColorSetting>();
        [SerializeField] private List<Vector2Setting> vector2Settings = new List<Vector2Setting>();
        [SerializeField] private List<Vector3Setting> vector3Settings = new List<Vector3Setting>();
        [SerializeField] private List<AnimationCurveSetting> curveSettings = new List<AnimationCurveSetting>();
        [SerializeField] private List<EnumSetting> enumSettings = new List<EnumSetting>();

        // ───────────────────────────────────────────────────────────────────────
        // RUNTIME DICTIONARIES (Built from lists for O(1) lookup)
        // ───────────────────────────────────────────────────────────────────────

        private Dictionary<string, int> _intDict;
        private Dictionary<string, float> _floatDict;
        private Dictionary<string, string> _stringDict;
        private Dictionary<string, bool> _boolDict;
        private Dictionary<string, Color> _colorDict;
        private Dictionary<string, Vector2> _vector2Dict;
        private Dictionary<string, Vector3> _vector3Dict;
        private Dictionary<string, AnimationCurve> _curveDict;
        private Dictionary<string, EnumSetting> _enumDict;

        private bool _initialized;

        // ───────────────────────────────────────────────────────────────────────
        // INITIALIZATION
        // ───────────────────────────────────────────────────────────────────────

        private void OnEnable()
        {
            BuildDictionaries();
        }

        private void BuildDictionaries()
        {
            _intDict = new Dictionary<string, int>();
            foreach (var s in intSettings)
                if (!string.IsNullOrEmpty(s.key)) _intDict[s.key] = s.value;

            _floatDict = new Dictionary<string, float>();
            foreach (var s in floatSettings)
                if (!string.IsNullOrEmpty(s.key)) _floatDict[s.key] = s.value;

            _stringDict = new Dictionary<string, string>();
            foreach (var s in stringSettings)
                if (!string.IsNullOrEmpty(s.key)) _stringDict[s.key] = s.value;

            _boolDict = new Dictionary<string, bool>();
            foreach (var s in boolSettings)
                if (!string.IsNullOrEmpty(s.key)) _boolDict[s.key] = s.value;

            _colorDict = new Dictionary<string, Color>();
            foreach (var s in colorSettings)
                if (!string.IsNullOrEmpty(s.key)) _colorDict[s.key] = s.value;

            _vector2Dict = new Dictionary<string, Vector2>();
            foreach (var s in vector2Settings)
                if (!string.IsNullOrEmpty(s.key)) _vector2Dict[s.key] = s.value;

            _vector3Dict = new Dictionary<string, Vector3>();
            foreach (var s in vector3Settings)
                if (!string.IsNullOrEmpty(s.key)) _vector3Dict[s.key] = s.value;

            _curveDict = new Dictionary<string, AnimationCurve>();
            foreach (var s in curveSettings)
                if (!string.IsNullOrEmpty(s.key)) _curveDict[s.key] = s.value;

            _enumDict = new Dictionary<string, EnumSetting>();
            foreach (var s in enumSettings)
                if (!string.IsNullOrEmpty(s.key)) _enumDict[s.key] = s;

            _initialized = true;
        }

        private void EnsureInitialized()
        {
            if (!_initialized) BuildDictionaries();
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // INT SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public int GetInt(string key, int defaultValue = 0)
        {
            EnsureInitialized();
            return _intDict.TryGetValue(key, out int value) ? value : defaultValue;
        }

        public void SetInt(string key, int value)
        {
            EnsureInitialized();
            _intDict[key] = value;
            SyncIntToList(key, value);
        }

        public bool HasInt(string key)
        {
            EnsureInitialized();
            return _intDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // FLOAT SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public float GetFloat(string key, float defaultValue = 0f)
        {
            EnsureInitialized();
            return _floatDict.TryGetValue(key, out float value) ? value : defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            EnsureInitialized();
            _floatDict[key] = value;
            SyncFloatToList(key, value);
        }

        public bool HasFloat(string key)
        {
            EnsureInitialized();
            return _floatDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // STRING SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public string GetString(string key, string defaultValue = "")
        {
            EnsureInitialized();
            return _stringDict.TryGetValue(key, out string value) ? value : defaultValue;
        }

        public void SetString(string key, string value)
        {
            EnsureInitialized();
            _stringDict[key] = value;
            SyncStringToList(key, value);
        }

        public bool HasString(string key)
        {
            EnsureInitialized();
            return _stringDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // BOOL SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public bool GetBool(string key, bool defaultValue = false)
        {
            EnsureInitialized();
            return _boolDict.TryGetValue(key, out bool value) ? value : defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            EnsureInitialized();
            _boolDict[key] = value;
            SyncBoolToList(key, value);
        }

        public bool HasBool(string key)
        {
            EnsureInitialized();
            return _boolDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // COLOR SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public Color GetColor(string key, Color? defaultValue = null)
        {
            EnsureInitialized();
            return _colorDict.TryGetValue(key, out Color value) ? value : (defaultValue ?? Color.white);
        }

        public void SetColor(string key, Color value)
        {
            EnsureInitialized();
            _colorDict[key] = value;
            SyncColorToList(key, value);
        }

        public bool HasColor(string key)
        {
            EnsureInitialized();
            return _colorDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // VECTOR2 SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public Vector2 GetVector2(string key, Vector2 defaultValue = default)
        {
            EnsureInitialized();
            return _vector2Dict.TryGetValue(key, out Vector2 value) ? value : defaultValue;
        }

        public void SetVector2(string key, Vector2 value)
        {
            EnsureInitialized();
            _vector2Dict[key] = value;
            SyncVector2ToList(key, value);
        }

        public bool HasVector2(string key)
        {
            EnsureInitialized();
            return _vector2Dict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // VECTOR3 SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public Vector3 GetVector3(string key, Vector3 defaultValue = default)
        {
            EnsureInitialized();
            return _vector3Dict.TryGetValue(key, out Vector3 value) ? value : defaultValue;
        }

        public void SetVector3(string key, Vector3 value)
        {
            EnsureInitialized();
            _vector3Dict[key] = value;
            SyncVector3ToList(key, value);
        }

        public bool HasVector3(string key)
        {
            EnsureInitialized();
            return _vector3Dict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ANIMATION CURVE SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        public AnimationCurve GetCurve(string key, AnimationCurve defaultValue = null)
        {
            EnsureInitialized();
            return _curveDict.TryGetValue(key, out AnimationCurve value) ? value : (defaultValue ?? AnimationCurve.Linear(0, 0, 1, 1));
        }

        public void SetCurve(string key, AnimationCurve value)
        {
            EnsureInitialized();
            _curveDict[key] = value;
            SyncCurveToList(key, value);
        }

        public bool HasCurve(string key)
        {
            EnsureInitialized();
            return _curveDict.ContainsKey(key);
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // ENUM SETTINGS
        // ═══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Gets an enum value by key.
        /// </summary>
        public T GetEnum<T>(string key, T defaultValue = default) where T : Enum
        {
            EnsureInitialized();
            if (_enumDict.TryGetValue(key, out EnumSetting setting))
            {
                return setting.GetEnumValue<T>();
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets an enum value by key.
        /// </summary>
        public void SetEnum<T>(string key, T value) where T : Enum
        {
            EnsureInitialized();

            if (_enumDict.TryGetValue(key, out EnumSetting setting))
            {
                setting.SetEnumValue(value);
            }
            else
            {
                var newSetting = new EnumSetting { key = key };
                newSetting.SetEnumValue(value);
                _enumDict[key] = newSetting;
            }

            SyncEnumToList(key, value);
        }

        /// <summary>
        /// Checks if an enum setting exists.
        /// </summary>
        public bool HasEnum(string key)
        {
            EnsureInitialized();
            return _enumDict.ContainsKey(key);
        }

        /// <summary>
        /// Gets the raw EnumSetting for advanced usage (e.g., runtime panel).
        /// </summary>
        public EnumSetting GetEnumSetting(string key)
        {
            EnsureInitialized();
            return _enumDict.TryGetValue(key, out EnumSetting setting) ? setting : null;
        }

        /// <summary>
        /// Sets an enum value from an EnumSetting object (non-generic, for runtime panel).
        /// </summary>
        public void SetEnumSetting(EnumSetting setting)
        {
            if (setting == null || string.IsNullOrEmpty(setting.key)) return;

            EnsureInitialized();

            if (_enumDict.TryGetValue(setting.key, out EnumSetting existing))
            {
                existing.enumTypeName = setting.enumTypeName;
                existing.intValue = setting.intValue;
            }
            else
            {
                var newSetting = new EnumSetting
                {
                    key = setting.key,
                    enumTypeName = setting.enumTypeName,
                    intValue = setting.intValue
                };
                _enumDict[setting.key] = newSetting;
                enumSettings.Add(newSetting);
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // UTILITY
        // ═══════════════════════════════════════════════════════════════════════════

        public void ClearAll()
        {
            intSettings.Clear();
            floatSettings.Clear();
            stringSettings.Clear();
            boolSettings.Clear();
            colorSettings.Clear();
            vector2Settings.Clear();
            vector3Settings.Clear();
            curveSettings.Clear();
            enumSettings.Clear();
            BuildDictionaries();
        }

        /// <summary>
        /// Call this after modifying settings in the editor to rebuild runtime dictionaries.
        /// </summary>
        public void RefreshDictionaries()
        {
            BuildDictionaries();
        }

        // ───────────────────────────────────────────────────────────────────────
        // SYNC HELPERS (Keep list and dictionary in sync at runtime)
        // ───────────────────────────────────────────────────────────────────────

        private void SyncIntToList(string key, int value)
        {
            var existing = intSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else intSettings.Add(new IntSetting { key = key, value = value });
        }

        private void SyncFloatToList(string key, float value)
        {
            var existing = floatSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else floatSettings.Add(new FloatSetting { key = key, value = value });
        }

        private void SyncStringToList(string key, string value)
        {
            var existing = stringSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else stringSettings.Add(new StringSetting { key = key, value = value });
        }

        private void SyncBoolToList(string key, bool value)
        {
            var existing = boolSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else boolSettings.Add(new BoolSetting { key = key, value = value });
        }

        private void SyncColorToList(string key, Color value)
        {
            var existing = colorSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else colorSettings.Add(new ColorSetting { key = key, value = value });
        }

        private void SyncVector2ToList(string key, Vector2 value)
        {
            var existing = vector2Settings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else vector2Settings.Add(new Vector2Setting { key = key, value = value });
        }

        private void SyncVector3ToList(string key, Vector3 value)
        {
            var existing = vector3Settings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else vector3Settings.Add(new Vector3Setting { key = key, value = value });
        }

        private void SyncCurveToList(string key, AnimationCurve value)
        {
            var existing = curveSettings.Find(s => s.key == key);
            if (existing != null) existing.value = value;
            else curveSettings.Add(new AnimationCurveSetting { key = key, value = value });
        }

        private void SyncEnumToList<T>(string key, T value) where T : Enum
        {
            var existing = enumSettings.Find(s => s.key == key);
            if (existing != null)
            {
                existing.SetEnumValue(value);
            }
            else
            {
                var newSetting = new EnumSetting { key = key };
                newSetting.SetEnumValue(value);
                enumSettings.Add(newSetting);
            }
        }
    }
}
