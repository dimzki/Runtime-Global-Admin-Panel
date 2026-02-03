using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Automatically manages the TMP_PRESENT scripting define symbol based on whether TextMesh Pro is installed.
    /// This allows the GlobalSettingsRuntimePanel to use TMP when available and fall back to Unity Text when not.
    /// </summary>
    [InitializeOnLoad]
    public class TMPDefineSymbolManager
    {
        private const string TMP_DEFINE = "TMP_PRESENT";
        private const string TMP_TYPE = "TMPro.TextMeshProUGUI";

        static TMPDefineSymbolManager()
        {
            UpdateDefineSymbol();
        }

        private static void UpdateDefineSymbol()
        {
            // Check if TMP is installed by trying to find the type
            bool isTMPInstalled = System.Type.GetType(TMP_TYPE + ", Unity.TextMeshPro") != null;

            // Get current build target
            UnityEditor.Build.NamedBuildTarget buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            // Get current defines
            string definesString = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
            System.Collections.Generic.List<string> allDefines = new System.Collections.Generic.List<string>(definesString.Split(';'));

            bool hasDefine = allDefines.Contains(TMP_DEFINE);

            if (isTMPInstalled && !hasDefine)
            {
                // Add TMP_PRESENT define
                allDefines.Add(TMP_DEFINE);
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", allDefines.ToArray()));
                Debug.Log($"[TMPDefineSymbolManager] TextMesh Pro detected. Added '{TMP_DEFINE}' scripting define symbol.");
            }
            else if (!isTMPInstalled && hasDefine)
            {
                // Remove TMP_PRESENT define
                allDefines.Remove(TMP_DEFINE);
                PlayerSettings.SetScriptingDefineSymbols(buildTarget, string.Join(";", allDefines.ToArray()));
                Debug.Log($"[TMPDefineSymbolManager] TextMesh Pro not found. Removed '{TMP_DEFINE}' scripting define symbol. Will use Unity Text instead.");
            }
        }

        [MenuItem("Tools/Alzaki/Global Settings/Force Update TMP Define Symbol")]
        private static void ForceUpdateDefineSymbol()
        {
            UpdateDefineSymbol();
            Debug.Log("[TMPDefineSymbolManager] TMP define symbol update completed.");
        }
    }
}
