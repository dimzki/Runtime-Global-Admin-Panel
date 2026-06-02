using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Alzaki.GlobalSettings
{
#if !ODIN_INSPECTOR
    [CustomEditor(typeof(GlobalSettingsManager), true)]
    public class GlobalSettingsManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(10);

            var type = target.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(DebugAttribute), true);
                if (attributes.Length > 0)
                {
                    // Draw a nice looking button
                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f);
                    
                    if (GUILayout.Button(method.Name, GUILayout.Height(30)))
                    {
                        if (method.IsStatic)
                        {
                            method.Invoke(null, null);
                        }
                        else
                        {
                            method.Invoke(target, null);
                        }
                    }
                    
                    GUI.backgroundColor = oldColor;
                }
            }
        }
    }
#endif
}
