using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
using InputField = TMPro.TMP_InputField;
#endif

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Runtime UI panel that displays all GlobalSettings in a scrollable panel.
    /// Call Show() to display the panel with all settings.
    /// Includes Save and Cancel buttons.
    /// </summary>
    public class GlobalSettingsRuntimePanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [SerializeField] private bool createOnAwake = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F12;

        private GameObject _panelRoot;
        private bool _isVisible = false;
        private Canvas _canvas;
        private Dictionary<string, InputField> _intInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _floatInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _stringInputs = new Dictionary<string, InputField>();
        private Dictionary<string, Toggle> _boolToggles = new Dictionary<string, Toggle>();
        private Dictionary<string, Image> _colorPreviews = new Dictionary<string, Image>();
        private Dictionary<string, InputField> _vector2XInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _vector2YInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _vector3XInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _vector3YInputs = new Dictionary<string, InputField>();
        private Dictionary<string, InputField> _vector3ZInputs = new Dictionary<string, InputField>();
        private Dictionary<string, Dropdown> _enumDropdowns = new Dictionary<string, Dropdown>();

        private GlobalSettings _originalSettings;
        private GameObject _colorPickerPanel;
        private string _currentColorKey;
        private Image _colorPickerPreview;
        private Slider _redSlider, _greenSlider, _blueSlider, _alphaSlider;
        private InputField _redValueField, _greenValueField, _blueValueField, _alphaValueField;
        private InputField _hexCodeField;
        private Font _calibriFont;
#if TMP_PRESENT
        private TMP_FontAsset _calibriTMPFont;
#endif
        private Dictionary<string, int> _tempInts = new Dictionary<string, int>();
        private Dictionary<string, float> _tempFloats = new Dictionary<string, float>();
        private Dictionary<string, string> _tempStrings = new Dictionary<string, string>();
        private Dictionary<string, bool> _tempBools = new Dictionary<string, bool>();
        private Dictionary<string, Color> _tempColors = new Dictionary<string, Color>();
        private Dictionary<string, Vector2> _tempVector2s = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> _tempVector3s = new Dictionary<string, Vector3>();
        private Dictionary<string, EnumSetting> _tempEnums = new Dictionary<string, EnumSetting>();

        // ═════════════════════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═════════════════════════════════════════════════════════════════════════════

        private void Awake()
        {
            _originalSettings = GlobalSettingsManager.Settings;

            _calibriFont = Resources.Load<Font>("Fonts/Calibri");
            if (_calibriFont == null)
            {
                Debug.LogWarning("[GlobalSettingsRuntimePanel] Calibri font not found at 'Resources/Fonts/Calibri'. Using Arial fallback.");
                _calibriFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

#if TMP_PRESENT
            _calibriTMPFont = Resources.Load<TMP_FontAsset>("Fonts/Calibri SDF");
            if (_calibriTMPFont == null)
            {
                _calibriTMPFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
            }
#endif

            if (createOnAwake)
            {
                CreatePanel();
            }
        }

        private void Update()
        {
            if (IsToggleKeyPressed())
            {
                Toggle();
            }

            UpdateDynamicPanelSize();
        }

        private void UpdateDynamicPanelSize()
        {
            if (_panelRoot == null || _canvas == null) return;

            RectTransform panelRt = (RectTransform)_panelRoot.transform;
            RectTransform canvasRt = (RectTransform)_canvas.transform;

            float maxWidth = canvasRt.rect.width * 0.95f;
            float maxHeight = canvasRt.rect.height * 0.95f;

            // Start with 80% of screen height, but at least 700, and never exceeding screen
            float targetHeight = canvasRt.rect.height * 0.8f;
            targetHeight = Mathf.Max(targetHeight, 700f);
            targetHeight = Mathf.Min(targetHeight, maxHeight);

            // Calculate proportional width (8:7)
            float targetWidth = targetHeight * (800f / 700f);

            // Never exceed screen width
            targetWidth = Mathf.Min(targetWidth, maxWidth);

            // But if the proportional width is too squished, force it to be at least 800 
            // (as long as it fits on the screen!)
            targetWidth = Mathf.Max(targetWidth, Mathf.Min(800f, maxWidth));

            panelRt.sizeDelta = new Vector2(targetWidth, targetHeight);
        }

        private bool IsToggleKeyPressed()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(toggleKey);
#else
            return CheckNewInputSystem();
#endif
        }

        private Type _keyboardType;
        private PropertyInfo _currentKeyboardProperty;
        private PropertyInfo _keyProperty;
        private PropertyInfo _wasPressedProperty;
        private bool _inputSystemInitialized = false;

        private void InitializeInputSystemReflection()
        {
            _inputSystemInitialized = true;
            try
            {
                var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    if (assembly.GetName().Name == "Unity.InputSystem")
                    {
                        _keyboardType = assembly.GetType("UnityEngine.InputSystem.Keyboard");
                        if (_keyboardType == null) return;

                        _currentKeyboardProperty = _keyboardType.GetProperty("current", BindingFlags.Public | BindingFlags.Static);
                        if (_currentKeyboardProperty == null) return;

                        string keyName = GetInputSystemKeyName(toggleKey);
                        if (string.IsNullOrEmpty(keyName)) return;

                        _keyProperty = _keyboardType.GetProperty(keyName, BindingFlags.Public | BindingFlags.Instance);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[GlobalSettingsRuntimePanel] Input check init failed: " + ex.Message);
            }
        }

        private bool CheckNewInputSystem()
        {
            if (!_inputSystemInitialized)
            {
                InitializeInputSystemReflection();
            }

            if (_currentKeyboardProperty == null || _keyProperty == null) return false;

            try
            {
                var keyboard = _currentKeyboardProperty.GetValue(null);
                if (keyboard == null) return false;

                var key = _keyProperty.GetValue(keyboard);
                if (key == null) return false;

                if (_wasPressedProperty == null)
                {
                    _wasPressedProperty = key.GetType().GetProperty("wasPressedThisFrame", BindingFlags.Public | BindingFlags.Instance);
                }

                if (_wasPressedProperty == null) return false;

                return (bool)_wasPressedProperty.GetValue(key);
            }
            catch
            {
                return false;
            }
        }

        private string GetInputSystemKeyName(KeyCode keyCode)
        {
            return keyCode switch
            {
                KeyCode.F1 => "f1Key",
                KeyCode.F2 => "f2Key",
                KeyCode.F3 => "f3Key",
                KeyCode.F4 => "f4Key",
                KeyCode.F5 => "f5Key",
                KeyCode.F6 => "f6Key",
                KeyCode.F7 => "f7Key",
                KeyCode.F8 => "f8Key",
                KeyCode.F9 => "f9Key",
                KeyCode.F10 => "f10Key",
                KeyCode.F11 => "f11Key",
                KeyCode.F12 => "f12Key",
                KeyCode.Escape => "escapeKey",
                KeyCode.BackQuote => "backquoteKey",
                KeyCode.Tab => "tabKey",
                KeyCode.Space => "spaceKey",
                KeyCode.Return => "enterKey",
                _ => null
            };
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═════════════════════════════════════════════════════════════════════════════

        public void Show()
        {
            if (_panelRoot == null)
                CreatePanel();

            _panelRoot.SetActive(true);
            _isVisible = true;
            LoadCurrentValues();
        }

        public void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);

            _isVisible = false;
        }

        public void Toggle()
        {
            if (_isVisible)
                Hide();
            else
                Show();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // PANEL CREATION
        // ═════════════════════════════════════════════════════════════════════════════

        private void CreatePanel()
        {
            EnsureEventSystem();

            GameObject canvasObj = new GameObject("GlobalSettingsCanvas");
            canvasObj.transform.SetParent(transform, false);
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            _panelRoot = CreatePanelBackground(canvasObj.transform);

            GameObject scrollView = CreateScrollView(_panelRoot.transform);

            Transform content = scrollView.transform.Find("Viewport/Content");
            PopulateContent(content);

            CreateButtons(_panelRoot.transform);

            _panelRoot.SetActive(false);

            Debug.Log("[GlobalSettingsRuntimePanel] Panel created successfully!");
        }

        private void EnsureEventSystem()
        {
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[GlobalSettingsRuntimePanel] EventSystem created!");
            }
        }

        private GameObject CreatePanelBackground(Transform parent)
        {
            GameObject panel = new GameObject("SettingsPanel", typeof(RectTransform));
            panel.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)panel.transform;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(800, 700);
            rt.anchoredPosition = Vector2.zero;

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);

            CreateHeader(panel.transform);

            return panel;
        }

        private void CreateHeader(Transform parent)
        {
            GameObject header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)header.transform;
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 60);
            rt.anchoredPosition = Vector2.zero;

            Image bg = header.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            GameObject titleObj = new GameObject("Title", typeof(RectTransform));
            titleObj.transform.SetParent(header.transform, false);

            RectTransform titleRt = (RectTransform)titleObj.transform;
            titleRt.anchorMin = Vector2.zero;
            titleRt.anchorMax = Vector2.one;
            titleRt.offsetMin = new Vector2(20, 0);
            titleRt.offsetMax = new Vector2(-20, 0);

