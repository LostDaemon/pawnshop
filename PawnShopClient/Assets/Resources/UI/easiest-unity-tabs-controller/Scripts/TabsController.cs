using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LostDaemon.TabsController
{
    /// <summary>
    /// Controller for managing tabs with button controls and corresponding game objects
    /// </summary>
    public class TabsController : MonoBehaviour
    {
        [System.Serializable]
        public class TabData
        {
            public Button button;
            public GameObject tabContent;
            public Color tabColor; // Added for individual tab colors
        }

        [Header("Tabs Configuration")]
        [SerializeField] private List<TabData> tabs = new List<TabData>();
        [SerializeField] private Color _highlightColor = Color.yellow; // Added for highlight color

        private void Start()
        {
            Debug.LogWarning("Start!");
            InitializeTabs();
        }

        /// <summary>
        /// Initialize tabs and subscribe to button events
        /// </summary>
        private void InitializeTabs()
        {
            Debug.Log("InitializeTabs");
            if (tabs == null || tabs.Count == 0)
            {
                Debug.LogWarning("TabsController: No tabs configured!");
                return;
            }

            // Subscribe to button events
            for (int i = 0; i < tabs.Count; i++)
            {
                int tabIndex = i; // Capture index for lambda
                TabData tab = tabs[i];

                if (tab.button != null)
                {
                    tab.button.onClick.AddListener(() => OnTabButtonClicked(tabIndex));
                }
                else
                {
                    Debug.LogError($"TabsController: Button at index {i} is null!");
                }
            }

            // Activate first tab by default
            if (tabs.Count > 0 && tabs[0].button != null)
            {
                OnTabButtonClicked(0);
            }
        }

        /// <summary>
        /// Handle tab button click event
        /// </summary>
        /// <param name="tabIndex">Index of the activated tab</param>
        private void OnTabButtonClicked(int tabIndex)
        {
            // Deactivate all tabs
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabContent != null)
                {
                    tabs[i].tabContent.SetActive(false);
                }
            }

            // Activate the selected tab
            if (tabIndex >= 0 && tabIndex < tabs.Count && tabs[tabIndex].tabContent != null)
            {
                tabs[tabIndex].tabContent.SetActive(true);
            }

            UpdateButtonColors(tabIndex);
        }

        /// <summary>
        /// Update button colors to highlight the active tab button
        /// </summary>
        /// <param name="activeTabIndex">Index of the active tab</param>
        private void UpdateButtonColors(int activeTabIndex)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].button != null)
                {
                    var colors = tabs[i].button.colors;
                    Color buttonColor;

                    if (i == activeTabIndex)
                    {
                        // Use highlight color for active tab
                        buttonColor = _highlightColor;
                        colors.normalColor = _highlightColor;
                        colors.selectedColor = _highlightColor;
                    }
                    else
                    {
                        // Use individual tab color for inactive tabs
                        buttonColor = tabs[i].tabColor;
                        colors.normalColor = tabs[i].tabColor;
                        colors.selectedColor = tabs[i].tabColor;
                    }

                    tabs[i].button.colors = colors;

                    // Update text color to be contrast with button color
                    UpdateButtonTextColor(tabs[i].button, buttonColor);
                }
            }
        }

        /// <summary>
        /// Update button text color to be contrast with button background color
        /// </summary>
        /// <param name="button">Button to update text color for</param>
        /// <param name="backgroundColor">Background color of the button</param>
        private void UpdateButtonTextColor(Button button, Color backgroundColor)
        {
            // Get text component from button
            var textComponent = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();

            if (textComponent != null)
            {
                // Calculate luminance of background color
                float luminance = 0.299f * backgroundColor.r + 0.587f * backgroundColor.g + 0.114f * backgroundColor.b;

                // If background is dark, use white text; if light, use black text
                Color textColor = luminance > 0.5f ? Color.black : Color.white;
                textComponent.color = textColor;
            }
        }

        /// <summary>
        /// Programmatically activate a specific tab
        /// </summary>
        /// <param name="tabIndex">Index of the tab to activate</param>
        public void ActivateTab(int tabIndex)
        {
            if (tabIndex >= 0 && tabIndex < tabs.Count)
            {
                OnTabButtonClicked(tabIndex);
            }
        }

        /// <summary>
        /// Get the currently active tab index
        /// </summary>
        /// <returns>Index of the active tab, or -1 if none active</returns>
        public int GetActiveTabIndex()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabContent != null && tabs[i].tabContent.activeSelf)
                {
                    return i;
                }
            }
            return -1;
        }

        private void OnDestroy()
        {
            // Unsubscribe from button events to prevent memory leaks
            if (tabs != null)
            {
                foreach (var tab in tabs)
                {
                    if (tab.button != null)
                    {
                        tab.button.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}
