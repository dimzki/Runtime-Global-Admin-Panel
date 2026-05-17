# Runtime Global Admin Panel

A lightweight, standalone global settings system for Unity with a runtime admin panel for testing and configuration during play mode.

## Features

- **Pre-Scene Loading**: Settings load via `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)` - available before any scene loads
- **Type Support**: Int, Float, String, Bool, Color, Vector2, Vector3, and Enum types
- **No Dependencies**: Fully standalone - no Odin Inspector or other third-party dependencies required
- **Runtime Admin Panel**: Press F12 during play mode for a beautiful runtime settings panel with color picker
- **Custom Inspector**: Clean, organized inspector UI with collapsible sections and visual separators
- **Singleton Pattern**: Easy access via `GlobalSettingsManager.Instance`
- **Resource-Based**: Loads from Resources folder - no scene dependencies
- **Enum Support**: Full enum support with searchable type selector

## Installation

### Install via Unity Package Manager (UPM) - Recommended

You can install this package directly via the Unity Package Manager using the Git URL.

1. Open your Unity project.
2. Go to **Window > Package Manager**.
3. Click the **+** button in the top-left corner.
4. Select **Add package from git URL...**
5. Enter the following URL and click **Add**:
   `https://github.com/dimzki/Runtime-Global-Admin-Panel.git`

*Note: You can also append a specific version tag or branch to the URL (e.g., `https://github.com/dimzki/Runtime-Global-Admin-Panel.git#v2.0.0`).*

### Install via manifest.json

Alternatively, you can open your project's `Packages/manifest.json` file and add the following line to your `"dependencies"` block:

```json
"com.alzaki.runtimeglobaladminpanel": "https://github.com/dimzki/Runtime-Global-Admin-Panel.git"
```

## Setup

### 1. Create GlobalSettings Asset

**Option A: Via Menu**
1. Go to `Tools > Alzaki > Global Settings`
2. Click "Create GlobalSettings Asset" button

**Option B: Manual**
1. Right-click in Project window
2. Select `Create > Alzaki > Global Settings`
3. Place it in `Assets/Alzaki/Global Settings Tool/Resources/`
4. Rename to `GlobalSettings`

### 2. Add Your Settings

Open the GlobalSettings asset and add your settings in the inspector:

- **Integer Settings**: Version numbers, counters, IDs
- **Float Settings**: Timers, multipliers, volumes
- **String Settings**: Player names, API keys, paths
- **Bool Settings**: Feature flags, toggles
- **Color Settings**: Theme colors, debug colors
- **Vector2 Settings**: 2D positions, sizes
- **Vector3 Settings**: 3D positions, rotations, scales
- **Enum Settings**: Custom enum types with dropdown selection

## Usage

### Basic Usage

```csharp
using Alzaki.GlobalSettings;
using UnityEngine;

public class MyScript : MonoBehaviour
{
    private void Awake()
    {
        // Settings are guaranteed to be loaded here!

        // Get values with defaults
        int version = GlobalSettingsManager.GetInt("Version", 1);
        float idleTimer = GlobalSettingsManager.GetFloat("IdleTimer", 10f);
        string playerName = GlobalSettingsManager.GetString("PlayerName", "Guest");
        bool debugMode = GlobalSettingsManager.GetBool("DebugMode", false);
        Color themeColor = GlobalSettingsManager.GetColor("ThemeColor", Color.blue);
        Vector3 spawnPos = GlobalSettingsManager.GetVector3("SpawnPosition", Vector3.zero);
    }
}
```

### Set Values at Runtime

```csharp
// Set values (changes ScriptableObject - persists in editor, not in builds)
GlobalSettingsManager.SetInt("Score", 1000);
GlobalSettingsManager.SetFloat("Volume", 0.8f);
GlobalSettingsManager.SetString("LastLevel", "Level_5");
GlobalSettingsManager.SetBool("MusicEnabled", true);
GlobalSettingsManager.SetColor("PlayerColor", Color.red);
GlobalSettingsManager.SetVector3("Position", new Vector3(10, 0, 5));
```

### Enum Settings

