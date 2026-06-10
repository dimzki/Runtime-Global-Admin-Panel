using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    [CustomPropertyDrawer(typeof(SettingsCategory))]
    public class SettingsCategoryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty nameProp = property.FindPropertyRelative("categoryName");
                SerializedProperty useIndProp = property.FindPropertyRelative("useIndependentScriptableObject");
                SerializedProperty indSOProp = property.FindPropertyRelative("independentScriptableObject");
                
                float y = position.y + EditorGUIUtility.singleLineHeight + 2;
                
                Rect nameRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(nameRect, nameProp);
                y += EditorGUIUtility.singleLineHeight + 2;
                
                Rect useIndRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(useIndRect, useIndProp);
                y += EditorGUIUtility.singleLineHeight + 2;
                
                if (useIndProp.boolValue)
                {
                    Rect indSORect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(indSORect, indSOProp);
                }
                else
                {
                    string[] listNames = { "intSettings", "floatSettings", "stringSettings", "boolSettings", "colorSettings", "vector2Settings", "vector3Settings", "curveSettings", "enumSettings" };
                    foreach (string listName in listNames)
                    {
                        SerializedProperty listProp = property.FindPropertyRelative(listName);
                        float propHeight = EditorGUI.GetPropertyHeight(listProp, true);
                        Rect listRect = new Rect(position.x, y, position.width, propHeight);
                        EditorGUI.PropertyField(listRect, listProp, true);
                        y += propHeight + 2;
                    }
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
            
            float height = EditorGUIUtility.singleLineHeight + 2; // Foldout
            height += EditorGUIUtility.singleLineHeight + 2; // categoryName
            height += EditorGUIUtility.singleLineHeight + 2; // useIndependentScriptableObject
            
            SerializedProperty useIndProp = property.FindPropertyRelative("useIndependentScriptableObject");
            
            if (useIndProp.boolValue)
            {
                height += EditorGUIUtility.singleLineHeight + 2; // independentScriptableObject
            }
            else
            {
                string[] listNames = { "intSettings", "floatSettings", "stringSettings", "boolSettings", "colorSettings", "vector2Settings", "vector3Settings", "curveSettings", "enumSettings" };
                foreach (string listName in listNames)
                {
                    SerializedProperty listProp = property.FindPropertyRelative(listName);
                    height += EditorGUI.GetPropertyHeight(listProp, true) + 2;
                }
            }
            
            return height;
        }
    }
}
