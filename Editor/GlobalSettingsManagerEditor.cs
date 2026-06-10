using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

namespace Alzaki.GlobalSettings
{
#if !ODIN_INSPECTOR
    [CustomEditor(typeof(GlobalSettingsManager), true)]
    public class GlobalSettingsManagerEditor : Editor
    {
        private Dictionary<string, object[]> _methodParameters = new Dictionary<string, object[]>();

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
                    var parameters = method.GetParameters();

                    if (!_methodParameters.ContainsKey(method.Name))
                    {
                        var defaultParams = new object[parameters.Length];
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].HasDefaultValue)
                                defaultParams[i] = parameters[i].DefaultValue;
                            else if (parameters[i].ParameterType.IsValueType)
                                defaultParams[i] = System.Activator.CreateInstance(parameters[i].ParameterType);
                            else if (parameters[i].ParameterType == typeof(string))
                                defaultParams[i] = "";
                        }
                        _methodParameters[method.Name] = defaultParams;
                    }

                    var methodParams = _methodParameters[method.Name];

                    if (parameters.Length > 0)
                    {
                        EditorGUILayout.BeginVertical("box");
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            if (parameters[i].ParameterType == typeof(string))
                            {
                                methodParams[i] = EditorGUILayout.TextField(ObjectNames.NicifyVariableName(parameters[i].Name), (string)methodParams[i]);
                            }
                            else if (parameters[i].ParameterType == typeof(int))
                            {
                                methodParams[i] = EditorGUILayout.IntField(ObjectNames.NicifyVariableName(parameters[i].Name), (int)methodParams[i]);
                            }
                            else if (parameters[i].ParameterType == typeof(float))
                            {
                                methodParams[i] = EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(parameters[i].Name), (float)methodParams[i]);
                            }
                            else if (parameters[i].ParameterType == typeof(bool))
                            {
                                methodParams[i] = EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(parameters[i].Name), (bool)methodParams[i]);
                            }
                            else
                            {
                                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(parameters[i].Name), "Unsupported Type: " + parameters[i].ParameterType.Name);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }

                    // Draw a nice looking button
                    var oldColor = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.2f, 0.6f, 1f, 1f);

                    if (GUILayout.Button(method.Name, GUILayout.Height(30)))
                    {
                        if (method.IsStatic)
                        {
                            method.Invoke(null, parameters.Length > 0 ? methodParams : null);
                        }
                        else
                        {
                            method.Invoke(target, parameters.Length > 0 ? methodParams : null);
                        }
                    }

                    GUI.backgroundColor = oldColor;
                }
            }
        }
    }
#endif
}
