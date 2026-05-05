using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Editor window for viewing and editing GlobalSettings.
    /// Odin-free implementation using native Unity EditorGUI.
    /// </summary>
    public class GlobalSettingsWindow : EditorWindow
    {
        private GlobalSettings _settings;
        private SerializedObject _serializedSettings;
        private Vector2 _scrollPosition;

        // Foldout states
        private bool _foldoutInt = true;
        private bool _foldoutFloat = true;
        private bool _foldoutString = true;
        private bool _foldoutBool = true;
        private bool _foldoutColor = true;
        private bool _foldoutVector2 = true;
        private bool _foldoutVector3 = true;
        private bool _foldoutCurve = true;
        private bool _foldoutEnum = true;

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
                DrawSettingsList("intSettings", "Integer Settings", ref _foldoutInt);
                DrawSettingsList("floatSettings", "Float Settings", ref _foldoutFloat);
                DrawSettingsList("stringSettings", "String Settings", ref _foldoutString);
                DrawSettingsList("boolSettings", "Bool Settings", ref _foldoutBool);
                DrawSettingsList("colorSettings", "Color Settings", ref _foldoutColor);
                DrawSettingsList("vector2Settings", "Vector2 Settings", ref _foldoutVector2);
                DrawSettingsList("vector3Settings", "Vector3 Settings", ref _foldoutVector3);
                DrawSettingsList("curveSettings", "Curve Settings", ref _foldoutCurve);
                DrawEnumSettingsList(ref _foldoutEnum);
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

        private void DrawSettingsList(string propertyName, string label, ref bool foldout)
        {
            var property = _serializedSettings.FindProperty(propertyName);
            if (property == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    foldout = EditorGUILayout.Foldout(foldout, $"{label} ({property.arraySize})", true, EditorStyles.foldoutHeader);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (foldout)
                {
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("key");
                        var valueProp = element.FindPropertyRelative("value");

                        EditorGUILayout.BeginHorizontal();
                        {
                            // Key field
                            EditorGUILayout.PropertyField(keyProp, GUIContent.none, GUILayout.Width(150));

                            // Value field
                            EditorGUILayout.PropertyField(valueProp, GUIContent.none);

                            // Delete button
                            if (GUILayout.Button("-", GUILayout.Width(25)))
                            {
                                property.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
        }

        private void DrawEnumSettingsList(ref bool foldout)
        {
            var property = _serializedSettings.FindProperty("enumSettings");
            if (property == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    foldout = EditorGUILayout.Foldout(foldout, $"Enum Settings ({property.arraySize})", true, EditorStyles.foldoutHeader);

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("+", GUILayout.Width(25)))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (foldout)
                {
                    EditorGUI.indentLevel++;

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("key");
                        var enumTypeProp = element.FindPropertyRelative("enumTypeName");
                        var intValueProp = element.FindPropertyRelative("intValue");

                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Key", GUILayout.Width(40));
                                EditorGUILayout.PropertyField(keyProp, GUIContent.none);

                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    property.DeleteArrayElementAtIndex(i);
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            // Try to resolve the enum type and show a proper dropdown
                            string enumTypeName = enumTypeProp.stringValue;
                            System.Type enumType = null;

                            if (!string.IsNullOrEmpty(enumTypeName))
                            {
                                enumType = System.Type.GetType(enumTypeName);
                            }

                            // Type selector row
                            EditorGUILayout.BeginHorizontal();
                            {
                                EditorGUILayout.LabelField("Type", GUILayout.Width(40));

                                // Display current type name or "None"
                                string displayTypeName = enumType != null ? $"{enumType.Name} (Enum)" : "None (Enum)";

                                // Capture the property for the closure
                                var capturedEnumTypeProp = enumTypeProp;
                                var capturedIntValueProp = intValueProp;

                                // Type selector button with dropdown icon
                                Rect buttonRect = GUILayoutUtility.GetRect(new GUIContent(displayTypeName), EditorStyles.popup);
                                if (GUI.Button(buttonRect, displayTypeName, EditorStyles.popup))
                                {
                                    ShowEnumTypeMenu(buttonRect, capturedEnumTypeProp, capturedIntValueProp);
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            // Value selector (only if type is valid)
                            if (enumType != null && enumType.IsEnum)
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.LabelField("Value", GUILayout.Width(40));
                                    var currentValue = (System.Enum)System.Enum.ToObject(enumType, intValueProp.intValue);
                                    var newValue = EditorGUILayout.EnumPopup(currentValue);
                                    intValueProp.intValue = System.Convert.ToInt32(newValue);
                                }
                                EditorGUILayout.EndHorizontal();
                            }
                            else if (!string.IsNullOrEmpty(enumTypeName))
                            {
                                EditorGUILayout.HelpBox($"Could not resolve enum type: {enumTypeName}", MessageType.Warning);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(2);
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

        private void ShowEnumTypeMenu(Rect buttonRect, SerializedProperty enumTypeProp, SerializedProperty intValueProp)
        {
            // Get all enum types from loaded assemblies
            var enumTypes = new System.Collections.Generic.List<System.Type>();

            foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Skip system assemblies for performance
                    string assemblyName = assembly.GetName().Name;
                    if (assemblyName.StartsWith("System") ||
                        assemblyName.StartsWith("Microsoft") ||
                        assemblyName.StartsWith("mscorlib") ||
                        assemblyName.StartsWith("netstandard") ||
                        assemblyName.StartsWith("Mono."))
                        continue;

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsEnum && !type.IsGenericType && type.IsPublic)
                        {
                            enumTypes.Add(type);
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that can't be reflected
                }
            }

            // Sort by name
            enumTypes.Sort((a, b) => string.Compare(a.Name, b.Name, System.StringComparison.Ordinal));

            // Create menu
            GenericMenu menu = new GenericMenu();

            // Add "None" option
            menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(enumTypeProp.stringValue), () =>
            {
                enumTypeProp.stringValue = "";
                intValueProp.intValue = 0;
                _serializedSettings.ApplyModifiedProperties();
            });

            menu.AddSeparator("");

            // Add enum types grouped by namespace
            var groupedTypes = enumTypes.GroupBy(t => string.IsNullOrEmpty(t.Namespace) ? "(Global)" : t.Namespace)
                                       .OrderBy(g => g.Key);

            foreach (var group in groupedTypes)
            {
                foreach (var type in group)
                {
                    string menuPath = $"{group.Key}/{type.Name}";
                    bool isSelected = enumTypeProp.stringValue == type.AssemblyQualifiedName;

                    menu.AddItem(new GUIContent(menuPath), isSelected, () =>
                    {
                        enumTypeProp.stringValue = type.AssemblyQualifiedName;
                        intValueProp.intValue = 0; // Reset value when type changes
                        _serializedSettings.ApplyModifiedProperties();
                    });
                }
            }

            menu.DropDown(buttonRect);
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

            // Ensure folder exists
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

            // Create asset
            var newSettings = CreateInstance<GlobalSettings>();
            string assetPath = $"{folderPath}/GlobalSettings.asset";
            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Load it
            _settings = newSettings;
            _serializedSettings = new SerializedObject(_settings);

            Debug.Log($"[GlobalSettings] Created new GlobalSettings asset at: {assetPath}");
            Selection.activeObject = newSettings;
            EditorGUIUtility.PingObject(newSettings);
        }
    }
}
