using System.Collections.Generic;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    [CreateAssetMenu(fileName = "SettingsCategory", menuName = "Alzaki/Global Settings Category", order = 1)]
    public class SettingsCategoryScriptableObject : ScriptableObject
    {
        [SerializeField] public List<IntSetting> intSettings = new List<IntSetting>();
        [SerializeField] public List<FloatSetting> floatSettings = new List<FloatSetting>();
        [SerializeField] public List<StringSetting> stringSettings = new List<StringSetting>();
        [SerializeField] public List<BoolSetting> boolSettings = new List<BoolSetting>();
        [SerializeField] public List<ColorSetting> colorSettings = new List<ColorSetting>();
        [SerializeField] public List<Vector2Setting> vector2Settings = new List<Vector2Setting>();
        [SerializeField] public List<Vector3Setting> vector3Settings = new List<Vector3Setting>();
        [SerializeField] public List<AnimationCurveSetting> curveSettings = new List<AnimationCurveSetting>();
        [SerializeField] public List<EnumSetting> enumSettings = new List<EnumSetting>();
    }
}
