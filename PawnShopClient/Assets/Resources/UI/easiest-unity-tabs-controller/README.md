# TabsController for Unity

Simple and efficient tabs controller for Unity UI with button-based navigation.

## Description

TabsController is a MonoBehaviour component that allows you to easily create and manage a tab system in Unity. When clicking on a tab button, all other tabs automatically hide, and the selected one becomes visible.

## Features

- ✅ Simple setup through Unity Inspector
- ✅ Automatic tab switching
- ✅ Programmatic tab control
- ✅ Safety (null checks)
- ✅ Automatic memory cleanup
- ✅ First tab activates by default

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

### 3. UI Structure

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

## Requirements

- Unity 2019.4 LTS or newer
- UI Toolkit (built into Unity)

## Usage Example

1. Create UI Canvas
2. Add panels for tab buttons and content
3. Create buttons for each tab
4. Create GameObjects with content for each tab
5. Add TabsController to Canvas
6. Configure tab list in inspector
7. Run scene - first tab will be active by default

## Troubleshooting

### Tabs Not Switching

- Check that buttons have Button component
- Ensure Tab Content is not null
- Check console for errors

### First Tab Not Active

- Ensure first element in tab list is configured correctly
- Check that first tab's Tab Content exists

### Console Errors

- Check inspector settings
- Ensure all fields are filled

## License

This component is distributed free for use in any Unity projects.

## Support

For issues or improvement suggestions, create an issue in the project repository.

---

**Version:** 1.0.0  
**Date:** 08.2025  
**Author:** Anatolii Nikolaev aka LostDaemon
