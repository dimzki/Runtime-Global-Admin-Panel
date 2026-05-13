using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    public class GlobalSettingsWindow : EditorWindow
    {
        private GlobalSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;

        [MenuItem("Tools/Alzaki/Global Settings", priority = 0)]
        public static void ShowWindow() { GetWindow<GlobalSettingsWindow>("Global Settings").Show(); }

        private void OnEnable() { LoadSettings(); }

        private void LoadSettings()
        {
            _settings = GlobalSettingsManager.Settings;
            if (_settings != null) _serializedSettings = new SerializedObject(_settings);
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                EditorGUILayout.HelpBox("No GlobalSettings asset found in Resources folder.", MessageType.Error);
                if (GUILayout.Button("Refresh")) LoadSettings();
                return;
            }

            _serializedSettings.Update();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            EditorGUILayout.PropertyField(_serializedSettings.FindProperty("categories"), true);
            EditorGUILayout.EndScrollView();
            _serializedSettings.ApplyModifiedProperties();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) { _settings.RefreshDictionaries(); EditorUtility.SetDirty(_settings); AssetDatabase.SaveAssets(); }
            if (GUILayout.Button("Clear All")) { _settings.ClearAll(); EditorUtility.SetDirty(_settings); }
            EditorGUILayout.EndHorizontal();
        }
    }
}