#if TMP_PRESENT
            TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
            title.text = "Global Settings";
            title.fontSize = 32;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Center;
            title.color = Color.white;
            if (_calibriTMPFont != null)
                title.font = _calibriTMPFont;
#else
            Text title = titleObj.AddComponent<Text>();
            title.text = "Global Settings";
            title.fontSize = 32;
            title.fontStyle = FontStyle.Bold;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;
            title.font = _calibriFont;
#endif
        }

        private GameObject CreateScrollView(Transform parent)
        {
            GameObject scrollView = new GameObject("ScrollView", typeof(RectTransform));
            scrollView.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)scrollView.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(10, 70);
            rt.offsetMax = new Vector2(-10, -60);

            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform vpRt = (RectTransform)viewport.transform;
            vpRt.anchorMin = Vector2.zero;
            vpRt.anchorMax = Vector2.one;
            vpRt.offsetMin = Vector2.zero;
            vpRt.offsetMax = Vector2.zero;

            Image vpImage = viewport.AddComponent<Image>();
            vpImage.color = new Color(1f, 1f, 1f, 1f);

            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRt = (RectTransform)content.transform;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0);

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.spacing = 10;
            vlg.padding = new RectOffset(20, 20, 20, 20);

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = vpRt;
            scroll.content = contentRt;

            return scrollView;
        }

        private void PopulateContent(Transform content)
        {
            if (_originalSettings == null)
            {
                CreateLabelInContent(content, "No GlobalSettings found!");
                return;
            }

            int totalFieldsAdded = 0;

            if (_originalSettings.categories != null)
            {
                foreach (var category in _originalSettings.categories)
                {
                    bool hasItems = false;
                    if (category.intSettings.Count > 0 || category.floatSettings.Count > 0 || category.stringSettings.Count > 0 ||
                        category.boolSettings.Count > 0 || category.colorSettings.Count > 0 || category.vector2Settings.Count > 0 ||
                        category.vector3Settings.Count > 0 || category.enumSettings.Count > 0)
                    {
                        hasItems = true;
                    }

                    if (hasItems)
                    {
                        CreateSectionHeader(content, category.categoryName);

                        foreach (var s in category.intSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateIntField(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.floatSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateFloatField(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.stringSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateStringField(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.boolSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateBoolField(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.colorSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateColorField(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.vector2Settings) { if (!string.IsNullOrEmpty(s.key)) { CreateVector2Field(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.vector3Settings) { if (!string.IsNullOrEmpty(s.key)) { CreateVector3Field(content, s.key, s.value); totalFieldsAdded++; } }
                        foreach (var s in category.enumSettings) { if (!string.IsNullOrEmpty(s.key)) { CreateEnumField(content, s); totalFieldsAdded++; } }
                    }
                }
            }

            if (totalFieldsAdded == 0)
            {
                CreateLabelInContent(content, "No settings found! Add settings to GlobalSettings asset.");
            }
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // UI ELEMENT CREATION
        // ═════════════════════════════════════════════════════════════════════════════

        private void CreateSectionHeader(Transform parent, string text)
        {
            GameObject header = new GameObject("SectionHeader_" + text, typeof(RectTransform));
            header.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)header.transform;
            rt.sizeDelta = new Vector2(0, 40);

#if TMP_PRESENT
            TextMeshProUGUI textComp = header.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.fontStyle = FontStyles.Bold;
            textComp.color = new Color(0.4f, 0.8f, 1f);
            if (_calibriTMPFont != null)
                textComp.font = _calibriTMPFont;
#else
            Text textComp = header.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.fontStyle = FontStyle.Bold;
            textComp.color = new Color(0.4f, 0.8f, 1f);
            textComp.font = _calibriFont;
#endif

            LayoutElement le = header.AddComponent<LayoutElement>();
            le.minHeight = 40;
        }

        private void CreateLabelInContent(Transform parent, string text)
        {
            GameObject label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(parent, false);

#if TMP_PRESENT
            TextMeshProUGUI textComp = label.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = 18;
            textComp.color = Color.white;
            if (_calibriTMPFont != null)
                textComp.font = _calibriTMPFont;
#else
            Text textComp = label.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 18;
            textComp.color = Color.white;
            textComp.font = _calibriFont;
#endif

            LayoutElement le = label.AddComponent<LayoutElement>();
            le.minHeight = 30;
        }

        private void CreateIntField(Transform parent, string key, int value)
        {
            GameObject field = CreateFieldRow(parent, key);
            InputField input = CreateInputField(field.transform);
            input.text = value.ToString();
            input.contentType = InputField.ContentType.IntegerNumber;

            _intInputs[key] = input;
            _tempInts[key] = value;

            input.onValueChanged.AddListener((newValue) =>
            {
                if (int.TryParse(newValue, out int parsed))
                    _tempInts[key] = parsed;
            });
        }

        private void CreateFloatField(Transform parent, string key, float value)
        {
            GameObject field = CreateFieldRow(parent, key);
            InputField input = CreateInputField(field.transform);
            input.text = value.ToString();
            input.contentType = InputField.ContentType.DecimalNumber;

            _floatInputs[key] = input;
            _tempFloats[key] = value;

            input.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempFloats[key] = parsed;
            });
        }

        private void CreateStringField(Transform parent, string key, string value)
        {
            GameObject field = CreateFieldRow(parent, key);
            InputField input = CreateInputField(field.transform);
            input.text = value;
            input.contentType = InputField.ContentType.Standard;

            _stringInputs[key] = input;
            _tempStrings[key] = value;

            input.onValueChanged.AddListener((newValue) =>
            {
                _tempStrings[key] = newValue;
            });
        }

        private void CreateBoolField(Transform parent, string key, bool value)
        {
            GameObject field = CreateFieldRow(parent, key);
            Toggle toggle = CreateToggle(field.transform);
            toggle.isOn = value;

            _boolToggles[key] = toggle;
            _tempBools[key] = value;

            toggle.onValueChanged.AddListener((newValue) =>
            {
                _tempBools[key] = newValue;
            });
        }

        private void CreateColorField(Transform parent, string key, Color value)
        {
            GameObject field = CreateFieldRow(parent, key);

            // Create container
            GameObject container = new GameObject("ColorContainer", typeof(RectTransform));
            container.transform.SetParent(field.transform, false);

            RectTransform containerRt = (RectTransform)container.transform;
            containerRt.sizeDelta = new Vector2(350, 30);

            LayoutElement containerLe = container.AddComponent<LayoutElement>();
            containerLe.flexibleWidth = 1;
            containerLe.preferredHeight = 30;

            // Create color box
            GameObject colorBox = new GameObject("ColorPreview", typeof(RectTransform));
            colorBox.transform.SetParent(container.transform, false);

            RectTransform colorRt = (RectTransform)colorBox.transform;
            colorRt.anchorMin = Vector2.zero;
            colorRt.anchorMax = Vector2.one;
            colorRt.sizeDelta = Vector2.zero;

            Image colorImage = colorBox.AddComponent<Image>();
            colorImage.color = value;

            // Add border frame on top (4 separate borders that won't blend with alpha)
            Color borderColor = new Color(0.6f, 0.6f, 0.6f, 1f);
            float borderWidth = 2f;

            // Top border
            CreateBorderLine(container.transform, "BorderTop", borderColor, borderWidth,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -borderWidth), Vector2.zero);

            // Bottom border
            CreateBorderLine(container.transform, "BorderBottom", borderColor, borderWidth,
                new Vector2(0, 0), new Vector2(1, 0), Vector2.zero, new Vector2(0, borderWidth));

            // Left border
            CreateBorderLine(container.transform, "BorderLeft", borderColor, borderWidth,
                new Vector2(0, 0), new Vector2(0, 1), Vector2.zero, new Vector2(borderWidth, 0));

            // Right border
            CreateBorderLine(container.transform, "BorderRight", borderColor, borderWidth,
                new Vector2(1, 0), new Vector2(1, 1), new Vector2(-borderWidth, 0), Vector2.zero);

            Button colorButton = container.AddComponent<Button>();
            colorButton.targetGraphic = colorImage;
            colorButton.onClick.AddListener(() => ShowColorPicker(key, _tempColors[key]));

            _colorPreviews[key] = colorImage;
            _tempColors[key] = value;
        }

        private void CreateBorderLine(Transform parent, string name, Color color, float thickness,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject border = new GameObject(name, typeof(RectTransform));
            border.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)border.transform;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;

            Image img = border.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false; // Don't block button clicks
        }

        private void CreateVector2Field(Transform parent, string key, Vector2 value)
        {
            GameObject field = CreateFieldRow(parent, key);

            InputField inputX = CreateSmallInputField(field.transform, "X");
            inputX.text = value.x.ToString();
            inputX.contentType = InputField.ContentType.DecimalNumber;

            InputField inputY = CreateSmallInputField(field.transform, "Y");
            inputY.text = value.y.ToString();
            inputY.contentType = InputField.ContentType.DecimalNumber;

            _vector2XInputs[key] = inputX;
            _vector2YInputs[key] = inputY;
            _tempVector2s[key] = value;

            inputX.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempVector2s[key] = new Vector2(parsed, _tempVector2s[key].y);
            });

            inputY.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempVector2s[key] = new Vector2(_tempVector2s[key].x, parsed);
            });
        }

        private void CreateVector3Field(Transform parent, string key, Vector3 value)
        {
            GameObject field = CreateFieldRow(parent, key);

            InputField inputX = CreateSmallInputField(field.transform, "X");
            inputX.text = value.x.ToString();
            inputX.contentType = InputField.ContentType.DecimalNumber;

            InputField inputY = CreateSmallInputField(field.transform, "Y");
            inputY.text = value.y.ToString();
            inputY.contentType = InputField.ContentType.DecimalNumber;

            InputField inputZ = CreateSmallInputField(field.transform, "Z");
            inputZ.text = value.z.ToString();
            inputZ.contentType = InputField.ContentType.DecimalNumber;

            _vector3XInputs[key] = inputX;
            _vector3YInputs[key] = inputY;
            _vector3ZInputs[key] = inputZ;
            _tempVector3s[key] = value;

            inputX.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempVector3s[key] = new Vector3(parsed, _tempVector3s[key].y, _tempVector3s[key].z);
            });

            inputY.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempVector3s[key] = new Vector3(_tempVector3s[key].x, parsed, _tempVector3s[key].z);
            });

            inputZ.onValueChanged.AddListener((newValue) =>
            {
                if (float.TryParse(newValue, out float parsed))
                    _tempVector3s[key] = new Vector3(_tempVector3s[key].x, _tempVector3s[key].y, parsed);
            });
        }

        private void CreateEnumField(Transform parent, EnumSetting setting)
        {
            string key = setting.key;
            Type enumType = null;

            if (!string.IsNullOrEmpty(setting.enumTypeName))
            {
                enumType = Type.GetType(setting.enumTypeName);
            }

            if (enumType == null || !enumType.IsEnum)
            {
                // Can't display unknown enum type - show as label
                GameObject field = CreateFieldRow(parent, key);
                CreateEnumLabel(field.transform, $"Unknown enum type: {setting.enumTypeName ?? "null"}");
                return;
            }

            GameObject fieldRow = CreateFieldRow(parent, key);

            // Create dropdown
            GameObject dropdownObj = new GameObject("EnumDropdown", typeof(RectTransform));
            dropdownObj.transform.SetParent(fieldRow.transform, false);

            RectTransform rt = (RectTransform)dropdownObj.transform;
            rt.sizeDelta = new Vector2(350, 40);

            Image bg = dropdownObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            Dropdown dropdown = dropdownObj.AddComponent<Dropdown>();

            // Create dropdown label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform labelRt = (RectTransform)labelObj.transform;
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = new Vector2(10, 0);
            labelRt.offsetMax = new Vector2(-25, 0);

            Text labelText = labelObj.AddComponent<Text>();
            labelText.font = _calibriFont;
            labelText.fontSize = 18;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;

            // Create arrow indicator
            GameObject arrowObj = new GameObject("Arrow", typeof(RectTransform));
            arrowObj.transform.SetParent(dropdownObj.transform, false);

            RectTransform arrowRt = (RectTransform)arrowObj.transform;
            arrowRt.anchorMin = new Vector2(1, 0.5f);
            arrowRt.anchorMax = new Vector2(1, 0.5f);
            arrowRt.sizeDelta = new Vector2(20, 20);
            arrowRt.anchoredPosition = new Vector2(-15, 0);

            Text arrowText = arrowObj.AddComponent<Text>();
            arrowText.text = "▼";
            arrowText.font = _calibriFont;
            arrowText.fontSize = 16;
            arrowText.color = Color.white;
            arrowText.alignment = TextAnchor.MiddleCenter;

            // Create template
            GameObject template = CreateDropdownTemplate(dropdownObj.transform);

            // Wire up dropdown
            dropdown.targetGraphic = bg;
            dropdown.template = (RectTransform)template.transform;
            dropdown.captionText = labelText;
            dropdown.itemText = template.transform.Find("Viewport/Content/Item/Item Label").GetComponent<Text>();

            template.SetActive(false);

            // Populate dropdown with enum values
            dropdown.ClearOptions();
            string[] enumNames = Enum.GetNames(enumType);
            List<Dropdown.OptionData> options = new List<Dropdown.OptionData>();

            foreach (string name in enumNames)
            {
                options.Add(new Dropdown.OptionData(name));
            }

            dropdown.AddOptions(options);

            // Set current value
            int currentIndex = Array.IndexOf(Enum.GetValues(enumType), setting.GetEnumValue());
            if (currentIndex < 0) currentIndex = 0;
            dropdown.value = currentIndex;
            dropdown.RefreshShownValue();

            // Store references
            _enumDropdowns[key] = dropdown;

            // Create a copy of the setting for temp storage
            var tempSetting = new EnumSetting
            {
                key = setting.key,
                enumTypeName = setting.enumTypeName,
                intValue = setting.intValue
            };
            _tempEnums[key] = tempSetting;

            // Capture enumType for the listener
            Type capturedEnumType = enumType;

            dropdown.onValueChanged.AddListener((index) =>
            {
                object newValue = Enum.GetValues(capturedEnumType).GetValue(index);
                _tempEnums[key].intValue = Convert.ToInt32(newValue);
            });

            LayoutElement le = dropdownObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
        }

        private void CreateEnumLabel(Transform parent, string text)
        {
            GameObject labelObj = new GameObject("EnumLabel");
            labelObj.transform.SetParent(parent, false);

#if TMP_PRESENT
            TextMeshProUGUI textComp = labelObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.font = _calibriTMPFont;
            textComp.fontSize = 14;
            textComp.color = new Color(0.8f, 0.5f, 0.5f);
            textComp.fontStyle = FontStyles.Italic;
#else
            Text textComp = labelObj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = _calibriFont;
            textComp.fontSize = 14;
            textComp.color = new Color(0.8f, 0.5f, 0.5f);
            textComp.fontStyle = FontStyle.Italic;
#endif

            LayoutElement le = labelObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
        }

        private GameObject CreateDropdownTemplate(Transform parent)
        {
            GameObject template = new GameObject("Template", typeof(RectTransform));
            template.transform.SetParent(parent, false);

            RectTransform templateRt = (RectTransform)template.transform;
            templateRt.anchorMin = new Vector2(0, 0);
            templateRt.anchorMax = new Vector2(1, 0);
            templateRt.pivot = new Vector2(0.5f, 1);
            templateRt.anchoredPosition = new Vector2(0, 2);
            templateRt.sizeDelta = new Vector2(0, 150);

            Image templateBg = template.AddComponent<Image>();
            templateBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            ScrollRect templateScroll = template.AddComponent<ScrollRect>();
            templateScroll.horizontal = false;
            templateScroll.vertical = true;

            // Viewport
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(template.transform, false);

            RectTransform viewportRt = (RectTransform)viewport.transform;
            viewportRt.anchorMin = Vector2.zero;
            viewportRt.anchorMax = Vector2.one;
            viewportRt.sizeDelta = Vector2.zero;
            viewportRt.pivot = new Vector2(0, 1);

            Image viewportImg = viewport.AddComponent<Image>();
            viewportImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);

            RectTransform contentRt = (RectTransform)content.transform;
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 28);

            // Item
            GameObject item = new GameObject("Item", typeof(RectTransform));
            item.transform.SetParent(content.transform, false);

            RectTransform itemRt = (RectTransform)item.transform;
            itemRt.anchorMin = new Vector2(0, 0.5f);
            itemRt.anchorMax = new Vector2(1, 0.5f);
            itemRt.sizeDelta = new Vector2(0, 25);

            Toggle itemToggle = item.AddComponent<Toggle>();

            // Item background
            GameObject itemBg = new GameObject("Item Background", typeof(RectTransform));
            itemBg.transform.SetParent(item.transform, false);

            RectTransform itemBgRt = (RectTransform)itemBg.transform;
            itemBgRt.anchorMin = Vector2.zero;
            itemBgRt.anchorMax = Vector2.one;
            itemBgRt.sizeDelta = Vector2.zero;

            Image itemBgImg = itemBg.AddComponent<Image>();
            itemBgImg.color = new Color(0.25f, 0.25f, 0.25f, 1f);

            itemToggle.targetGraphic = itemBgImg;

            // Item checkmark
            GameObject checkmark = new GameObject("Item Checkmark", typeof(RectTransform));
            checkmark.transform.SetParent(item.transform, false);

            RectTransform checkRt = (RectTransform)checkmark.transform;
            checkRt.anchorMin = new Vector2(0, 0.5f);
            checkRt.anchorMax = new Vector2(0, 0.5f);
            checkRt.sizeDelta = new Vector2(20, 20);
            checkRt.anchoredPosition = new Vector2(10, 0);

            Text checkText = checkmark.AddComponent<Text>();
            checkText.text = "✓";
            checkText.font = _calibriFont;
            checkText.fontSize = 18;
            checkText.fontStyle = FontStyle.Bold;
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            checkText.alignment = TextAnchor.MiddleCenter;

            itemToggle.graphic = checkText;

            // Item label
            GameObject itemLabel = new GameObject("Item Label", typeof(RectTransform));
            itemLabel.transform.SetParent(item.transform, false);

            RectTransform itemLabelRt = (RectTransform)itemLabel.transform;
            itemLabelRt.anchorMin = Vector2.zero;
            itemLabelRt.anchorMax = Vector2.one;
            itemLabelRt.offsetMin = new Vector2(25, 1);
            itemLabelRt.offsetMax = new Vector2(-10, -2);

            Text itemLabelText = itemLabel.AddComponent<Text>();
            itemLabelText.font = _calibriFont;
            itemLabelText.fontSize = 16;
            itemLabelText.color = Color.white;
            itemLabelText.alignment = TextAnchor.MiddleLeft;

            templateScroll.content = contentRt;
            templateScroll.viewport = viewportRt;

            return template;
        }

        private GameObject CreateFieldRow(Transform parent, string labelText)
        {
            GameObject row = new GameObject("Field_" + labelText, typeof(RectTransform));
            row.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)row.transform;
            rt.sizeDelta = new Vector2(0, 40);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlHeight = false;
            hlg.childControlWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 15;

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 40;

            GameObject label = new GameObject("Label", typeof(RectTransform));
            label.transform.SetParent(row.transform, false);
            RectTransform labelRt = (RectTransform)label.transform;
            labelRt.sizeDelta = new Vector2(370, 40);