```csharp
using Alzaki.GlobalSettings;

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
    Expert
}

public class GameController : MonoBehaviour
{
    private void Awake()
    {
        // Get enum value
        var difficulty = GlobalSettingsManager.GetEnum<GameDifficulty>("Difficulty", GameDifficulty.Normal);

        Debug.Log($"Game difficulty: {difficulty}");
    }

    private void SetDifficulty()
    {
        // Set enum value
        GlobalSettingsManager.SetEnum("Difficulty", GameDifficulty.Hard);
    }
}
```

### Runtime Admin Panel

During play mode, press **F12** to open the runtime settings panel. Features:

- Live editing of all settings
- **Color Picker**: Full RGBA color picker with hex code support
- **Save/Cancel**: Apply or discard changes
- **Visual Feedback**: See changes in real-time
- **Keyboard Shortcut**: Customizable (default F12)

```csharp
// Open the admin panel programmatically
GlobalSettingsRuntimePanel.Instance.Show();

// Or access via GlobalSettingsManager
GlobalSettingsManager.OpenRuntimePanel();
```

### Direct Access to Settings Asset

```csharp
// Access the ScriptableObject directly for advanced usage
GlobalSettings settings = GlobalSettingsManager.Settings;

if (settings != null)
{
    // Use any custom method you added to GlobalSettings
    settings.RefreshDictionaries();
    settings.ClearAll();
}
```

## Architecture

### Files Structure

```
Packages/com.alzaki.runtimeglobaladminpanel/
├── Scripts/
│   ├── GlobalSettings.cs              // ScriptableObject data container
│   ├── GlobalSettingsManager.cs       // Singleton manager (auto-loads before scene)
│   ├── GlobalSettingsRuntimePanel.cs  // Runtime admin panel UI
│   ├── GlobalSettingsUsageExample.cs  // Example usage script
│   └── Attributes/                    // Custom attributes
├── Editor/
│   ├── GlobalSettingsInspector.cs     // Custom inspector with collapsible UI
│   ├── GlobalSettingsWindow.cs        // Editor window
│   └── EnumTypeSelector.cs            // Enum type selector with search
├── Resources/
│   └── GlobalSettings.asset           // Your settings asset (create this)
└── README.md                          // This file
```

### How It Works

1. **Before Scene Load**: `GlobalSettingsManager` uses `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]` to initialize
2. **Loads from Resources**: Automatically loads `Resources/GlobalSettings.asset`
3. **DontDestroyOnLoad**: Manager persists across all scenes
4. **Singleton Access**: Access via `GlobalSettingsManager.Instance` from anywhere
5. **Runtime Panel**: Creates UI canvas on-demand during play mode

### Key Components

- **GlobalSettings**: SerializedScriptableObject with typed lists for each setting type
- **GlobalSettingsManager**: Singleton MonoBehaviour with static convenience methods
- **GlobalSettingsInspector**: Custom inspector with collapsible foldouts and clean UI
- **GlobalSettingsRuntimePanel**: Runtime UI panel with color picker and live editing
- **EnumTypeSelector**: Searchable enum type selector for the inspector

## Custom Inspector Features

- **Collapsible Sections**: Each setting type has a foldout with item count
- **Item Foldouts**: Each individual setting is collapsible
- **Visual Separators**: Clean lines between sections and items
- **Add/Remove Buttons**: Inline buttons for managing settings
- **Enum Type Selector**: Searchable dropdown for selecting enum types
- **Stable Foldout State**: Foldout state persists while editing keys

## Runtime Panel Features

- **RGBA Color Picker**: Full color picker with sliders and hex code input
- **Bordered Color Fields**: Visible borders even with alpha = 0
- **Live Updates**: Changes reflect immediately in the game
- **Save/Cancel**: Apply or discard all changes at once
- **Keyboard Toggle**: Press F12 to show/hide (customizable)

## Important Notes

### Editor vs Build
- Settings changes in the editor modify the ScriptableObject asset
- In builds, settings are read-only (loaded from asset)
- For runtime persistence in builds, use PlayerPrefs or a save system

### Resource Loading
- The asset MUST be in a Resources folder
- The asset MUST be named "GlobalSettings" (or update `SETTINGS_RESOURCE_PATH` in `GlobalSettingsManager.cs`)

### Performance
- Settings load once before scene load - zero runtime overhead
- Dictionary lookups are fast (O(1))
- No GC allocations during Get/Set operations
- Runtime panel only creates UI when opened

### Thread Safety
- Not thread-safe - use from main thread only
- For async usage, ensure calls happen on main thread

