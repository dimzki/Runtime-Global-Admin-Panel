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
        public bool useIndependentScriptableObject = false;
        public SettingsCategoryScriptableObject independentScriptableObject;


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
        [SerializeField] private bool HasPassword = false;
        [SerializeField] private string Password = "0000";

        public bool GetHasPassword() => HasPassword;
        public string GetPassword() => Password;
        public void SetPassword(string newPassword) => Password = newPassword;

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
                var intList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.intSettings : cat.intSettings;
                var floatList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.floatSettings : cat.floatSettings;
                var stringList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.stringSettings : cat.stringSettings;
                var boolList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.boolSettings : cat.boolSettings;
                var colorList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.colorSettings : cat.colorSettings;
                var vector2List = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.vector2Settings : cat.vector2Settings;
                var vector3List = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.vector3Settings : cat.vector3Settings;
                var curveList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.curveSettings : cat.curveSettings;
                var enumList = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.enumSettings : cat.enumSettings;

                foreach (var s in intList) if (!string.IsNullOrEmpty(s.key)) _intDict[s.key] = s.value;
                foreach (var s in floatList) if (!string.IsNullOrEmpty(s.key)) _floatDict[s.key] = s.value;
                foreach (var s in stringList) if (!string.IsNullOrEmpty(s.key)) _stringDict[s.key] = s.value;
                foreach (var s in boolList) if (!string.IsNullOrEmpty(s.key)) _boolDict[s.key] = s.value;
                foreach (var s in colorList) if (!string.IsNullOrEmpty(s.key)) _colorDict[s.key] = s.value;
                foreach (var s in vector2List) if (!string.IsNullOrEmpty(s.key)) _vector2Dict[s.key] = s.value;
                foreach (var s in vector3List) if (!string.IsNullOrEmpty(s.key)) _vector3Dict[s.key] = s.value;
                foreach (var s in curveList) if (!string.IsNullOrEmpty(s.key)) _curveDict[s.key] = s.value;
                foreach (var s in enumList) if (!string.IsNullOrEmpty(s.key)) _enumDict[s.key] = s;
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

        private void MarkDirtyIfIndependent(SettingsCategory cat)
        {
#if UNITY_EDITOR
            if (cat.useIndependentScriptableObject && cat.independentScriptableObject != null)
                UnityEditor.EditorUtility.SetDirty(cat.independentScriptableObject);
#endif
        }

        private void SyncIntToList(string key, int value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.intSettings : cat.intSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.intSettings : defaultCat.intSettings; targetList.Add(new IntSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncFloatToList(string key, float value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.floatSettings : cat.floatSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.floatSettings : defaultCat.floatSettings; targetList.Add(new FloatSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncStringToList(string key, string value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.stringSettings : cat.stringSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.stringSettings : defaultCat.stringSettings; targetList.Add(new StringSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncBoolToList(string key, bool value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.boolSettings : cat.boolSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.boolSettings : defaultCat.boolSettings; targetList.Add(new BoolSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncColorToList(string key, Color value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.colorSettings : cat.colorSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.colorSettings : defaultCat.colorSettings; targetList.Add(new ColorSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncVector2ToList(string key, Vector2 value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.vector2Settings : cat.vector2Settings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.vector2Settings : defaultCat.vector2Settings; targetList.Add(new Vector2Setting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncVector3ToList(string key, Vector3 value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.vector3Settings : cat.vector3Settings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.vector3Settings : defaultCat.vector3Settings; targetList.Add(new Vector3Setting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncCurveToList(string key, AnimationCurve value) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.curveSettings : cat.curveSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.value = value; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.curveSettings : defaultCat.curveSettings; targetList.Add(new AnimationCurveSetting { key = key, value = value }); MarkDirtyIfIndependent(defaultCat); }
        private void SyncEnumToList<T>(string key, T value) where T : Enum { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.enumSettings : cat.enumSettings; var existing = list.Find(s => s.key == key); if (existing != null) { existing.SetEnumValue(value); MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.enumSettings : defaultCat.enumSettings; var newSetting = new EnumSetting { key = key }; newSetting.SetEnumValue(value); targetList.Add(newSetting); MarkDirtyIfIndependent(defaultCat); }
        private void SyncEnumSettingToList(EnumSetting setting) { foreach (var cat in categories) { var list = cat.useIndependentScriptableObject && cat.independentScriptableObject != null ? cat.independentScriptableObject.enumSettings : cat.enumSettings; var existing = list.Find(s => s.key == setting.key); if (existing != null) { existing.enumTypeName = setting.enumTypeName; existing.intValue = setting.intValue; MarkDirtyIfIndependent(cat); return; } } var defaultCat = GetOrCreateDefaultCategory(); var targetList = defaultCat.useIndependentScriptableObject && defaultCat.independentScriptableObject != null ? defaultCat.independentScriptableObject.enumSettings : defaultCat.enumSettings; targetList.Add(setting); MarkDirtyIfIndependent(defaultCat); }
    }
}