#if TMP_PRESENT
            TextMeshProUGUI text = label.AddComponent<TextMeshProUGUI>();
            text.text = labelText;
            text.font = _calibriTMPFont;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.MidlineLeft;
#else
            Text text = label.AddComponent<Text>();
            text.text = labelText;
            text.font = _calibriFont;
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;
#endif

            LayoutElement labelLe = label.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1;

            return row;
        }

        private InputField CreateInputField(Transform parent)
        {
            GameObject inputObj = new GameObject("InputField", typeof(RectTransform));
            inputObj.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)inputObj.transform;
            rt.sizeDelta = new Vector2(350, 40);

            Image bg = inputObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            InputField input = inputObj.AddComponent<InputField>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRt = (RectTransform)textObj.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 0);
            textRt.offsetMax = new Vector2(-5, 0);

#if TMP_PRESENT
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.font = _calibriTMPFont;
            textComp.fontSize = 18;
            textComp.color = Color.white;
            textComp.alignment = TextAlignmentOptions.MidlineLeft;
            textComp.richText = false;
#else
            Text textComp = textObj.AddComponent<Text>();
            textComp.font = _calibriFont;
            textComp.fontSize = 18;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleLeft;
            textComp.supportRichText = false;
#endif

            input.textComponent = textComp;

            LayoutElement le = inputObj.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;

            return input;
        }

        private InputField CreateSmallInputField(Transform parent, string label)
        {
            GameObject container = new GameObject("Input_" + label, typeof(RectTransform));
            container.transform.SetParent(parent, false);

            RectTransform containerRt = (RectTransform)container.transform;
            containerRt.sizeDelta = new Vector2(100, 40);

            HorizontalLayoutGroup hlg = container.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            LayoutElement containerLe = container.AddComponent<LayoutElement>();
            containerLe.preferredWidth = 100;
            containerLe.preferredHeight = 40;

            // Label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(container.transform, false);

#if TMP_PRESENT
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.font = _calibriTMPFont;
            labelText.fontSize = 18;
            labelText.color = Color.gray;
            labelText.alignment = TextAlignmentOptions.Center;
#else
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label;
            labelText.font = _calibriFont;
            labelText.fontSize = 18;
            labelText.color = Color.gray;
            labelText.alignment = TextAnchor.MiddleCenter;
#endif

            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 15;

            // Input
            GameObject inputObj = new GameObject("Input", typeof(RectTransform));
            inputObj.transform.SetParent(container.transform, false);

            Image bg = inputObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            InputField input = inputObj.AddComponent<InputField>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(inputObj.transform, false);

            RectTransform textRt = (RectTransform)textObj.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(3, 0);
            textRt.offsetMax = new Vector2(-3, 0);

#if TMP_PRESENT
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.font = _calibriTMPFont;
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.alignment = TextAlignmentOptions.Center;
#else
            Text textComp = textObj.AddComponent<Text>();
            textComp.font = _calibriFont;
            textComp.fontSize = 14;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
#endif

            input.textComponent = textComp;

            LayoutElement inputLe = inputObj.AddComponent<LayoutElement>();
            inputLe.preferredWidth = 70;

            return input;
        }

        private Toggle CreateToggle(Transform parent)
        {
            GameObject toggleObj = new GameObject("Toggle", typeof(RectTransform));
            toggleObj.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)toggleObj.transform;
            rt.sizeDelta = new Vector2(40, 40);

            Toggle toggle = toggleObj.AddComponent<Toggle>();

            GameObject bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(toggleObj.transform, false);

            RectTransform bgRt = (RectTransform)bg.transform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            toggle.targetGraphic = bgImg;

            GameObject checkmark = new GameObject("Checkmark", typeof(RectTransform));
            checkmark.transform.SetParent(bg.transform, false);

            RectTransform checkRt = (RectTransform)checkmark.transform;
            checkRt.anchorMin = Vector2.zero;
            checkRt.anchorMax = Vector2.one;
            checkRt.offsetMin = Vector2.zero;
            checkRt.offsetMax = Vector2.zero;

