using System;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Example custom types that can be stored in GlobalSettings.
    /// You can add your own enums, classes, or structs here.
    /// </summary>

    // ═════════════════════════════════════════════════════════════════════════════
    // EXAMPLE ENUMS
    // ═════════════════════════════════════════════════════════════════════════════

    [Serializable]
    public enum GameDifficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Expert = 3
    }

    [Serializable]
    public enum GraphicsQuality
    {
        Low = 0,
        Medium = 1,
        High = 2,
        Ultra = 3
    }

    [Serializable]
    public enum AudioChannel
    {
        Master = 0,
        Music = 1,
        SFX = 2,
        Voice = 3,
        Ambient = 4
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // EXAMPLE CUSTOM CLASS
    // ═════════════════════════════════════════════════════════════════════════════

    [Serializable]
    public class PlayerPreferences
    {
        public string playerName = "Player";
        public int level = 1;
        public float experience = 0f;
        public Color favoriteColor = Color.blue;
        public GameDifficulty difficulty = GameDifficulty.Normal;

        public PlayerPreferences() { }

        public PlayerPreferences(string name, int level, float exp)
        {
            playerName = name;
            this.level = level;
            experience = exp;
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // EXAMPLE CUSTOM STRUCT
    // ═════════════════════════════════════════════════════════════════════════════

    [Serializable]
    public struct Resolution2D
    {
        public int width;
        public int height;
        public bool fullscreen;

        public Resolution2D(int w, int h, bool fs = false)
        {
            width = w;
            height = h;
            fullscreen = fs;
        }

        public override string ToString()
        {
            return $"{width}x{height} {(fullscreen ? "(Fullscreen)" : "(Windowed)")}";
        }
    }

    // ═════════════════════════════════════════════════════════════════════════════
    // EXAMPLE RANGE SETTINGS
    // ═════════════════════════════════════════════════════════════════════════════

    [Serializable]
    public class AudioSettings
    {
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float voiceVolume = 1f;

        public bool muteOnFocusLoss = false;
    }
}
