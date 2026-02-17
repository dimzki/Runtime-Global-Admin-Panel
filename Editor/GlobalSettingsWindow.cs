using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Editor window for viewing and editing GlobalSettings.
    /// Settings are now grouped by SettingsCategory.
    /// </summary>
    public class GlobalSettingsWindow : EditorWindow
    {
        private GlobalSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;

        // ═════════════════════════════════════════════════════════════════════════════
        // WINDOW CREATION
        // ═════════════════════════════════════════════════════════════════════════════

        [MenuItem("Tools/Alzaki/Global Settings", priority = 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<GlobalSettingsWindow>("Global Settings");
            window.minSize = new Vector2(400f, 300f);
            window.Show();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═════════════════════════════════════════════════════════════════════════════

        private void OnEnable()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            _settings = GlobalSettingsManager.Settings;

            if (_settings != null)
            {
                _serializedSettings = new SerializedObject(_settings);
            }
            else
            {
                _serializedSettings = null;
                Debug.LogWarning("[GlobalSettingsWindow] No GlobalSettings found. Please create one in Resources folder.");
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // GUI RENDERING
        // ═════════════════════════════════════════════════════════════════════════════

        private void OnGUI()
        {
            DrawHeader();

            if (_settings == null)
            {
                DrawNoSettingsWarning();
                return;
            }

            _serializedSettings.Update();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                var categoriesProp = _serializedSettings.FindProperty("categories");
                if (categoriesProp != null)
                {
                    EditorGUILayout.PropertyField(categoriesProp, new GUIContent("Categories"), true);
                }
                else
                {
                    EditorGUILayout.HelpBox("Could not find 'categories' property.", MessageType.Error);
                }
            }
            EditorGUILayout.EndScrollView();

            _serializedSettings.ApplyModifiedProperties();

            DrawFooter();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Global Settings", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Refresh", GUILayout.Width(80)))
                    {
                        LoadSettings();
                        Repaint();
                    }

                    if (_settings != null)
                    {
                        if (GUILayout.Button("Select Asset", GUILayout.Width(100)))
                        {
                            Selection.activeObject = _settings;
                            EditorGUIUtility.PingObject(_settings);
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_settings != null)
                {
                    EditorGUILayout.HelpBox($"Asset: {AssetDatabase.GetAssetPath(_settings)}", MessageType.Info);
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);
        }

        private void DrawNoSettingsWarning()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.HelpBox(
                    "No GlobalSettings asset found!\n\n" +
                    "Please create one:\n" +
                    "1. Right-click in Project window\n" +
                    "2. Create > Alzaki > Global Settings\n" +
                    "3. Place it in Assets/Alzaki/Global Settings Tool/Resources/ folder\n" +
                    "4. Rename it to 'GlobalSettings'",
                    MessageType.Error
                );

                GUILayout.Space(10);

                if (GUILayout.Button("Create GlobalSettings Asset", GUILayout.Height(40)))
                {
                    CreateGlobalSettingsAsset();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            GUILayout.Space(5);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    if (_settings != null)
                    {
                        if (GUILayout.Button("Save Asset", GUILayout.Height(30)))
                        {
                            _settings.RefreshDictionaries();
                            EditorUtility.SetDirty(_settings);
                            AssetDatabase.SaveAssets();
                            Debug.Log("[GlobalSettings] Settings saved!");
                        }

                        if (GUILayout.Button("Clear All", GUILayout.Height(30)))
                        {
                            if (EditorUtility.DisplayDialog("Clear All Settings",
                                "Are you sure you want to clear all settings?", "Yes", "Cancel"))
                            {
                                _settings.ClearAll();
                                EditorUtility.SetDirty(_settings);
                                AssetDatabase.SaveAssets();
                                Debug.Log("[GlobalSettings] All settings cleared!");
                            }
                        }
                    }

                    if (GUILayout.Button("Close", GUILayout.Height(30)))
                    {
                        Close();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // UTILITY
        // ═════════════════════════════════════════════════════════════════════════════

        private void CreateGlobalSettingsAsset()
        {
            string folderPath = "Assets/Alzaki/Global Settings Tool/Resources";

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/Alzaki/Global Settings Tool";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    if (!AssetDatabase.IsValidFolder("Assets/Alzaki"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Alzaki");
                    }
                    AssetDatabase.CreateFolder("Assets/Alzaki", "Global Settings Tool");
                }
                AssetDatabase.CreateFolder(parentFolder, "Resources");
            }

            var newSettings = CreateInstance<GlobalSettings>();
            string assetPath = $"{folderPath}/GlobalSettings.asset";
            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _settings = newSettings;
            _serializedSettings = new SerializedObject(_settings);

            Debug.Log($"[GlobalSettings] Created new GlobalSettings asset at: {assetPath}");
            Selection.activeObject = newSettings;
            EditorGUIUtility.PingObject(newSettings);
        }
    }
}
