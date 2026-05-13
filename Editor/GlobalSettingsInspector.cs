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