using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Custom inspector for GlobalSettings ScriptableObject.
    /// Provides dropdown UI for enum type selection with search functionality.
    /// </summary>
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsInspector : Editor
    {
        private SerializedProperty _intSettings;
        private SerializedProperty _floatSettings;
        private SerializedProperty _stringSettings;
        private SerializedProperty _boolSettings;
        private SerializedProperty _colorSettings;
        private SerializedProperty _vector2Settings;
        private SerializedProperty _vector3Settings;
        private SerializedProperty _enumSettings;

        private bool _showIntSettings = true;
        private bool _showFloatSettings = true;
        private bool _showStringSettings = true;
        private bool _showBoolSettings = true;
        private bool _showColorSettings = true;
        private bool _showVector2Settings = true;
        private bool _showVector3Settings = true;
        private bool _showEnumSettings = true;

        // Track foldout state for each item
        private System.Collections.Generic.Dictionary<string, bool> _itemFoldouts = new System.Collections.Generic.Dictionary<string, bool>();

        private void OnEnable()
        {
            _intSettings = serializedObject.FindProperty("intSettings");
            _floatSettings = serializedObject.FindProperty("floatSettings");
            _stringSettings = serializedObject.FindProperty("stringSettings");
            _boolSettings = serializedObject.FindProperty("boolSettings");
            _colorSettings = serializedObject.FindProperty("colorSettings");
            _vector2Settings = serializedObject.FindProperty("vector2Settings");
            _vector3Settings = serializedObject.FindProperty("vector3Settings");
            _enumSettings = serializedObject.FindProperty("enumSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // Int Settings
            DrawSettingsList(_intSettings, ref _showIntSettings, "Int Settings");

            // Float Settings
            DrawSettingsList(_floatSettings, ref _showFloatSettings, "Float Settings");

            // String Settings
            DrawSettingsList(_stringSettings, ref _showStringSettings, "String Settings");

            // Bool Settings
            DrawSettingsList(_boolSettings, ref _showBoolSettings, "Bool Settings");

            // Color Settings
            DrawSettingsList(_colorSettings, ref _showColorSettings, "Color Settings");

            // Vector2 Settings
            DrawSettingsList(_vector2Settings, ref _showVector2Settings, "Vector2 Settings");

            // Vector3 Settings
            DrawSettingsList(_vector3Settings, ref _showVector3Settings, "Vector3 Settings");

            // Enum Settings (custom drawing)
            DrawEnumSettingsList();

            EditorGUILayout.Space(10);

            // Buttons
            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Refresh Dictionaries", GUILayout.Height(30)))
                {
                    ((GlobalSettings)target).RefreshDictionaries();
                    EditorUtility.SetDirty(target);
                }

                if (GUILayout.Button("Clear All", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("Clear All Settings",
                        "Are you sure you want to clear all settings?", "Yes", "Cancel"))
                    {
                        ((GlobalSettings)target).ClearAll();
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettingsList(SerializedProperty property, ref bool foldout, string label)
        {
            if (property == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    // Add small space to move foldout away from edge
                    GUILayout.Space(12);

                    // Use foldout with custom button handling
                    EditorGUILayout.BeginVertical();

                    Rect foldoutRect = EditorGUILayout.GetControlRect();
                    Rect buttonRect = new Rect(foldoutRect.xMax - 30, foldoutRect.y, 25, foldoutRect.height);
                    Rect actualFoldoutRect = new Rect(foldoutRect.x, foldoutRect.y, foldoutRect.width - 35, foldoutRect.height);

                    // Check button click first, before foldout processes events
                    if (Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                        Event.current.Use();
                    }

                    // Draw foldout in the remaining space with bold label
                    GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                    foldout = EditorGUI.Foldout(actualFoldoutRect, foldout, $"{label} ({property.arraySize})", true, foldoutStyle);

                    // Draw button
                    GUI.Button(buttonRect, "+");
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (foldout)
                {
                    EditorGUI.indentLevel++;

                    // Add separator line between header and items
                    if (property.arraySize > 0)
                    {
                        var headerSeparatorRect = EditorGUILayout.GetControlRect(false, 3);
                        EditorGUI.DrawRect(headerSeparatorRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        EditorGUILayout.Space(2);
                    }

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element = property.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("key");
                        var valueProp = element.FindPropertyRelative("value");

                        // Display key name as a foldout header
                        string keyName = !string.IsNullOrEmpty(keyProp.stringValue) ? keyProp.stringValue : $"Item {i}";
                        string foldoutKey = $"{label}_{i}";

                        if (!_itemFoldouts.ContainsKey(foldoutKey))
                            _itemFoldouts[foldoutKey] = false;

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                _itemFoldouts[foldoutKey] = EditorGUILayout.Foldout(_itemFoldouts[foldoutKey], keyName, true);

                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    property.DeleteArrayElementAtIndex(i);
                                    _itemFoldouts.Remove(foldoutKey);
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            // Show the actual fields only when expanded
                            if (_itemFoldouts[foldoutKey])
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));
                                EditorGUILayout.PropertyField(valueProp, new GUIContent("Value"));
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        // Add a subtle separator line
                        if (i < property.arraySize - 1)
                        {
                            var separatorRect = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(separatorRect, new UnityEngine.Color(0.5f, 0.5f, 0.5f, 0.3f));
                        }

                        EditorGUILayout.Space(2);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);
        }

        private void DrawEnumSettingsList()
        {
            if (_enumSettings == null) return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    // Add small space to move foldout away from edge
                    GUILayout.Space(12);

                    // Use foldout with custom button handling
                    EditorGUILayout.BeginVertical();

                    Rect enumFoldoutRect = EditorGUILayout.GetControlRect();
                    Rect enumButtonRect = new Rect(enumFoldoutRect.xMax - 30, enumFoldoutRect.y, 25, enumFoldoutRect.height);
                    Rect actualEnumFoldoutRect = new Rect(enumFoldoutRect.x, enumFoldoutRect.y, enumFoldoutRect.width - 35, enumFoldoutRect.height);

                    // Check button click first, before foldout processes events
                    if (Event.current.type == EventType.MouseDown && enumButtonRect.Contains(Event.current.mousePosition))
                    {
                        _enumSettings.InsertArrayElementAtIndex(_enumSettings.arraySize);
                        Event.current.Use();
                    }

                    // Draw foldout in the remaining space with bold label
                    GUIStyle enumFoldoutStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                    _showEnumSettings = EditorGUI.Foldout(actualEnumFoldoutRect, _showEnumSettings, $"Enum Settings ({_enumSettings.arraySize})", true, enumFoldoutStyle);

                    // Draw button
                    GUI.Button(enumButtonRect, "+");
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (_showEnumSettings)
                {
                    EditorGUI.indentLevel++;

                    // Add separator line between header and items
                    if (_enumSettings.arraySize > 0)
                    {
                        var headerSeparatorRect = EditorGUILayout.GetControlRect(false, 3);
                        EditorGUI.DrawRect(headerSeparatorRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        EditorGUILayout.Space(2);
                    }

                    for (int i = 0; i < _enumSettings.arraySize; i++)
                    {
                        var element = _enumSettings.GetArrayElementAtIndex(i);
                        var keyProp = element.FindPropertyRelative("key");
                        var enumTypeProp = element.FindPropertyRelative("enumTypeName");
                        var intValueProp = element.FindPropertyRelative("intValue");

                        // Header with key name and delete button
                        string keyName = !string.IsNullOrEmpty(keyProp.stringValue) ? keyProp.stringValue : $"Item {i}";
                        string foldoutKey = $"Enum_{i}";

                        if (!_itemFoldouts.ContainsKey(foldoutKey))
                            _itemFoldouts[foldoutKey] = false;

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                _itemFoldouts[foldoutKey] = EditorGUILayout.Foldout(_itemFoldouts[foldoutKey], keyName, true);

                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    _enumSettings.DeleteArrayElementAtIndex(i);
                                    _itemFoldouts.Remove(foldoutKey);
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            // Show the actual fields only when expanded
                            if (_itemFoldouts[foldoutKey])
                            {
                                EditorGUI.indentLevel++;

                                // Key field
                                EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));

                                // Try to resolve the enum type
                                string enumTypeName = enumTypeProp.stringValue;
                                System.Type enumType = null;

                                if (!string.IsNullOrEmpty(enumTypeName))
                                {
                                    enumType = System.Type.GetType(enumTypeName);
                                }

                                // Type selector
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.PrefixLabel("Type");

                                    // Display current type name or "None"
                                    string displayTypeName = enumType != null ? $"{enumType.Name} (Enum)" : "None (Enum)";

                                    // Type selector button with dropdown icon
                                    if (EditorGUILayout.DropdownButton(new GUIContent(displayTypeName), FocusType.Keyboard))
                                    {
                                        // Get button rect in screen space
                                        Rect buttonRect = GUILayoutUtility.GetLastRect();
                                        Rect screenRect = GUIUtility.GUIToScreenRect(buttonRect);

                                        EnumTypeSelector.Show(screenRect, (selectedType) =>
                                        {
                                            if (selectedType != null)
                                            {
                                                enumTypeProp.stringValue = selectedType.AssemblyQualifiedName;
                                                intValueProp.intValue = 0;
                                            }
                                            else
                                            {
                                                enumTypeProp.stringValue = "";
                                                intValueProp.intValue = 0;
                                            }
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                // Value selector (only if type is valid)
                                if (enumType != null && enumType.IsEnum)
                                {
                                    // Show enum popup with names
                                    var currentValue = (System.Enum)System.Enum.ToObject(enumType, intValueProp.intValue);
                                    var newValue = EditorGUILayout.EnumPopup(new GUIContent("Value"), currentValue);
                                    intValueProp.intValue = System.Convert.ToInt32(newValue);
                                }
                                else if (!string.IsNullOrEmpty(enumTypeName))
                                {
                                    EditorGUILayout.HelpBox($"Could not resolve enum type: {enumTypeName}", MessageType.Warning);
                                }

                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        // Add a subtle separator line
                        if (i < _enumSettings.arraySize - 1)
                        {
                            var separatorRect = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(separatorRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        }

                        EditorGUILayout.Space(2);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(2);
        }

    }
}
