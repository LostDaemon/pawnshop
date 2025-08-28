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
        }

        [Header("Tabs Configuration")]
        [SerializeField] private List<TabData> tabs = new List<TabData>();
        
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
                    if (i == activeTabIndex)
                    {
                        colors.normalColor = colors.selectedColor;
                    }
                    else
                    {
                        colors.normalColor = Color.white;
                    }
                    tabs[i].button.colors = colors;
                }
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