#if TMP_PRESENT
            TextMeshProUGUI checkText = checkmark.AddComponent<TextMeshProUGUI>();
            checkText.text = "X";
            checkText.font = _calibriTMPFont;
            checkText.fontSize = 32;
            checkText.fontStyle = FontStyles.Bold;
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            checkText.alignment = TextAlignmentOptions.Center;
            toggle.graphic = checkText;
#else
            Text checkText = checkmark.AddComponent<Text>();
            checkText.text = "X";
            checkText.font = _calibriFont;
            checkText.fontSize = 32;
            checkText.fontStyle = FontStyle.Bold;
            checkText.color = new Color(0.3f, 0.9f, 0.3f);
            checkText.alignment = TextAnchor.MiddleCenter;
            toggle.graphic = checkText;
#endif

            LayoutElement le = toggleObj.AddComponent<LayoutElement>();
            le.preferredWidth = 40;

            return toggle;
        }

        private void CreateButtons(Transform parent)
        {
            GameObject buttonPanel = new GameObject("ButtonPanel", typeof(RectTransform));
            buttonPanel.transform.SetParent(parent, false);

            RectTransform rt = (RectTransform)buttonPanel.transform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.sizeDelta = new Vector2(0, 60);
            rt.anchoredPosition = Vector2.zero;

            HorizontalLayoutGroup hlg = buttonPanel.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;
            hlg.childForceExpandHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.spacing = 20;
            hlg.padding = new RectOffset(20, 20, 10, 10);

            CreateButton(buttonPanel.transform, "Save", new Color(0.3f, 0.8f, 0.3f), OnSave);
            CreateButton(buttonPanel.transform, "Cancel", new Color(0.8f, 0.3f, 0.3f), OnCancel);

            var buttonPanelRT = (RectTransform)buttonPanel.transform;
            buttonPanelRT.sizeDelta = new Vector2(buttonPanelRT.sizeDelta.x, 80);
        }

        private void CreateButton(Transform parent, string text, Color color, UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonObj = new GameObject("Button_" + text, typeof(RectTransform));
            buttonObj.transform.SetParent(parent, false);

            Image img = buttonObj.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = true;

            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = img;
            button.onClick.AddListener(callback);

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(buttonObj.transform, false);

            RectTransform textRt = (RectTransform)textObj.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

#if TMP_PRESENT
            TextMeshProUGUI textComp = textObj.AddComponent<TextMeshProUGUI>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.fontStyle = FontStyles.Bold;
            textComp.alignment = TextAlignmentOptions.Center;
            textComp.color = Color.white;
            textComp.raycastTarget = false;
            if (_calibriTMPFont != null)
                textComp.font = _calibriTMPFont;
#else
            Text textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.fontSize = 24;
            textComp.fontStyle = FontStyle.Bold;
            textComp.alignment = TextAnchor.MiddleCenter;
            textComp.color = Color.white;
            textComp.raycastTarget = false;
            textComp.font = _calibriFont;
#endif
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // COLOR PICKER (simplified)
        // ═════════════════════════════════════════════════════════════════════════════

        private void ShowColorPicker(string key, Color currentColor)
        {
            _currentColorKey = key;

            if (_colorPickerPanel == null)
                CreateColorPickerPanel();

            UpdateColorPickerFromColor(currentColor);
            _colorPickerPanel.SetActive(true);
        }

        // private void CreateColorPickerPanel()
        // {
        //     _colorPickerPanel = new GameObject("ColorPickerPanel", typeof(RectTransform));
        //     _colorPickerPanel.transform.SetParent(_canvas.transform, false);

        //     RectTransform panelRt = (RectTransform)_colorPickerPanel.transform;
        //     panelRt.anchorMin = Vector2.zero;
        //     panelRt.anchorMax = Vector2.one;
        //     panelRt.offsetMin = Vector2.zero;
        //     panelRt.offsetMax = Vector2.zero;

        //     Image overlay = _colorPickerPanel.AddComponent<Image>();
        //     overlay.color = new Color(0, 0, 0, 0.7f);

        //     GameObject pickerContent = new GameObject("PickerContent", typeof(RectTransform));
        //     pickerContent.transform.SetParent(_colorPickerPanel.transform, false);

        //     RectTransform contentRt = (RectTransform)pickerContent.transform;
        //     contentRt.anchorMin = new Vector2(0.5f, 0.5f);
        //     contentRt.anchorMax = new Vector2(0.5f, 0.5f);
        //     contentRt.sizeDelta = new Vector2(400, 350);
        //     contentRt.anchoredPosition = Vector2.zero;

        //     Image contentBg = pickerContent.AddComponent<Image>();
        //     contentBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        //     VerticalLayoutGroup vlg = pickerContent.AddComponent<VerticalLayoutGroup>();
        //     vlg.padding = new RectOffset(15, 15, 15, 15);
        //     vlg.spacing = 15;
        //     vlg.childControlHeight = false;
        //     vlg.childControlWidth = true;
        //     vlg.childForceExpandWidth = true;

        //     // Preview
        //     GameObject previewObj = new GameObject("Preview", typeof(RectTransform));
        //     previewObj.transform.SetParent(pickerContent.transform, false);

        //     _colorPickerPreview = previewObj.AddComponent<Image>();
        //     _colorPickerPreview.color = Color.white;

        //     LayoutElement previewLe = previewObj.AddComponent<LayoutElement>();
        //     previewLe.preferredHeight = 80;

        //     // Sliders
        //     _redSlider = CreateColorSlider(pickerContent.transform, "R", Color.red, out _redValueField);
        //     _greenSlider = CreateColorSlider(pickerContent.transform, "G", Color.green, out _greenValueField);
        //     _blueSlider = CreateColorSlider(pickerContent.transform, "B", Color.blue, out _blueValueField);
        //     _alphaSlider = CreateColorSlider(pickerContent.transform, "A", Color.white, out _alphaValueField);

        //     // Hex field
        //     CreateHexCodeField(pickerContent.transform);

        //     // Buttons
        //     GameObject buttonRow = new GameObject("Buttons");
        //     buttonRow.transform.SetParent(pickerContent.transform, false);

        //     HorizontalLayoutGroup hlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
        //     hlg.spacing = 10;
        //     hlg.childControlWidth = true;
        //     hlg.childForceExpandWidth = true;
        //     hlg.childControlHeight = true;

        //     LayoutElement rowLe = buttonRow.AddComponent<LayoutElement>();
        //     rowLe.minHeight = 35;

        //     CreateButton(buttonRow.transform, "OK", new Color(0.3f, 0.8f, 0.3f), OnColorPickerOK);
        //     CreateButton(buttonRow.transform, "Cancel", new Color(0.8f, 0.3f, 0.3f), OnColorPickerCancel);

        //     _colorPickerPanel.SetActive(false);
        // }

        private void CreateColorPickerPanel()
        {
            // Create overlay panel
            _colorPickerPanel = new GameObject("ColorPickerPanel", typeof(RectTransform));
            _colorPickerPanel.transform.SetParent(_canvas.transform, false);

            RectTransform panelRt = (RectTransform)_colorPickerPanel.transform;
            panelRt.anchorMin = Vector2.zero;
            panelRt.anchorMax = Vector2.one;
            panelRt.offsetMin = Vector2.zero;
            panelRt.offsetMax = Vector2.zero;

            // Semi-transparent background
            Image overlay = _colorPickerPanel.AddComponent<Image>();
            overlay.color = new Color(0, 0, 0, 0.7f);

            // Picker content panel - compact horizontal layout like Unity's
            GameObject pickerContent = new GameObject("PickerContent", typeof(RectTransform));
            pickerContent.transform.SetParent(_colorPickerPanel.transform, false);

            RectTransform contentRt = (RectTransform)pickerContent.transform;
            contentRt.anchorMin = new Vector2(0.5f, 0.5f);
            contentRt.anchorMax = new Vector2(0.5f, 0.5f);
            contentRt.sizeDelta = new Vector2(600, 400);  // Adjusted for hexcode field
            contentRt.anchoredPosition = Vector2.zero;

            Image contentBg = pickerContent.AddComponent<Image>();
            contentBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            VerticalLayoutGroup vlg = pickerContent.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(15, 15, 15, 15);
            vlg.spacing = 20;
            vlg.childControlHeight = false;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;

            // Header
            CreateColorPickerHeader(pickerContent.transform);

            // Horizontal content area (preview on left, sliders on right)
            GameObject contentRow = new GameObject("ContentRow", typeof (RectTransform));
            contentRow.transform.SetParent(pickerContent.transform, false);
            ((RectTransform)contentRow.transform).sizeDelta = new Vector2(570, 130);

            HorizontalLayoutGroup contentHlg = contentRow.AddComponent<HorizontalLayoutGroup>();
            contentHlg.spacing = 15;
            contentHlg.childControlWidth = false;
            contentHlg.childControlHeight = true;
            contentHlg.childForceExpandHeight = true;
            contentHlg.childAlignment = TextAnchor.MiddleCenter;

            LayoutElement contentRowLe = contentRow.AddComponent<LayoutElement>();
            contentRowLe.flexibleHeight = 1;
            contentRowLe.preferredWidth = 570;
            contentRowLe.preferredHeight = 130;

            // Left side: Color preview
            CreateColorPickerPreview(contentRow.transform);

            // Right side: Sliders container
            GameObject slidersContainer = new GameObject("Sliders", typeof (RectTransform));
            slidersContainer.transform.SetParent(contentRow.transform, false);
            ((RectTransform)slidersContainer.transform).sizeDelta = new Vector2(260, 135);

            VerticalLayoutGroup slidersVlg = slidersContainer.AddComponent<VerticalLayoutGroup>();
            slidersVlg.spacing = 5;
            slidersVlg.childControlHeight = true;
            slidersVlg.childControlWidth = true;
            slidersVlg.childForceExpandWidth = true;

            LayoutElement slidersLe = slidersContainer.AddComponent<LayoutElement>();
            slidersLe.flexibleWidth = 1;
            slidersLe.preferredWidth = 260;
            slidersLe.preferredHeight = 135;            

            // RGB Sliders (compact)
            _redSlider = CreateColorSlider(slidersContainer.transform, "R", Color.red, out _redValueField);
            _greenSlider = CreateColorSlider(slidersContainer.transform, "G", Color.green, out _greenValueField);
            _blueSlider = CreateColorSlider(slidersContainer.transform, "B", Color.blue, out _blueValueField);
            _alphaSlider = CreateColorSlider(slidersContainer.transform, "A", Color.white, out _alphaValueField);

            // Hexcode input field
            CreateHexCodeField(pickerContent.transform);

            // Buttons
            CreateColorPickerButtons(pickerContent.transform);

            _colorPickerPanel.SetActive(false);
        }

        private void CreateColorPickerHeader(Transform parent)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(parent, false);

#if TMP_PRESENT
            TextMeshProUGUI text = header.AddComponent<TextMeshProUGUI>();
            text.text = "Color Picker";
            text.font = _calibriTMPFont;
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;
#else
            Text text = header.AddComponent<Text>();
            text.text = "Color Picker";
            text.font = _calibriFont;
            text.fontSize = 20;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
#endif

            LayoutElement le = header.AddComponent<LayoutElement>();
            le.minHeight = 30;  // Smaller header
        }

        private void CreateColorPickerPreview(Transform parent)
        {
            var preview = new GameObject("Preview", typeof (RectTransform));
            preview.transform.SetParent(parent, false);

            _colorPickerPreview = preview.AddComponent<Image>();
            _colorPickerPreview.color = Color.white;

            LayoutElement le = preview.AddComponent<LayoutElement>();
            le.preferredWidth = 130;  // Fixed width for preview
            le.preferredHeight = 130;
            le.flexibleHeight = 1;    // Take full height of parent

            ((RectTransform)preview.transform).sizeDelta = new Vector2(130, 130);
        }

        private void CreateColorPickerButtons(Transform parent)
        {
            var buttonRow = new GameObject("Buttons");
            buttonRow.transform.SetParent(parent, false);

            HorizontalLayoutGroup hlg = buttonRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childForceExpandWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            LayoutElement rowLe = buttonRow.AddComponent<LayoutElement>();
            rowLe.minHeight = 35;  // Smaller buttons

            CreateButton(buttonRow.transform, "OK", new Color(0.3f, 0.8f, 0.3f), OnColorPickerOK);
            CreateButton(buttonRow.transform, "Cancel", new Color(0.8f, 0.3f, 0.3f), OnColorPickerCancel);

            var buttonRowRT = (RectTransform)buttonRow.transform;
            buttonRowRT.sizeDelta = new Vector2(buttonRowRT.sizeDelta.x, 50);
        }

        private Slider CreateColorSlider(Transform parent, string label, Color sliderColor, out InputField valueField)
        {
            GameObject row = new GameObject("Slider_" + label);
            row.transform.SetParent(parent, false);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 5;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = false;

            LayoutElement rowLe = row.AddComponent<LayoutElement>();
            rowLe.minHeight = 30;

            // Label (R, G, B, A)
            GameObject labelObj = new GameObject("Label", typeof (RectTransform));
            labelObj.transform.SetParent(row.transform, false);
            ((RectTransform)labelObj.transform).sizeDelta = new Vector2 (50, 16);

#if TMP_PRESENT
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label + ":";
            labelText.font = _calibriTMPFont;
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
#else
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = label + ":";
            labelText.font = _calibriFont;
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
#endif

            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 50;
            labelLe.preferredHeight = 16;

            // Slider
            GameObject sliderObj = new GameObject("Slider", typeof(RectTransform));
            sliderObj.transform.SetParent(row.transform, false);

            RectTransform sliderRt = (RectTransform)sliderObj.transform;
            sliderRt.sizeDelta = new Vector2(150, 20);  // Fixed width and height for better proportions

            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0;
            slider.maxValue = 1;

            LayoutElement sliderLe = sliderObj.AddComponent<LayoutElement>();
            sliderLe.preferredWidth = 150;
            sliderLe.preferredHeight = 20;

            // Background
            GameObject bg = new GameObject("Background", typeof(RectTransform));
            bg.transform.SetParent(sliderObj.transform, false);

            RectTransform bgRt = (RectTransform) bg.transform;
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.sizeDelta = Vector2.zero;

            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObj.transform, false);

            RectTransform fillAreaRt = (RectTransform)fillArea.transform;
            fillAreaRt.anchorMin = Vector2.zero;
            fillAreaRt.anchorMax = Vector2.one;
            fillAreaRt.sizeDelta = new Vector2(-5, -5);  // Smaller padding

            // Fill
            GameObject fill = new GameObject("Fill", typeof(RectTransform));
            fill.transform.SetParent(fillArea.transform, false);

            RectTransform fillRt = (RectTransform)fill.transform;
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.sizeDelta = Vector2.zero;

            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = sliderColor;

            slider.fillRect = fillRt;

            // Handle area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObj.transform, false);

            RectTransform handleAreaRt = (RectTransform) handleArea.transform;
            handleAreaRt.anchorMin = Vector2.zero;
            handleAreaRt.anchorMax = Vector2.one;
            handleAreaRt.sizeDelta = new Vector2(-5, 0);  // Padding on sides

            // Handle
            GameObject handle = new GameObject("Handle", typeof(RectTransform));
            handle.transform.SetParent(handleArea.transform, false);

            RectTransform handleRt = (RectTransform) handle.transform;
            handleRt.sizeDelta = new Vector2(15, 0);  // Width 15, height matches slider

            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;

            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;

            // Value input field (0-255)
            GameObject valueObj = new GameObject("Value", typeof (RectTransform));
            valueObj.transform.SetParent(row.transform, false);
            ((RectTransform)valueObj.transform).sizeDelta = new Vector2 (50, 25);

            Image valueImg = valueObj.AddComponent<Image>();
            valueImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            valueField = valueObj.AddComponent<InputField>();
            valueField.textComponent = CreateTextComponent(valueObj.transform, "0");
            valueField.text = "0";
            valueField.characterLimit = 3;
            valueField.contentType = InputField.ContentType.IntegerNumber;

            LayoutElement valueLe = valueObj.AddComponent<LayoutElement>();
            valueLe.preferredWidth = 50;
            valueLe.preferredHeight = 25;

            // Capture valueField in a local variable to avoid ref parameter issue
            var capturedValueField = valueField;

            // Add listeners
            slider.onValueChanged.AddListener((value) =>
            {
                capturedValueField.SetTextWithoutNotify(Mathf.RoundToInt(value * 255).ToString());
                OnColorSliderChanged();
            });

            valueField.onEndEdit.AddListener((text) =>
            {
                if (int.TryParse(text, out int intValue))
                {
                    intValue = Mathf.Clamp(intValue, 0, 255);
                    slider.SetValueWithoutNotify(intValue / 255f);
                    capturedValueField.SetTextWithoutNotify(intValue.ToString());
                    OnColorSliderChanged();
                }
            });

            return slider;
        }

