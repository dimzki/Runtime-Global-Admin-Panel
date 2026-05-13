using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    [CustomPropertyDrawer(typeof(EnumSetting))]
    public class EnumSettingDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeProp = property.FindPropertyRelative("enumTypeName");
            System.Type enumType = null;
            if (!string.IsNullOrEmpty(typeProp.stringValue)) enumType = System.Type.GetType(typeProp.stringValue);
            
            float height = EditorGUIUtility.singleLineHeight * 3 + 6;
            if (enumType != null && enumType.IsEnum) height += EditorGUIUtility.singleLineHeight + 2;
            else if (!string.IsNullOrEmpty(typeProp.stringValue)) height += EditorGUIUtility.singleLineHeight * 2;
            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var keyProp = property.FindPropertyRelative("key");
            var typeProp = property.FindPropertyRelative("enumTypeName");
            var valProp = property.FindPropertyRelative("intValue");

            Rect r = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.PropertyField(r, keyProp, new GUIContent("Key"));
            r.y += EditorGUIUtility.singleLineHeight + 2;

            System.Type enumType = null;
            if (!string.IsNullOrEmpty(typeProp.stringValue)) enumType = System.Type.GetType(typeProp.stringValue);

            string displayTypeName = enumType != null ? $"{enumType.Name} (Enum)" : "None (Enum)";

            Rect typeLabelRect = new Rect(r.x, r.y, EditorGUIUtility.labelWidth, r.height);
            Rect typeBtnRect = new Rect(r.x + EditorGUIUtility.labelWidth, r.y, r.width - EditorGUIUtility.labelWidth, r.height);

            EditorGUI.LabelField(typeLabelRect, "Type");
            if (GUI.Button(typeBtnRect, displayTypeName, EditorStyles.popup))
            {
                Rect screenRect = GUIUtility.GUIToScreenRect(typeBtnRect);
                EnumTypeSelector.Show(screenRect, (selectedType) =>
                {
                    typeProp.stringValue = selectedType != null ? selectedType.AssemblyQualifiedName : "";
                    valProp.intValue = 0;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }

            r.y += EditorGUIUtility.singleLineHeight + 2;

            if (enumType != null && enumType.IsEnum)
            {
                var currentValue = (System.Enum)System.Enum.ToObject(enumType, valProp.intValue);
                var newValue = EditorGUI.EnumPopup(r, "Value", currentValue);
                valProp.intValue = System.Convert.ToInt32(newValue);
            }
            else if (!string.IsNullOrEmpty(typeProp.stringValue))
            {
                r.height = EditorGUIUtility.singleLineHeight * 2;
                EditorGUI.HelpBox(r, $"Could not resolve enum type: {typeProp.stringValue}", MessageType.Warning);
            }

            EditorGUI.EndProperty();
        }
    }
}