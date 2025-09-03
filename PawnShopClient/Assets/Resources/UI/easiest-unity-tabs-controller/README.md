# TabsController for Unity

Simple and efficient tabs controller for Unity UI with button-based navigation and color customization.

## Description

TabsController is a MonoBehaviour component that allows you to easily create and manage a tab system in Unity. When clicking on a tab button, all other tabs automatically hide, and the selected one becomes visible. The controller also provides automatic color management for better visual feedback.

## Features

- ✅ Simple setup through Unity Inspector
- ✅ Automatic tab switching
- ✅ Programmatic tab control
- ✅ Safety (null checks)
- ✅ Automatic memory cleanup
- ✅ First tab activates by default
- ✅ **Color customization for each tab**
- ✅ **Automatic highlight color for active tab**
- ✅ **Smart text color contrast calculation**

## Installation

1. Install Package using Package Manager
2. Add the `EasiestTabbedPanel` prefab to your scene
3. Add and configure Additional Tabs

## Setup

### 1. Adding the Component

- Select a GameObject in the hierarchy
- Click "Add Component" in the inspector
- Find and add "TabsController"

### 2. Configuring Tabs

In the component inspector, configure the `Tabs` list:

- **Button** - button for switching tabs
- **Tab Content** - GameObject with tab content
- **Tab Color** - individual color for each tab button

### 3. Color Configuration

The TabsController includes a color system:

- **Highlight Color** - color for the active tab button
- **Individual Tab Colors** - custom color for each tab button
- **Automatic Text Contrast** - text color automatically adjusts for readability

### 4. UI Structure

Recommended structure:

```
Canvas
├── TabButtons (Panel)
│   ├── Tab1Button (Button)
│   ├── Tab2Button (Button)
│   └── Tab3Button (Button)
└── TabContents (Panel)
    ├── Tab1Content (GameObject)
    ├── Tab2Content (GameObject)
    └── Tab3Content (GameObject)
```

## Usage

### Automatic Switching

After setup, tabs will switch automatically when clicking buttons.

### Color Behavior

- **Active Tab**: Uses the highlight color
- **Inactive Tabs**: Use their individual tab colors
- **Text Colors**: Automatically calculated for optimal contrast

### Programmatic Control

```csharp
// Get reference to controller
TabsController tabsController = GetComponent<TabsController>();

// Activate tab by index
tabsController.ActivateTab(1);

// Get active tab index
int activeTabIndex = tabsController.GetActiveTabIndex();
```

## API

### Public Methods

#### `ActivateTab(int tabIndex)`

Activates the tab with the specified index.

**Parameters:**

- `tabIndex` - index of tab to activate (starting from 0)

#### `GetActiveTabIndex()`

Returns the index of the currently active tab.

**Returns:**

- Index of active tab or -1 if none active

### Private Methods

#### `InitializeTabs()`

Initializes tabs and subscribes to button events.

#### `OnTabButtonClicked(int tabIndex)`

Handles tab button click event.

#### `UpdateButtonColors(int activeTabIndex)`

Updates button colors to highlight the active tab.

#### `UpdateButtonTextColor(Button button, Color backgroundColor)`

Updates button text color for optimal contrast.

## Color System Details

### TabData Structure

```csharp
[System.Serializable]
public class TabData
{
    public Button button;
    public GameObject tabContent;
    public Color tabColor; // Individual tab color
}
```

### Color Configuration

- **Highlight Color**: Set in inspector, used for active tab
- **Tab Colors**: Individual colors for each tab button
- **Text Contrast**: Automatically calculated using luminance formula

### Text Contrast Algorithm

The controller uses the luminance formula to determine optimal text color:

- If background is dark (luminance < 0.5): White text
- If background is light (luminance >= 0.5): Black text

## Requirements

- Unity 2019.4 LTS or newer
- UnityEngine.UI
- TextMeshPro (for text components)

## Usage Example

1. Create UI Canvas
2. Add panels for tab buttons and content
3. Create buttons for each tab
4. Create GameObjects with content for each tab
5. Add TabsController to Canvas
6. Configure tab list in inspector
7. Set individual tab colors
8. Adjust highlight color if needed
9. Run scene - first tab will be active by default

## Troubleshooting

### Tabs Not Switching

- Check that buttons have Button component
- Ensure Tab Content is not null
- Check console for errors

### First Tab Not Active

- Ensure first element in tab list is configured correctly
- Check that first tab's Tab Content exists

### Color Issues

- Verify TextMeshPro components are attached to buttons
- Check that tab colors are properly set in inspector
- Ensure highlight color is visible against your UI theme

### Console Errors

- Check inspector settings
- Ensure all fields are filled

## License

This component is distributed free for use in any Unity projects.

## Support

For issues or improvement suggestions, create an issue in the project repository.

---

**Version:** 1.0.1  
**Date:** 08.2025  
**Author:** Anatolii Nikolaev aka LostDaemon
