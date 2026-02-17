using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Custom inspector for GlobalSettings ScriptableObject.
    /// Displays settings grouped by category, each with typed sub-lists.
    /// </summary>
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsInspector : Editor
    {
        private SerializedProperty _categories;

        // Foldout state: category index → expanded
        private Dictionary<int, bool> _categoryFoldouts = new Dictionary<int, bool>();

        // Foldout state: "CategoryIndex_FieldName" → expanded
        private Dictionary<string, bool> _sectionFoldouts = new Dictionary<string, bool>();

        // Foldout state: "CategoryIndex_FieldName_ItemIndex" → expanded
        private Dictionary<string, bool> _itemFoldouts = new Dictionary<string, bool>();

        private static readonly string[] _settingFieldNames =
        {
            "intSettings", "floatSettings", "stringSettings", "boolSettings",
            "colorSettings", "vector2Settings", "vector3Settings", "curveSettings", "enumSettings"
        };

        /// <summary>
        /// Scans all categories and all setting-type lists for duplicate keys.
        /// Returns a HashSet of keys that appear more than once across the entire asset.
        /// </summary>
        private HashSet<string> GetDuplicateKeys()
        {
            var seen      = new HashSet<string>();
            var duplicates = new HashSet<string>();

            for (int ci = 0; ci < _categories.arraySize; ci++)
            {
                var category = _categories.GetArrayElementAtIndex(ci);
                foreach (var fieldName in _settingFieldNames)
                {
                    var list = category.FindPropertyRelative(fieldName);
                    if (list == null) continue;
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        var keyProp = list.GetArrayElementAtIndex(i).FindPropertyRelative("key");
                        if (keyProp == null) continue;
                        string k = keyProp.stringValue;
                        if (string.IsNullOrEmpty(k)) continue;
                        if (!seen.Add(k))
                            duplicates.Add(k);
                    }
                }
            }

            return duplicates;
        }

        private void OnEnable()
        {
            _categories = serializedObject.FindProperty("categories");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);

            // ── Top toolbar ────────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Categories", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ Add Category", GUILayout.Width(110), GUILayout.Height(22)))
                {
                    _categories.InsertArrayElementAtIndex(_categories.arraySize);
                    var newCat = _categories.GetArrayElementAtIndex(_categories.arraySize - 1);
                    // InsertArrayElementAtIndex copies the previous element — reset everything.
                    newCat.FindPropertyRelative("categoryName").stringValue = "New Category";
                    newCat.FindPropertyRelative("intSettings").ClearArray();
                    newCat.FindPropertyRelative("floatSettings").ClearArray();
                    newCat.FindPropertyRelative("stringSettings").ClearArray();
                    newCat.FindPropertyRelative("boolSettings").ClearArray();
                    newCat.FindPropertyRelative("colorSettings").ClearArray();
                    newCat.FindPropertyRelative("vector2Settings").ClearArray();
                    newCat.FindPropertyRelative("vector3Settings").ClearArray();
                    newCat.FindPropertyRelative("curveSettings").ClearArray();
                    newCat.FindPropertyRelative("enumSettings").ClearArray();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // ── Draw each category ─────────────────────────────────────────────
            HashSet<string> duplicateKeys = GetDuplicateKeys();
            for (int ci = 0; ci < _categories.arraySize; ci++)
            {
                if (DrawCategory(ci, duplicateKeys)) break; // category was deleted
            }

            EditorGUILayout.Space(10);

            // ── Bottom buttons ─────────────────────────────────────────────────
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
                        "Are you sure you want to clear ALL categories and settings?", "Yes", "Cancel"))
                    {
                        ((GlobalSettings)target).ClearAll();
                        EditorUtility.SetDirty(target);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        // Returns true if the category was deleted (caller should break the loop).
        private bool DrawCategory(int ci, HashSet<string> duplicateKeys)
        {
            var category = _categories.GetArrayElementAtIndex(ci);
            var catNameProp = category.FindPropertyRelative("categoryName");

            string catName = string.IsNullOrEmpty(catNameProp.stringValue)
                ? $"Category {ci}"
                : catNameProp.stringValue;

            if (!_categoryFoldouts.ContainsKey(ci))
                _categoryFoldouts[ci] = true;

            EditorGUILayout.BeginVertical();
            {
                // Header row
                EditorGUILayout.BeginHorizontal();
                {
                    GUIStyle catStyle = new GUIStyle(EditorStyles.foldoutHeader) { fontStyle = FontStyle.Bold };
                    _categoryFoldouts[ci] = EditorGUILayout.Foldout(_categoryFoldouts[ci], catName, true, catStyle);

                    if (GUILayout.Button("✕", GUILayout.Width(24)))
                    {
                        if (EditorUtility.DisplayDialog("Delete Category",
                            $"Delete '{catName}' and all its settings?", "Delete", "Cancel"))
                        {
                            _categories.DeleteArrayElementAtIndex(ci);
                            serializedObject.ApplyModifiedProperties();
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                            return true;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (_categoryFoldouts[ci])
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(catNameProp, new GUIContent("Category Name"));
                    EditorGUILayout.Space(4);

                    DrawSettingsList(category, ci, "intSettings",    "Int Settings",     duplicateKeys);
                    DrawSettingsList(category, ci, "floatSettings",   "Float Settings",   duplicateKeys);
                    DrawSettingsList(category, ci, "stringSettings",  "String Settings",  duplicateKeys);
                    DrawSettingsList(category, ci, "boolSettings",    "Bool Settings",    duplicateKeys);
                    DrawSettingsList(category, ci, "colorSettings",   "Color Settings",   duplicateKeys);
                    DrawSettingsList(category, ci, "vector2Settings", "Vector2 Settings", duplicateKeys);
                    DrawSettingsList(category, ci, "vector3Settings", "Vector3 Settings", duplicateKeys);
                    DrawSettingsList(category, ci, "curveSettings",   "Curve Settings",   duplicateKeys);
                    DrawEnumSettingsList(category, ci, duplicateKeys);

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);

            return false;
        }

        private void DrawSettingsList(SerializedProperty category, int ci, string fieldName, string label, HashSet<string> duplicateKeys)
        {
            var property = category.FindPropertyRelative(fieldName);
            if (property == null) return;

            string sectionKey = $"{ci}_{fieldName}";
            if (!_sectionFoldouts.ContainsKey(sectionKey))
                _sectionFoldouts[sectionKey] = true;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                // Section header with + button
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(12);
                    EditorGUILayout.BeginVertical();

                    Rect foldoutRect = EditorGUILayout.GetControlRect();
                    Rect btnRect = new Rect(foldoutRect.xMax - 30, foldoutRect.y, 25, foldoutRect.height);
                    Rect labelRect = new Rect(foldoutRect.x, foldoutRect.y, foldoutRect.width - 35, foldoutRect.height);

                    if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                        Event.current.Use();
                    }

                    GUIStyle style = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                    _sectionFoldouts[sectionKey] = EditorGUI.Foldout(
                        labelRect, _sectionFoldouts[sectionKey],
                        $"{label} ({property.arraySize})", true, style);

                    GUI.Button(btnRect, "+");
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (_sectionFoldouts[sectionKey])
                {
                    EditorGUI.indentLevel++;

                    if (property.arraySize > 0)
                    {
                        var sepRect = EditorGUILayout.GetControlRect(false, 3);
                        EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        EditorGUILayout.Space(2);
                    }

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element  = property.GetArrayElementAtIndex(i);
                        var keyProp  = element.FindPropertyRelative("key");
                        var valProp  = element.FindPropertyRelative("value");

                        string keyName  = !string.IsNullOrEmpty(keyProp.stringValue) ? keyProp.stringValue : $"Item {i}";
                        string itemKey  = $"{sectionKey}_{i}";
                        if (!_itemFoldouts.ContainsKey(itemKey)) _itemFoldouts[itemKey] = false;

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                _itemFoldouts[itemKey] = EditorGUILayout.Foldout(_itemFoldouts[itemKey], keyName, true);
                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    property.DeleteArrayElementAtIndex(i);
                                    _itemFoldouts.Remove(itemKey);
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.EndVertical();
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            if (_itemFoldouts[itemKey])
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));
                                if (!string.IsNullOrEmpty(keyProp.stringValue) && duplicateKeys.Contains(keyProp.stringValue))
                                    EditorGUILayout.HelpBox($"Duplicate key \"{keyProp.stringValue}\" — keys must be unique across all categories.", MessageType.Error);
                                EditorGUILayout.PropertyField(valProp, new GUIContent("Value"));
                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        if (i < property.arraySize - 1)
                        {
                            var sepRect = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        }
                        EditorGUILayout.Space(2);
                    }

                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawEnumSettingsList(SerializedProperty category, int ci, HashSet<string> duplicateKeys)
        {
            var property = category.FindPropertyRelative("enumSettings");
            if (property == null) return;

            string sectionKey = $"{ci}_enumSettings";
            if (!_sectionFoldouts.ContainsKey(sectionKey))
                _sectionFoldouts[sectionKey] = true;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.Space(12);
                    EditorGUILayout.BeginVertical();

                    Rect foldoutRect = EditorGUILayout.GetControlRect();
                    Rect btnRect = new Rect(foldoutRect.xMax - 30, foldoutRect.y, 25, foldoutRect.height);
                    Rect labelRect = new Rect(foldoutRect.x, foldoutRect.y, foldoutRect.width - 35, foldoutRect.height);

                    if (Event.current.type == EventType.MouseDown && btnRect.Contains(Event.current.mousePosition))
                    {
                        property.InsertArrayElementAtIndex(property.arraySize);
                        Event.current.Use();
                    }

                    GUIStyle style = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold };
                    _sectionFoldouts[sectionKey] = EditorGUI.Foldout(
                        labelRect, _sectionFoldouts[sectionKey],
                        $"Enum Settings ({property.arraySize})", true, style);

                    GUI.Button(btnRect, "+");
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                if (_sectionFoldouts[sectionKey])
                {
                    EditorGUI.indentLevel++;

                    if (property.arraySize > 0)
                    {
                        var sepRect = EditorGUILayout.GetControlRect(false, 3);
                        EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
                        EditorGUILayout.Space(2);
                    }

                    for (int i = 0; i < property.arraySize; i++)
                    {
                        var element      = property.GetArrayElementAtIndex(i);
                        var keyProp      = element.FindPropertyRelative("key");
                        var enumTypeProp = element.FindPropertyRelative("enumTypeName");
                        var intValueProp = element.FindPropertyRelative("intValue");

                        string keyName = !string.IsNullOrEmpty(keyProp.stringValue) ? keyProp.stringValue : $"Item {i}";
                        string itemKey = $"{sectionKey}_{i}";
                        if (!_itemFoldouts.ContainsKey(itemKey)) _itemFoldouts[itemKey] = false;

                        EditorGUILayout.BeginVertical();
                        {
                            EditorGUILayout.BeginHorizontal();
                            {
                                _itemFoldouts[itemKey] = EditorGUILayout.Foldout(_itemFoldouts[itemKey], keyName, true);
                                if (GUILayout.Button("-", GUILayout.Width(25)))
                                {
                                    property.DeleteArrayElementAtIndex(i);
                                    _itemFoldouts.Remove(itemKey);
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.EndVertical();
                                    break;
                                }
                            }
                            EditorGUILayout.EndHorizontal();

                            if (_itemFoldouts[itemKey])
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(keyProp, new GUIContent("Key"));
                                if (!string.IsNullOrEmpty(keyProp.stringValue) && duplicateKeys.Contains(keyProp.stringValue))
                                    EditorGUILayout.HelpBox($"Duplicate key \"{keyProp.stringValue}\" — keys must be unique across all categories.", MessageType.Error);

                                string enumTypeName = enumTypeProp.stringValue;
                                System.Type enumType = string.IsNullOrEmpty(enumTypeName)
                                    ? null
                                    : System.Type.GetType(enumTypeName);

                                // Type selector button
                                EditorGUILayout.BeginHorizontal();
                                {
                                    EditorGUILayout.PrefixLabel("Type");
                                    string displayName = enumType != null ? $"{enumType.Name} (Enum)" : "None";
                                    if (EditorGUILayout.DropdownButton(new GUIContent(displayName), FocusType.Keyboard))
                                    {
                                        Rect btnRect = GUILayoutUtility.GetLastRect();
                                        EnumTypeSelector.Show(GUIUtility.GUIToScreenRect(btnRect), (selectedType) =>
                                        {
                                            enumTypeProp.stringValue = selectedType?.AssemblyQualifiedName ?? "";
                                            intValueProp.intValue    = 0;
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                // Value selector
                                if (enumType != null && enumType.IsEnum)
                                {
                                    var current  = (System.Enum)System.Enum.ToObject(enumType, intValueProp.intValue);
                                    var newValue = EditorGUILayout.EnumPopup(new GUIContent("Value"), current);
                                    intValueProp.intValue = System.Convert.ToInt32(newValue);
                                }
                                else if (!string.IsNullOrEmpty(enumTypeName))
                                {
                                    EditorGUILayout.HelpBox($"Cannot resolve type: {enumTypeName}", MessageType.Warning);
                                }

                                EditorGUI.indentLevel--;
                            }
                        }
                        EditorGUILayout.EndVertical();

                        if (i < property.arraySize - 1)
                        {
                            var sepRect = EditorGUILayout.GetControlRect(false, 1);
                            EditorGUI.DrawRect(sepRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
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