## Enum Support

### Adding Enum Settings in Inspector

1. Open GlobalSettings asset
2. Expand "Enum Settings"
3. Click "+" to add new enum setting
4. Enter a key name
5. Click the "Type" dropdown
6. Search for your enum type
7. Select the enum value

### Accessing Enum Settings

```csharp
// Get enum value
GameDifficulty difficulty = GlobalSettingsManager.GetEnum<GameDifficulty>("Difficulty");

// Get with default value
GameDifficulty difficulty = GlobalSettingsManager.GetEnum("Difficulty", GameDifficulty.Normal);

// Set enum value
GlobalSettingsManager.SetEnum("Difficulty", GameDifficulty.Hard);
```

## Advanced Usage

### Extending GlobalSettings

You can add custom methods to `GlobalSettings.cs`:

```csharp
public class GlobalSettings : ScriptableObject
{
    // ... existing code ...

    // Add custom helper methods
    public void ResetToDefaults()
    {
        SetInt("Version", 1);
        SetFloat("IdleTimer", 10f);
        SetBool("DebugMode", false);
    }

    public int IncrementCounter(string key)
    {
        int current = GetInt(key, 0);
        current++;
        SetInt(key, current);
        return current;
    }
}
```

### Customizing Runtime Panel

```csharp
// Change the toggle key
public class CustomSettings : MonoBehaviour
{
    private void Start()
    {
        var panel = GlobalSettingsRuntimePanel.Instance;
        panel.toggleKey = KeyCode.F10; // Change to F10
    }
}
```

### Multiple Settings Assets

If you need multiple settings contexts:

```csharp
// Load additional settings from Resources
var gameSettings = Resources.Load<GlobalSettings>("GameSettings");
var userSettings = Resources.Load<GlobalSettings>("UserSettings");

// Access directly
int difficulty = gameSettings.GetInt("Difficulty", 1);
string username = userSettings.GetString("Username", "Guest");
```

## Troubleshooting

### "Failed to load GlobalSettings"
- Ensure GlobalSettings.asset exists in `Assets/.../Resources/` folder
- Check the asset is named exactly "GlobalSettings"
- Verify the Resources folder is spelled correctly

### Runtime panel not showing
- Check the console for errors
- Ensure GlobalSettingsRuntimePanel component exists
- Verify the toggle key is correctly set (default F12)

### Enum types not showing in dropdown
- Ensure your enum is public
- The enum must be defined in a compiled assembly
- Try refreshing the enum cache

### Settings not persisting in builds
- ScriptableObject changes don't persist in builds
- Use PlayerPrefs, JSON, or a save system for runtime persistence

### Color field not visible with alpha = 0
- The system now includes visible borders on color fields
- Border is always visible regardless of alpha value

## Example: Integration with GameManager

```csharp
using Alzaki.GlobalSettings;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float idleDuration;
    private bool debugMode;
    private Color themeColor;
    private GameDifficulty difficulty;

    private void Awake()
    {
        // Load settings (guaranteed to be available)
        idleDuration = GlobalSettingsManager.GetFloat("IdleDuration", 10f);
        debugMode = GlobalSettingsManager.GetBool("DebugMode", false);
        themeColor = GlobalSettingsManager.GetColor("ThemeColor", Color.white);
        difficulty = GlobalSettingsManager.GetEnum<GameDifficulty>("Difficulty", GameDifficulty.Normal);

        if (debugMode)
        {
            Debug.Log($"GameManager initialized:");
            Debug.Log($"  IdleDuration: {idleDuration}");
            Debug.Log($"  Difficulty: {difficulty}");
            Debug.Log($"  ThemeColor: {themeColor}");
        }
    }
}
```

## Migration from Odin Inspector

This tool was originally designed with Odin Inspector but has been refactored to be fully standalone. If you're migrating from the Odin version:

1. Remove Odin-specific attributes from GlobalSettings.cs
2. The custom inspector provides similar functionality without Odin
3. All features remain the same, just without the Odin dependency

## Credits

- **Unity**: Built-in serialization and UI systems
- Original design inspired by Odin Inspector's type dictionaries

## License

Free to use in any project. No attribution required.

## Version History

- **v2.0**: Removed Odin dependency, added custom inspector and enum support
- **v1.0**: Initial release with Odin Inspector
