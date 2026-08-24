using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    [CustomEditor(typeof(SettingsCategoryScriptableObject))]
    public class SettingsCategoryScriptableObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            string[] listNames = { "intSettings", "floatSettings", "stringSettings", "boolSettings", "colorSettings", "vector2Settings", "vector3Settings", "curveSettings", "enumSettings" };
            foreach (string listName in listNames)
            {
                SerializedProperty listProp = serializedObject.FindProperty(listName);
                if (listProp != null)
                {
                    EditorGUILayout.PropertyField(listProp, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