#if TMP_PRESENT
        private TextMeshProUGUI CreateTextComponent(Transform parent, string initialText)
        {
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(parent, false);

            RectTransform textRt = (RectTransform)textObj.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 2);
            textRt.offsetMax = new Vector2(-5, -2);

            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = initialText;
            text.font = _calibriTMPFont;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Center;

            return text;
        }
#else
        private Text CreateTextComponent(Transform parent, string initialText)
        {
            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(parent, false);

            RectTransform textRt = (RectTransform)textObj.transform;
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = new Vector2(5, 2);
            textRt.offsetMax = new Vector2(-5, -2);

            Text text = textObj.AddComponent<Text>();
            text.text = initialText;
            text.font = _calibriFont;
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;

            return text;
        }
#endif

        private void CreateHexCodeField(Transform parent)
        {
            GameObject hexRow = new GameObject("HexRow", typeof (RectTransform));
            hexRow.transform.SetParent(parent, false);

            HorizontalLayoutGroup hlg = hexRow.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = false;
            hlg.padding.left   = 40;
            hlg.padding.right   = 11;


            ((RectTransform)hlg.transform).sizeDelta = new Vector2(570, 30);

            LayoutElement rowLe = hexRow.AddComponent<LayoutElement>();
            rowLe.minHeight = 30;
            rowLe.preferredWidth = 570;
            rowLe.preferredHeight = 30;

            // Label
            GameObject labelObj = new GameObject("Label", typeof(RectTransform));
            labelObj.transform.SetParent(hexRow.transform, false);

#if TMP_PRESENT
            TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = "Hexadecimal:";
            labelText.font = _calibriTMPFont;
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.MidlineLeft;
#else
            Text labelText = labelObj.AddComponent<Text>();
            labelText.text = "Hexadecimal:";
            labelText.font = _calibriFont;
            labelText.fontSize = 16;
            labelText.color = Color.white;
            labelText.alignment = TextAnchor.MiddleLeft;
#endif

            LayoutElement labelLe = labelObj.AddComponent<LayoutElement>();
            labelLe.preferredWidth = 200;

            // Input field
            GameObject inputObj = new GameObject("HexInput", typeof (RectTransform));
            inputObj.transform.SetParent(hexRow.transform, false);
            ((RectTransform)inputObj.transform).sizeDelta = new Vector2(150, 25);

            Image inputImg = inputObj.AddComponent<Image>();
            inputImg.color = new Color(0.1960784f, 0.1960784f, 0.1960784f, 1f);

            _hexCodeField = inputObj.AddComponent<InputField>();
            _hexCodeField.textComponent = CreateTextComponent(inputObj.transform, "#FFFFFF");
            _hexCodeField.text = "#FFFFFF";
            _hexCodeField.characterLimit = 9;  // #RRGGBBAA
            _hexCodeField.contentType = InputField.ContentType.Standard;

            LayoutElement inputLe = inputObj.AddComponent<LayoutElement>();
            inputLe.flexibleWidth = 1;
            inputLe.preferredWidth = 150;
            inputLe.preferredHeight = 25;

            // Add listener for hex code changes
            _hexCodeField.onEndEdit.AddListener((hexText) =>
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(hexText, out color))
                {
                    // Update sliders from hex
                    _redSlider.SetValueWithoutNotify(color.r);
                    _greenSlider.SetValueWithoutNotify(color.g);
                    _blueSlider.SetValueWithoutNotify(color.b);
                    _alphaSlider.SetValueWithoutNotify(color.a);

                    // Update value fields
                    _redValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.r * 255).ToString());
                    _greenValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.g * 255).ToString());
                    _blueValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.b * 255).ToString());
                    _alphaValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.a * 255).ToString());

                    // Update preview
                    _colorPickerPreview.color = color;
                }
                else
                {
                    // Invalid hex, revert to current color
                    _hexCodeField.text = ColorToHex(_colorPickerPreview.color);
                }
            });
        }

        private string ColorToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(color);
        }

        private void OnColorSliderChanged()
        {
            Color newColor = new Color(
                _redSlider.value,
                _greenSlider.value,
                _blueSlider.value,
                _alphaSlider.value
            );

            _colorPickerPreview.color = newColor;

            if (_hexCodeField != null)
            {
                _hexCodeField.SetTextWithoutNotify(ColorToHex(newColor));
            }
        }

        private void UpdateColorPickerFromColor(Color color)
        {
            _redSlider.SetValueWithoutNotify(color.r);
            _greenSlider.SetValueWithoutNotify(color.g);
            _blueSlider.SetValueWithoutNotify(color.b);
            _alphaSlider.SetValueWithoutNotify(color.a);

            _redValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.r * 255).ToString());
            _greenValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.g * 255).ToString());
            _blueValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.b * 255).ToString());
            _alphaValueField.SetTextWithoutNotify(Mathf.RoundToInt(color.a * 255).ToString());

            if (_hexCodeField != null)
            {
                _hexCodeField.SetTextWithoutNotify(ColorToHex(color));
            }

            _colorPickerPreview.color = color;
        }

        private void OnColorPickerOK()
        {
            Color selectedColor = _colorPickerPreview.color;
            _tempColors[_currentColorKey] = selectedColor;
            _colorPreviews[_currentColorKey].color = selectedColor;
            _colorPickerPanel.SetActive(false);
        }

        private void OnColorPickerCancel()
        {
            _colorPickerPanel.SetActive(false);
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // DATA HANDLING
        // ═════════════════════════════════════════════════════════════════════════════

        private void LoadCurrentValues()
        {
            _tempInts.Clear();
            _tempFloats.Clear();
            _tempStrings.Clear();
            _tempBools.Clear();
            _tempColors.Clear();
            _tempVector2s.Clear();
            _tempVector3s.Clear();
            _tempEnums.Clear();

            foreach (var kvp in _intInputs)
            {
                int value = GlobalSettingsManager.GetInt(kvp.Key, 0);
                kvp.Value.text = value.ToString();
                _tempInts[kvp.Key] = value;
            }

            foreach (var kvp in _floatInputs)
            {
                float value = GlobalSettingsManager.GetFloat(kvp.Key, 0f);
                kvp.Value.text = value.ToString();
                _tempFloats[kvp.Key] = value;
            }

            foreach (var kvp in _stringInputs)
            {
                string value = GlobalSettingsManager.GetString(kvp.Key, "");
                kvp.Value.text = value;
                _tempStrings[kvp.Key] = value;
            }

            foreach (var kvp in _boolToggles)
            {
                bool value = GlobalSettingsManager.GetBool(kvp.Key, false);
                kvp.Value.isOn = value;
                _tempBools[kvp.Key] = value;
            }

            foreach (var kvp in _colorPreviews)
            {
                Color value = GlobalSettingsManager.GetColor(kvp.Key, Color.white);
                kvp.Value.color = value;
                _tempColors[kvp.Key] = value;
            }

            foreach (var kvp in _vector2XInputs)
            {
                Vector2 value = GlobalSettingsManager.GetVector2(kvp.Key, Vector2.zero);
                kvp.Value.text = value.x.ToString();
                _vector2YInputs[kvp.Key].text = value.y.ToString();
                _tempVector2s[kvp.Key] = value;
            }

            foreach (var kvp in _vector3XInputs)
            {
                Vector3 value = GlobalSettingsManager.GetVector3(kvp.Key, Vector3.zero);
                kvp.Value.text = value.x.ToString();
                _vector3YInputs[kvp.Key].text = value.y.ToString();
                _vector3ZInputs[kvp.Key].text = value.z.ToString();
                _tempVector3s[kvp.Key] = value;
            }

            // Load enum values from settings
            foreach (var kvp in _enumDropdowns)
            {
                string key = kvp.Key;
                EnumSetting setting = GlobalSettingsManager.Settings?.GetEnumSetting(key);
                if (setting != null)
                {
                    Type enumType = Type.GetType(setting.enumTypeName);
                    if (enumType != null && enumType.IsEnum)
                    {
                        int currentIndex = Array.IndexOf(Enum.GetValues(enumType), setting.GetEnumValue());
                        if (currentIndex >= 0)
                        {
                            kvp.Value.value = currentIndex;
                            kvp.Value.RefreshShownValue();
                        }

                        _tempEnums[key] = new EnumSetting
                        {
                            key = setting.key,
                            enumTypeName = setting.enumTypeName,
                            intValue = setting.intValue
                        };
                    }
                }
            }
        }

        private void OnSave()
        {
            foreach (var kvp in _tempInts)
                GlobalSettingsManager.SetInt(kvp.Key, kvp.Value);

            foreach (var kvp in _tempFloats)
                GlobalSettingsManager.SetFloat(kvp.Key, kvp.Value);

            foreach (var kvp in _tempStrings)
                GlobalSettingsManager.SetString(kvp.Key, kvp.Value);

            foreach (var kvp in _tempBools)
                GlobalSettingsManager.SetBool(kvp.Key, kvp.Value);

            foreach (var kvp in _tempColors)
                GlobalSettingsManager.SetColor(kvp.Key, kvp.Value);

            foreach (var kvp in _tempVector2s)
                GlobalSettingsManager.SetVector2(kvp.Key, kvp.Value);

            foreach (var kvp in _tempVector3s)
                GlobalSettingsManager.SetVector3(kvp.Key, kvp.Value);

            // Save enum values using the non-generic SetEnumSetting method
            if (GlobalSettingsManager.Settings != null)
            {
                foreach (var kvp in _tempEnums)
                    GlobalSettingsManager.Settings.SetEnumSetting(kvp.Value);
            }

            GlobalSettingsManager.OnAdminPanelSaved?.Invoke();

            Debug.Log("[GlobalSettingsRuntimePanel] Settings saved!");
            Hide();
        }

        private void OnCancel()
        {
            Debug.Log("[GlobalSettingsRuntimePanel] Settings cancelled!");
            Hide();
        }
    }
}
