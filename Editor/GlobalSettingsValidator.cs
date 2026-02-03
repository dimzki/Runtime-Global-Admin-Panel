using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Validates GlobalSettings setup and provides helpful warnings.
    /// Runs automatically on project load.
    /// </summary>
    [InitializeOnLoad]
    public static class GlobalSettingsValidator
    {
        private const string SETTINGS_RESOURCE_PATH = "GlobalSettings";
        private const string EXPECTED_PATH = "Assets/Alzaki/Global Settings Tool/Resources/GlobalSettings.asset";

        static GlobalSettingsValidator()
        {
            EditorApplication.delayCall += ValidateSetup;
        }

        private static void ValidateSetup()
        {
            // Check if GlobalSettings asset exists in Resources
            var settings = Resources.Load<GlobalSettings>(SETTINGS_RESOURCE_PATH);

            if (settings == null)
            {
                ShowSetupWarning();
            }
            else
            {
                // Validate path
                string actualPath = AssetDatabase.GetAssetPath(settings);
                if (!actualPath.Contains("/Resources/"))
                {
                    Debug.LogWarning($"[GlobalSettings] Asset found at '{actualPath}', but it should be in a Resources folder for runtime loading to work.");
                }
            }
        }

        private static void ShowSetupWarning()
        {
            bool shouldCreate = EditorUtility.DisplayDialog(
                "Global Settings Not Found",
                "No GlobalSettings asset found in Resources folder.\n\n" +
                "The Global Settings Tool requires a GlobalSettings asset to function.\n\n" +
                "Would you like to create one now?",
                "Create Asset",
                "Remind Me Later"
            );

            if (shouldCreate)
            {
                CreateGlobalSettingsAsset();
            }
        }

        [MenuItem("Tools/Alzaki/Create GlobalSettings Asset", priority = 100)]
        private static void CreateGlobalSettingsAssetMenu()
        {
            CreateGlobalSettingsAsset();
        }

        private static void CreateGlobalSettingsAsset()
        {
            string folderPath = "Assets/Alzaki/Global Settings Tool/Resources";

            // Ensure all folders exist
            if (!AssetDatabase.IsValidFolder("Assets/Alzaki"))
            {
                AssetDatabase.CreateFolder("Assets", "Alzaki");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Alzaki/Global Settings Tool"))
            {
                AssetDatabase.CreateFolder("Assets/Alzaki", "Global Settings Tool");
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets/Alzaki/Global Settings Tool", "Resources");
            }

            // Create asset
            var newSettings = ScriptableObject.CreateInstance<GlobalSettings>();
            string assetPath = $"{folderPath}/GlobalSettings.asset";

            // Check if asset already exists
            if (AssetDatabase.LoadAssetAtPath<GlobalSettings>(assetPath) != null)
            {
                bool overwrite = EditorUtility.DisplayDialog(
                    "Asset Already Exists",
                    $"A GlobalSettings asset already exists at:\n{assetPath}\n\nOverwrite it?",
                    "Overwrite",
                    "Cancel"
                );

                if (!overwrite)
                {
                    Debug.Log("[GlobalSettings] Asset creation cancelled.");
                    return;
                }
            }

            AssetDatabase.CreateAsset(newSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[GlobalSettings] Created GlobalSettings asset at: {assetPath}");

            // Select and ping the asset
            Selection.activeObject = newSettings;
            EditorGUIUtility.PingObject(newSettings);

            EditorUtility.DisplayDialog(
                "Success!",
                $"GlobalSettings asset created successfully!\n\nLocation: {assetPath}\n\n" +
                "You can now add your settings in the Inspector.\n\n" +
                "Open the settings window via:\nTools > Alzaki > Global Settings",
                "OK"
            );
        }

        [MenuItem("Tools/Alzaki/Validate GlobalSettings Setup", priority = 101)]
        private static void ValidateSetupMenu()
        {
            var settings = Resources.Load<GlobalSettings>(SETTINGS_RESOURCE_PATH);

            if (settings == null)
            {
                EditorUtility.DisplayDialog(
                    "Validation Failed",
                    "❌ No GlobalSettings asset found in Resources folder.\n\n" +
                    "Please create one via:\nTools > Alzaki > Create GlobalSettings Asset",
                    "OK"
                );
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(settings);
            bool inResources = assetPath.Contains("/Resources/");

            string message = "GlobalSettings Validation:\n\n";
            message += $"✅ Asset found: {settings.name}\n";
            message += $"✅ Path: {assetPath}\n";
            message += inResources ? "✅ In Resources folder\n" : "❌ NOT in Resources folder (runtime loading will fail)\n";
            message += $"\nAsset is ready to use!";

            EditorUtility.DisplayDialog(
                "Validation Result",
                message,
                "OK"
            );

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
        }
    }
}
