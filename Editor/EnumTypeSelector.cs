using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Alzaki.GlobalSettings
{
    /// <summary>
    /// Popup window for selecting enum types with search functionality.
    /// Similar to Odin Inspector's type selector.
    /// </summary>
    public class EnumTypeSelector : EditorWindow
    {
        private static List<Type> _cachedEnumTypes;
        private static string[] _cachedDisplayNames;
        private static Dictionary<string, Type> _displayNameToType;

        private string _searchText = "";
        private Vector2 _scrollPosition;
        private List<int> _filteredIndices = new List<int>();
        private Action<Type> _onTypeSelected;
        private int _selectedIndex = -1;
        private bool _focusSearchField = true;

        private const float WINDOW_WIDTH = 350f;
        private const float WINDOW_HEIGHT = 400f;

        // ═════════════════════════════════════════════════════════════════════════════
        // PUBLIC API
        // ═════════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Shows the enum type selector popup.
        /// </summary>
        /// <param name="buttonRect">The rect of the button that triggered this popup (in screen space)</param>
        /// <param name="onTypeSelected">Callback when a type is selected</param>
        public static void Show(Rect buttonRect, Action<Type> onTypeSelected)
        {
            EnsureEnumTypesCached();

            var window = CreateInstance<EnumTypeSelector>();
            window._onTypeSelected = onTypeSelected;
            window._filteredIndices = Enumerable.Range(0, _cachedEnumTypes.Count).ToList();
            window.titleContent = new GUIContent("Select Enum Type");

            // Calculate position below the button
            float x = buttonRect.x;
            float y = buttonRect.yMax + 2;

            // Make sure window stays on screen
            if (x + WINDOW_WIDTH > Screen.currentResolution.width)
                x = Screen.currentResolution.width - WINDOW_WIDTH - 10;
            if (y + WINDOW_HEIGHT > Screen.currentResolution.height - 50)
                y = buttonRect.y - WINDOW_HEIGHT - 2;

            // Use ShowUtility for a popup-style window
            window.ShowUtility();
            window.position = new Rect(x, y, WINDOW_WIDTH, WINDOW_HEIGHT);
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.Focus();
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // CACHING
        // ═════════════════════════════════════════════════════════════════════════════

        private static void EnsureEnumTypesCached()
        {
            if (_cachedEnumTypes != null) return;

            _cachedEnumTypes = new List<Type>();
            _displayNameToType = new Dictionary<string, Type>();

            // Get all enum types from all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Skip system assemblies for performance
                    string assemblyName = assembly.GetName().Name;
                    if (assemblyName.StartsWith("System") ||
                        assemblyName.StartsWith("Microsoft") ||
                        assemblyName.StartsWith("mscorlib") ||
                        assemblyName.StartsWith("netstandard") ||
                        assemblyName.StartsWith("Mono."))
                        continue;

                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.IsEnum && !type.IsGenericType && type.IsPublic)
                        {
                            _cachedEnumTypes.Add(type);
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that can't be reflected
                }
            }

            // Sort by name for easier browsing
            _cachedEnumTypes = _cachedEnumTypes.OrderBy(t => t.Name).ToList();

            // Build display names
            _cachedDisplayNames = new string[_cachedEnumTypes.Count];
            for (int i = 0; i < _cachedEnumTypes.Count; i++)
            {
                var type = _cachedEnumTypes[i];
                string displayName = $"{type.Name}";
                string namespacePart = string.IsNullOrEmpty(type.Namespace) ? "(global)" : type.Namespace;
                _cachedDisplayNames[i] = $"{displayName}|{namespacePart}";
                _displayNameToType[displayName] = type;
            }
        }

        /// <summary>
        /// Clears the cached enum types. Call this if you need to refresh the list.
        /// </summary>
        public static void ClearCache()
        {
            _cachedEnumTypes = null;
            _cachedDisplayNames = null;
            _displayNameToType = null;
        }

        // ═════════════════════════════════════════════════════════════════════════════
        // GUI
        // ═════════════════════════════════════════════════════════════════════════════

        private void OnGUI()
        {
            // Handle keyboard input
            HandleKeyboardInput();

            // Search field
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                GUI.SetNextControlName("SearchField");
                string newSearch = EditorGUILayout.TextField(_searchText, EditorStyles.toolbarSearchField);

                if (newSearch != _searchText)
                {
                    _searchText = newSearch;
                    FilterResults();
                    _selectedIndex = _filteredIndices.Count > 0 ? 0 : -1;
                }

                if (GUILayout.Button("", GUI.skin.FindStyle("ToolbarSearchCancelButton") ?? EditorStyles.miniButton, GUILayout.Width(18)))
                {
                    _searchText = "";
                    FilterResults();
                    GUI.FocusControl("SearchField");
                }
            }
            EditorGUILayout.EndHorizontal();

            // Focus search field on first frame
            if (_focusSearchField)
            {
                EditorGUI.FocusTextInControl("SearchField");
                _focusSearchField = false;
            }

            // "None" option
            EditorGUILayout.BeginVertical();
            {
                if (GUILayout.Button("None", EditorStyles.miniButton))
                {
                    _onTypeSelected?.Invoke(null);
                    Close();
                }
            }
            EditorGUILayout.EndVertical();

            // Separator
            EditorGUILayout.Space(2);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            EditorGUILayout.Space(2);

            // Results count
            EditorGUILayout.LabelField($"Found {_filteredIndices.Count} enum types", EditorStyles.centeredGreyMiniLabel);

            // Scrollable list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            {
                for (int i = 0; i < _filteredIndices.Count; i++)
                {
                    int typeIndex = _filteredIndices[i];
                    var type = _cachedEnumTypes[typeIndex];
                    string[] parts = _cachedDisplayNames[typeIndex].Split('|');
                    string typeName = parts[0];
                    string namespaceName = parts.Length > 1 ? parts[1] : "";

                    bool isSelected = (i == _selectedIndex);

                    // Draw item
                    Rect itemRect = EditorGUILayout.BeginHorizontal(isSelected ? "SelectionRect" : GUIStyle.none);
                    {
                        // Highlight on hover
                        if (itemRect.Contains(Event.current.mousePosition) && !isSelected)
                        {
                            EditorGUI.DrawRect(itemRect, new Color(0.3f, 0.3f, 0.3f, 0.3f));
                        }

                        EditorGUILayout.BeginVertical();
                        {
                            // Type name (bold)
                            GUILayout.Label(typeName, EditorStyles.boldLabel);

                            // Namespace (smaller, gray)
                            var namespaceStyle = new GUIStyle(EditorStyles.miniLabel)
                            {
                                normal = { textColor = Color.gray }
                            };
                            GUILayout.Label(namespaceName, namespaceStyle);
                        }
                        EditorGUILayout.EndVertical();

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    // Handle click
                    if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                    {
                        _onTypeSelected?.Invoke(type);
                        Close();
                        Event.current.Use();
                    }

                    // Small spacing between items
                    GUILayout.Space(2);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void FilterResults()
        {
            _filteredIndices.Clear();

            if (string.IsNullOrEmpty(_searchText))
            {
                _filteredIndices = Enumerable.Range(0, _cachedEnumTypes.Count).ToList();
                return;
            }

            string search = _searchText.ToLowerInvariant();

            for (int i = 0; i < _cachedEnumTypes.Count; i++)
            {
                var type = _cachedEnumTypes[i];
                string fullName = (type.Namespace + "." + type.Name).ToLowerInvariant();
                string typeName = type.Name.ToLowerInvariant();

                if (typeName.Contains(search) || fullName.Contains(search))
                {
                    _filteredIndices.Add(i);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown) return;

            switch (e.keyCode)
            {
                case KeyCode.DownArrow:
                    _selectedIndex = Mathf.Min(_selectedIndex + 1, _filteredIndices.Count - 1);
                    e.Use();
                    Repaint();
                    break;

                case KeyCode.UpArrow:
                    _selectedIndex = Mathf.Max(_selectedIndex - 1, 0);
                    e.Use();
                    Repaint();
                    break;

                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (_selectedIndex >= 0 && _selectedIndex < _filteredIndices.Count)
                    {
                        int typeIndex = _filteredIndices[_selectedIndex];
                        _onTypeSelected?.Invoke(_cachedEnumTypes[typeIndex]);
                        Close();
                    }
                    e.Use();
                    break;

                case KeyCode.Escape:
                    Close();
                    e.Use();
                    break;
            }
        }

        private void OnLostFocus()
        {
            // Close when clicking outside
            Close();
        }
    }
}
