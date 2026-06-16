using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    [CustomEditor(typeof(GlobalSettings))]
    public class GlobalSettingsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(5);
            SerializedProperty hasPasswordProp = serializedObject.FindProperty("HasPassword");
            EditorGUILayout.PropertyField(hasPasswordProp);
            if (hasPasswordProp.boolValue)
            {
                EditorGUI.indentLevel++;
                SerializedProperty passwordProp = serializedObject.FindProperty("Password");
                
                EditorGUI.BeginChangeCheck();
                string newPassword = EditorGUILayout.TextField(passwordProp.displayName, passwordProp.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    string filteredPassword = "";
                    for (int i = 0; i < newPassword.Length; i++)
                    {
                        if (char.IsDigit(newPassword[i]))
                        {
                            filteredPassword += newPassword[i];
                        }
                    }
                    passwordProp.stringValue = filteredPassword;
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useVirtualKeyboard"));
            
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("categories"), true);
            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Dictionaries", GUILayout.Height(30)))
            {
                ((GlobalSettings)target).RefreshDictionaries();
                EditorUtility.SetDirty(target);
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear All", "Clear?", "Yes", "No"))
                {
                    ((GlobalSettings)target).ClearAll();
                    EditorUtility.SetDirty(target);
                }
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }
    }
}