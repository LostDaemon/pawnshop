# AssetBundle System Usage Guide

## Overview

This system allows you to create and load AssetBundles containing ItemPrototypes and their corresponding sprites for DLC/add-on distribution.

## Architecture

- **ItemBundleBuilder** - Editor tool for creating AssetBundles
- **AssetBundleLoader** - Runtime component for loading bundles and distributing content
- **AssetBundleService** - Service for loading AssetBundle files
- **SpriteService** - Service for managing loaded sprites
- **ItemRepository** - Repository for storing item prototypes

## Creating AssetBundles

### 1. Using ItemBundleBuilder Editor Tool

1. Open Unity Editor
2. Go to `Tools → Item Bundle Builder`
3. Select the folder containing your ItemPrototypes and sprites
4. Click "Build All Item Bundles"
5. The tool will create a single bundle from all assets in the folder

### 2. Bundle Structure

- **One folder = One bundle**
- Bundle name is automatically generated: `bundle_[folder_name]`
- All `.asset` (ItemPrototype) and `.png` (Sprite) files in the folder are included

## Loading AssetBundles in Game

### 1. Using AssetBundleLoader Component

1. Add `AssetBundleLoader` component to a GameObject
2. Assign `ItemRepository` and `SpriteService` references in the inspector
3. Configure bundle paths in the inspector
4. The component will automatically load bundles on Start
5. Or call methods manually:
   ```csharp
   var loader = GetComponent<AssetBundleLoader>();
   loader.LoadAssetBundle("Path/To/Bundle.unity3d");
   ```

### 2. Manual Loading

```csharp
// Create AssetBundleLoader instance
var loader = new AssetBundleLoader();

// Load single bundle
loader.LoadAssetBundle("Path/To/Your/Bundle.unity3d");

// Load multiple bundles
string[] bundlePaths = {
    "Path/To/Bundle1.unity3d",
    "Path/To/Bundle2.unity3d"
};
loader.LoadAssetBundles(bundlePaths);
```

## How It Works

### 1. AssetBundleLoader Responsibilities

- Loads AssetBundle files using AssetBundleService
- Extracts ItemPrototypes and Sprites from bundles
- Distributes ItemPrototypes to ItemRepository
- Registers Sprites in SpriteService

### 2. Repository Independence

- ItemRepository and SpriteService don't know about AssetBundles
- They only store and provide access to their respective data
- AssetBundleLoader acts as a bridge between bundles and repositories

## Accessing Loaded Content

### 1. Get Items

```csharp
// Get random item
var randomItem = itemRepository.GetRandomItem();

// Get specific item by ClassId
var specificItem = itemRepository.GetItem("item_brilliant");
```

### 2. Get Sprites

```csharp
// Get sprite from sprite service
var sprite = spriteService.GetSprite("item_brilliant");
```

## File Paths

### Build Output

- AssetBundles are built to: `Assets/AssetBundles/Items/[BuildTarget]/`
- Example: `Assets/AssetBundles/Items/StandaloneWindows/bundle_JewelryPack`

### Runtime Loading

- Use absolute paths or relative to StreamingAssets
- Example: `Application.streamingAssetsPath + "/Bundles/bundle_JewelryPack"`

## Memory Management

### 1. Automatic Cleanup

- AssetBundles are automatically unloaded when AssetBundleLoader is destroyed
- Use `AssetBundleService.UnloadAllBundles()` to manually free memory

### 2. Best Practices

- Load bundles when needed, not all at once
- Unload bundles when no longer needed
- Monitor memory usage in profiler

## Error Handling

### Common Issues

1. **Bundle not found**: Check file path and ensure bundle exists
2. **Assets not loading**: Verify bundle contains correct asset types
3. **Memory issues**: Ensure bundles are properly unloaded

### Debug Logs

The system provides detailed logging:

- AssetBundle loading/unloading
- Item and sprite registration
- Error messages with context

## Example Workflow

1. **Create Bundle**:

   - Organize ItemPrototypes and sprites in a folder
   - Use ItemBundleBuilder to create bundle
   - Bundle is saved to `Assets/AssetBundles/Items/`

2. **Distribute Bundle**:

   - Copy bundle file to your DLC distribution system
   - Bundle contains all necessary assets

3. **Load in Game**:

   - Place bundle in StreamingAssets or download path
   - Use AssetBundleLoader to load and distribute content
   - Items and sprites are automatically registered in repositories

4. **Use Content**:
   - Access items through ItemRepository
   - Access sprites through SpriteService
   - All loaded content is available for gameplay

## Tips

- **Bundle Size**: Keep bundles reasonably sized for faster loading
- **Asset Names**: Ensure ImageId in ItemPrototype matches sprite names
- **Testing**: Test bundle loading in builds, not just editor
- **Versioning**: Consider bundle versioning for updates
- **Separation of Concerns**: AssetBundleLoader handles bundle logic, repositories handle data storage
