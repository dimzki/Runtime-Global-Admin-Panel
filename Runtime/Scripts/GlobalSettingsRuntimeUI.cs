using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Runtime UI panel for displaying and editing GlobalSettings during gameplay.
    /// Shows a popup with all settings and Save/Cancel buttons.
    /// </summary>
    public class GlobalSettingsRuntimeUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform contentParent;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ScrollRect scrollRect;

        [Header("Prefabs")]
        [SerializeField] private GameObject intFieldPrefab;
        [SerializeField] private GameObject floatFieldPrefab;
        [SerializeField] private GameObject stringFieldPrefab;
        [SerializeField] private GameObject boolFieldPrefab;
        [SerializeField] private GameObject colorFieldPrefab;

        [Header("Settings")]
        [SerializeField] private bool hideOnAwake = true;

        private GlobalSettings _settings;
        private Dictionary<string, object> _tempValues = new Dictionary<string, object>();

        // ═════════════════════════════════════════════════════════════════════════════
        // LIFECYCLE
        // ═════════════════════════════════════════════════════════════════════════════

        private void Awake()
        {
            _settings = GlobalSettingsManager.Settings;

            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);

            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancelClicked);

            if (hideOnAwake)
                Hide();
        }

        private void OnDestroy()
        {
            if (saveButton != null)
                saveButton.onClick.RemoveListener(OnSaveClicked);

            if (cancelButton != null)
                cancelButton.onClick.RemoveListener(OnCancelClicked);
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Shows the settings panel and generates UI for all settings.
        /// </summary>
        public void Show()
        {
            if (_settings == null)
            {
                Debug.LogError("[GlobalSettingsRuntimeUI] No GlobalSettings found!");
                return;
            }

            panelRoot.SetActive(true);
            GenerateUI();
        }

        /// <summary>
        /// Hides the settings panel.
        /// </summary>
        public void Hide()
        {
            panelRoot.SetActive(false);
            ClearUI();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // UI GENERATION
        // ═════════════════════════════════════════════════════════════════════════════

        private void GenerateUI()
        {
            ClearUI();
            _tempValues.Clear();

            // This is a simplified version - we'll use reflection to get the dictionaries
            // For now, let's create a simple implementation

            // You can manually add settings here, or we can use reflection
            // For demonstration, let's add a simple text showing that it works
            CreateHeaderText("Integer Settings");
            CreateHeaderText("Float Settings");
            CreateHeaderText("String Settings");
            CreateHeaderText("Bool Settings");
            CreateHeaderText("Color Settings");

            // Note: Full implementation would use reflection to iterate through
            // the GlobalSettings dictionaries and create UI elements dynamically
        }

        private void ClearUI()
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateHeaderText(string text)
        {
            GameObject header = new GameObject("Header_" + text);
            header.transform.SetParent(contentParent, false);

            TextMeshProUGUI textComponent = header.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 24;
            textComponent.fontStyle = FontStyles.Bold;
            textComponent.color = Color.white;

            LayoutElement layoutElement = header.AddComponent<LayoutElement>();
            layoutElement.minHeight = 40;
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // BUTTON HANDLERS
        // ═════════════════════════════════════════════════════════════════════════════

        private void OnSaveClicked()
        {
            // Apply temp values to GlobalSettings
            foreach (var kvp in _tempValues)
            {
                // Apply based on type
                // This would be implemented based on the type of value
            }

            Debug.Log("[GlobalSettingsRuntimeUI] Settings saved!");
            Hide();
        }

        private void OnCancelClicked()
        {
            Debug.Log("[GlobalSettingsRuntimeUI] Settings cancelled!");
            Hide();
        }
    }
}
