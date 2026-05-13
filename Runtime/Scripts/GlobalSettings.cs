using System;
using System.Collections.Generic;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    // ═══════════════════════════════════════════════════════════════════════════
    // SERIALIZABLE WRAPPERS
    // ═══════════════════════════════════════════════════════════════════════════

    [Serializable]
    public class IntSetting { public string key; public int value; }

    [Serializable]
    public class FloatSetting { public string key; public float value; }

    [Serializable]
    public class StringSetting { public string key; public string value; }

    [Serializable]
    public class BoolSetting { public string key; public bool value; }

    [Serializable]
    public class ColorSetting { public string key; public Color value = Color.white; }

    [Serializable]
    public class Vector2Setting { public string key; public Vector2 value; }

    [Serializable]
    public class Vector3Setting { public string key; public Vector3 value; }

    [Serializable]
    public class AnimationCurveSetting { public string key; public AnimationCurve value = AnimationCurve.Linear(0, 0, 1, 1); }

    [Serializable]
    public class EnumSetting
    {
        public string key;
        public string enumTypeName;
        public int intValue;

        public object GetEnumValue()
        {
            if (string.IsNullOrEmpty(enumTypeName)) return null;
            Type enumType = Type.GetType(enumTypeName);
            if (enumType == null || !enumType.IsEnum) return null;
            return Enum.ToObject(enumType, intValue);
        }

        public T GetEnumValue<T>() where T : Enum { return (T)Enum.ToObject(typeof(T), intValue); }
        public void SetEnumValue<T>(T value) where T : Enum { enumTypeName = typeof(T).AssemblyQualifiedName; intValue = Convert.ToInt32(value); }
    }

    [Serializable]
    public class SettingsCategory
    {
        public string categoryName = "New Category";
        public List<IntSetting> intSettings = new List<IntSetting>();
        public List<FloatSetting> floatSettings = new List<FloatSetting>();
        public List<StringSetting> stringSettings = new List<StringSetting>();
        public List<BoolSetting> boolSettings = new List<BoolSetting>();
        public List<ColorSetting> colorSettings = new List<ColorSetting>();
        public List<Vector2Setting> vector2Settings = new List<Vector2Setting>();
        public List<Vector3Setting> vector3Settings = new List<Vector3Setting>();
        public List<AnimationCurveSetting> curveSettings = new List<AnimationCurveSetting>();
        public List<EnumSetting> enumSettings = new List<EnumSetting>();
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // GLOBAL SETTINGS
    // ═══════════════════════════════════════════════════════════════════════════

    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Alzaki/Global Settings", order = 0)]
    public class GlobalSettings : ScriptableObject
    {
        [SerializeField] public List<SettingsCategory> categories = new List<SettingsCategory>();

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

        private void OnEnable() { BuildDictionaries(); }

        private void BuildDictionaries()
        {
            _intDict = new Dictionary<string, int>();
            _floatDict = new Dictionary<string, float>();
            _stringDict = new Dictionary<string, string>();
            _boolDict = new Dictionary<string, bool>();
            _colorDict = new Dictionary<string, Color>();
            _vector2Dict = new Dictionary<string, Vector2>();
            _vector3Dict = new Dictionary<string, Vector3>();
            _curveDict = new Dictionary<string, AnimationCurve>();
            _enumDict = new Dictionary<string, EnumSetting>();

            foreach (var cat in categories)
            {
                foreach (var s in cat.intSettings) if (!string.IsNullOrEmpty(s.key)) _intDict[s.key] = s.value;
                foreach (var s in cat.floatSettings) if (!string.IsNullOrEmpty(s.key)) _floatDict[s.key] = s.value;
                foreach (var s in cat.stringSettings) if (!string.IsNullOrEmpty(s.key)) _stringDict[s.key] = s.value;
                foreach (var s in cat.boolSettings) if (!string.IsNullOrEmpty(s.key)) _boolDict[s.key] = s.value;
                foreach (var s in cat.colorSettings) if (!string.IsNullOrEmpty(s.key)) _colorDict[s.key] = s.value;
                foreach (var s in cat.vector2Settings) if (!string.IsNullOrEmpty(s.key)) _vector2Dict[s.key] = s.value;
                foreach (var s in cat.vector3Settings) if (!string.IsNullOrEmpty(s.key)) _vector3Dict[s.key] = s.value;
                foreach (var s in cat.curveSettings) if (!string.IsNullOrEmpty(s.key)) _curveDict[s.key] = s.value;
                foreach (var s in cat.enumSettings) if (!string.IsNullOrEmpty(s.key)) _enumDict[s.key] = s;
            }

            _initialized = true;
        }

        private void EnsureInitialized() { if (!_initialized) BuildDictionaries(); }

        public int GetInt(string key, int defaultValue = 0) { EnsureInitialized(); return _intDict.TryGetValue(key, out int value) ? value : defaultValue; }
        public void SetInt(string key, int value) { EnsureInitialized(); _intDict[key] = value; SyncIntToList(key, value); }
        public bool HasInt(string key) { EnsureInitialized(); return _intDict.ContainsKey(key); }

        public float GetFloat(string key, float defaultValue = 0f) { EnsureInitialized(); return _floatDict.TryGetValue(key, out float value) ? value : defaultValue; }
        public void SetFloat(string key, float value) { EnsureInitialized(); _floatDict[key] = value; SyncFloatToList(key, value); }
        public bool HasFloat(string key) { EnsureInitialized(); return _floatDict.ContainsKey(key); }

        public string GetString(string key, string defaultValue = "") { EnsureInitialized(); return _stringDict.TryGetValue(key, out string value) ? value : defaultValue; }
        public void SetString(string key, string value) { EnsureInitialized(); _stringDict[key] = value; SyncStringToList(key, value); }
        public bool HasString(string key) { EnsureInitialized(); return _stringDict.ContainsKey(key); }

        public bool GetBool(string key, bool defaultValue = false) { EnsureInitialized(); return _boolDict.TryGetValue(key, out bool value) ? value : defaultValue; }
        public void SetBool(string key, bool value) { EnsureInitialized(); _boolDict[key] = value; SyncBoolToList(key, value); }
        public bool HasBool(string key) { EnsureInitialized(); return _boolDict.ContainsKey(key); }

        public Color GetColor(string key, Color? defaultValue = null) { EnsureInitialized(); return _colorDict.TryGetValue(key, out Color value) ? value : (defaultValue ?? Color.white); }
        public void SetColor(string key, Color value) { EnsureInitialized(); _colorDict[key] = value; SyncColorToList(key, value); }
        public bool HasColor(string key) { EnsureInitialized(); return _colorDict.ContainsKey(key); }

        public Vector2 GetVector2(string key, Vector2 defaultValue = default) { EnsureInitialized(); return _vector2Dict.TryGetValue(key, out Vector2 value) ? value : defaultValue; }
        public void SetVector2(string key, Vector2 value) { EnsureInitialized(); _vector2Dict[key] = value; SyncVector2ToList(key, value); }
        public bool HasVector2(string key) { EnsureInitialized(); return _vector2Dict.ContainsKey(key); }

        public Vector3 GetVector3(string key, Vector3 defaultValue = default) { EnsureInitialized(); return _vector3Dict.TryGetValue(key, out Vector3 value) ? value : defaultValue; }
        public void SetVector3(string key, Vector3 value) { EnsureInitialized(); _vector3Dict[key] = value; SyncVector3ToList(key, value); }
        public bool HasVector3(string key) { EnsureInitialized(); return _vector3Dict.ContainsKey(key); }

        public AnimationCurve GetCurve(string key, AnimationCurve defaultValue = null) { EnsureInitialized(); return _curveDict.TryGetValue(key, out AnimationCurve value) ? value : (defaultValue ?? AnimationCurve.Linear(0, 0, 1, 1)); }
        public void SetCurve(string key, AnimationCurve value) { EnsureInitialized(); _curveDict[key] = value; SyncCurveToList(key, value); }
        public bool HasCurve(string key) { EnsureInitialized(); return _curveDict.ContainsKey(key); }

        public T GetEnum<T>(string key, T defaultValue = default) where T : Enum { EnsureInitialized(); if (_enumDict.TryGetValue(key, out EnumSetting setting)) return setting.GetEnumValue<T>(); return defaultValue; }
        public void SetEnum<T>(string key, T value) where T : Enum { EnsureInitialized(); if (_enumDict.TryGetValue(key, out EnumSetting setting)) { setting.SetEnumValue(value); } else { var newSetting = new EnumSetting { key = key }; newSetting.SetEnumValue(value); _enumDict[key] = newSetting; } SyncEnumToList(key, value); }
        public bool HasEnum(string key) { EnsureInitialized(); return _enumDict.ContainsKey(key); }
        
        public EnumSetting GetEnumSetting(string key) { EnsureInitialized(); return _enumDict.TryGetValue(key, out EnumSetting setting) ? setting : null; }
        public void SetEnumSetting(EnumSetting setting) { 
            if (setting == null || string.IsNullOrEmpty(setting.key)) return; 
            EnsureInitialized(); 
            if (_enumDict.TryGetValue(setting.key, out EnumSetting existing)) { existing.enumTypeName = setting.enumTypeName; existing.intValue = setting.intValue; } 
            else { var newSetting = new EnumSetting { key = setting.key, enumTypeName = setting.enumTypeName, intValue = setting.intValue }; _enumDict[setting.key] = newSetting; SyncEnumSettingToList(newSetting); } 
        }

        public void ClearAll() { categories.Clear(); BuildDictionaries(); }
        public void RefreshDictionaries() { BuildDictionaries(); }

        private SettingsCategory GetOrCreateDefaultCategory() { if (categories.Count == 0) categories.Add(new SettingsCategory { categoryName = "Generic" }); return categories[0]; }

        private void SyncIntToList(string key, int value) { foreach (var cat in categories) { var existing = cat.intSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().intSettings.Add(new IntSetting { key = key, value = value }); }
        private void SyncFloatToList(string key, float value) { foreach (var cat in categories) { var existing = cat.floatSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().floatSettings.Add(new FloatSetting { key = key, value = value }); }
        private void SyncStringToList(string key, string value) { foreach (var cat in categories) { var existing = cat.stringSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().stringSettings.Add(new StringSetting { key = key, value = value }); }
        private void SyncBoolToList(string key, bool value) { foreach (var cat in categories) { var existing = cat.boolSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().boolSettings.Add(new BoolSetting { key = key, value = value }); }
        private void SyncColorToList(string key, Color value) { foreach (var cat in categories) { var existing = cat.colorSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().colorSettings.Add(new ColorSetting { key = key, value = value }); }
        private void SyncVector2ToList(string key, Vector2 value) { foreach (var cat in categories) { var existing = cat.vector2Settings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().vector2Settings.Add(new Vector2Setting { key = key, value = value }); }
        private void SyncVector3ToList(string key, Vector3 value) { foreach (var cat in categories) { var existing = cat.vector3Settings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().vector3Settings.Add(new Vector3Setting { key = key, value = value }); }
        private void SyncCurveToList(string key, AnimationCurve value) { foreach (var cat in categories) { var existing = cat.curveSettings.Find(s => s.key == key); if (existing != null) { existing.value = value; return; } } GetOrCreateDefaultCategory().curveSettings.Add(new AnimationCurveSetting { key = key, value = value }); }
        private void SyncEnumToList<T>(string key, T value) where T : Enum { foreach (var cat in categories) { var existing = cat.enumSettings.Find(s => s.key == key); if (existing != null) { existing.SetEnumValue(value); return; } } var newSetting = new EnumSetting { key = key }; newSetting.SetEnumValue(value); GetOrCreateDefaultCategory().enumSettings.Add(newSetting); }
        private void SyncEnumSettingToList(EnumSetting setting) { foreach (var cat in categories) { var existing = cat.enumSettings.Find(s => s.key == setting.key); if (existing != null) { existing.enumTypeName = setting.enumTypeName; existing.intValue = setting.intValue; return; } } GetOrCreateDefaultCategory().enumSettings.Add(setting); }
    }
}