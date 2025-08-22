using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using PawnShop.Repositories;
using PawnShop.Services;
using PawnShop.ScriptableObjects;

namespace PawnShop.Infrastructure
{
    /// <summary>
    /// Loads AssetBundles and distributes loaded items and sprites to repositories
    /// </summary>
    public class AssetBundleLoader
    {
        private readonly IItemRepository _itemRepository;
        private readonly ISpriteService _spriteService;
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        
        public AssetBundleLoader(IItemRepository itemRepository, ISpriteService spriteService)
        {
            _itemRepository = itemRepository;
            _spriteService = spriteService;
        }
        
        /// <summary>
        /// Loads multiple AssetBundles from file paths
        /// </summary>
        /// <param name="bundlePaths">Array of AssetBundle file paths</param>
        public void LoadAssetBundles(string[] bundlePaths)
        {
            if (bundlePaths == null || bundlePaths.Length == 0)
            {
                Debug.LogWarning("[AssetBundleLoader] No bundle paths provided");
                return;
            }
            
            Debug.Log($"[AssetBundleLoader] Starting to load {bundlePaths.Length} AssetBundles...");
            
            foreach (var bundlePath in bundlePaths)
            {
                LoadAssetBundle(bundlePath);
            }
            
            Debug.Log("[AssetBundleLoader] Finished loading AssetBundles");
        }
        
        /// <summary>
        /// Loads a specific AssetBundle by file path
        /// </summary>
        /// <param name="bundlePath">Full path to the AssetBundle file</param>
        public void LoadAssetBundle(string bundlePath)
        {
            if (string.IsNullOrEmpty(bundlePath))
            {
                Debug.LogError("[AssetBundleLoader] Bundle path is null or empty");
                return;
            }
            
            Debug.Log($"[AssetBundleLoader] Loading AssetBundle: {bundlePath}");
            
            try
            {
                // Load item prototypes from bundle
                var prototypes = LoadItemPrototypesFromBundle(bundlePath);
                if (prototypes.Length == 0)
                {
                    Debug.LogWarning($"[AssetBundleLoader] No ItemPrototypes found in bundle: {bundlePath}");
                }
                else
                {
                    // Add prototypes to item repository
                    foreach (var prototype in prototypes)
                    {
                        _itemRepository.AddItem(prototype);
                    }
                    Debug.Log($"[AssetBundleLoader] Added {prototypes.Length} ItemPrototypes to repository");
                }
                
                // Load sprites from bundle
                var sprites = LoadSpritesFromBundle(bundlePath);
                if (sprites.Length == 0)
                {
                    Debug.LogWarning($"[AssetBundleLoader] No Sprites found in bundle: {bundlePath}");
                }
                else
                {
                    // Register sprites in sprite service
                    _spriteService.RegisterSprites(sprites);
                    Debug.Log($"[AssetBundleLoader] Registered {sprites.Length} sprites");
                }
                
                Debug.Log($"[AssetBundleLoader] Successfully processed bundle: {bundlePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AssetBundleLoader] Error processing bundle {bundlePath}: {e.Message}");
            }
        }
        

        
        /// <summary>
        /// Loads all AssetBundles from appropriate folder (editor or build)
        /// </summary>
        public void LoadAllBundles()
        {
            string folderPath;
            
            if (Application.isEditor)
            {
                // In editor: use StreamingAssets
                folderPath = Application.streamingAssetsPath;
                Debug.Log($"[AssetBundleLoader] Editor mode - loading from: {folderPath}");
            }
            else
            {
                // In build: try multiple locations
                folderPath = GetBundlePathInBuild();
                Debug.Log($"[AssetBundleLoader] Build mode - loading from: {folderPath}");
            }
            
            LoadAllBundlesFromPath(folderPath);
        }
        
        /// <summary>
        /// Gets the appropriate bundle path in build
        /// </summary>
        private string GetBundlePathInBuild()
        {
            // In build, Application.dataPath points to the _Data folder
            // We need to go up one level to get to the exe directory
            var dataPath = Application.dataPath;
            var exePath = Directory.GetParent(dataPath)?.FullName ?? dataPath;
            
            // First try StreamingAssets (if Unity copied them)
            if (Directory.Exists(Application.streamingAssetsPath))
            {
                var streamingBundles = Directory.GetFiles(Application.streamingAssetsPath, "*.bundle", SearchOption.AllDirectories);
                if (streamingBundles.Length > 0)
                {
                    Debug.Log($"[AssetBundleLoader] Found {streamingBundles.Length} bundles in StreamingAssets: {Application.streamingAssetsPath}");
                    return Application.streamingAssetsPath;
                }
            }
            
            // Then search in exe directory and all subdirectories for any bundles
            var allBundles = Directory.GetFiles(exePath, "*.bundle", SearchOption.AllDirectories);
            if (allBundles.Length > 0)
            {
                // Find the directory with most bundles (likely the main addon folder)
                var bundleGroups = allBundles
                    .GroupBy(file => Path.GetDirectoryName(file))
                    .OrderByDescending(group => group.Count())
                    .First();
                
                var bestPath = bundleGroups.Key;
                Debug.Log($"[AssetBundleLoader] Found {bundleGroups.Count()} bundles in: {bestPath}");
                return bestPath;
            }
            
            Debug.LogWarning($"[AssetBundleLoader] No bundles found anywhere, using exe directory: {exePath}");
            return exePath;
        }
        
        /// <summary>
        /// Loads all AssetBundles from a folder and all subfolders
        /// </summary>
        /// <param name="folderPath">Path to folder containing AssetBundles</param>
        public void LoadAllBundlesFromPath(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[AssetBundleLoader] Bundle folder not found: {folderPath}");
                return;
            }
            
            LoadAllBundlesRecursive(folderPath);
        }
        
        /// <summary>
        /// Recursively loads all AssetBundles from a folder and its subfolders
        /// </summary>
        /// <param name="folderPath">Path to search for AssetBundles</param>
        private void LoadAllBundlesRecursive(string folderPath)
        {
            // Load bundles from current folder (with .bundle extension)
            var bundleFiles = Directory.GetFiles(folderPath, "*.bundle");
            if (bundleFiles.Length > 0)
            {
                Debug.Log($"[AssetBundleLoader] Found {bundleFiles.Length} bundles in {folderPath}");
                
                foreach (var bundleFile in bundleFiles)
                {
                    LoadAssetBundle(bundleFile); // Pass full path
                }
            }
            
            // Recursively search subfolders
            var subfolders = Directory.GetDirectories(folderPath);
            foreach (var subfolder in subfolders)
            {
                LoadAllBundlesRecursive(subfolder);
            }
        }
        
        /// <summary>
        /// Loads all ItemPrototypes from AssetBundle
        /// </summary>
        private ItemPrototype[] LoadItemPrototypesFromBundle(string bundlePath)
        {
            var assetBundle = LoadAssetBundleInternal(bundlePath);
            if (assetBundle == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load AssetBundle from: {bundlePath}");
                return new ItemPrototype[0];
            }
            
            try
            {
                var assetNames = assetBundle.GetAllAssetNames();
                Debug.Log($"[AssetBundleLoader] Found {assetNames.Length} assets in bundle: {string.Join(", ", assetNames)}");
                
                var prototypes = new List<ItemPrototype>();
                
                foreach (var assetName in assetNames)
                {
                    var asset = assetBundle.LoadAsset(assetName);
                    if (asset is ItemPrototype prototype)
                    {
                        prototypes.Add(prototype);
                        Debug.Log($"[AssetBundleLoader] Loaded ItemPrototype: {prototype.Name} from bundle");
                    }
                }
                
                Debug.Log($"[AssetBundleLoader] Successfully loaded {prototypes.Count} ItemPrototypes from bundle");
                return prototypes.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AssetBundleLoader] Error loading ItemPrototypes from bundle: {e.Message}");
                return new ItemPrototype[0];
            }
        }
        
        /// <summary>
        /// Loads all Sprites from AssetBundle
        /// </summary>
        private Sprite[] LoadSpritesFromBundle(string bundlePath)
        {
            var assetBundle = LoadAssetBundleInternal(bundlePath);
            if (assetBundle == null)
            {
                Debug.LogError($"[AssetBundleLoader] Failed to load AssetBundle from: {bundlePath}");
                return new Sprite[0];
            }
            
            try
            {
                var assetNames = assetBundle.GetAllAssetNames();
                Debug.Log($"[AssetBundleLoader] Found {assetNames.Length} assets in bundle: {string.Join(", ", assetNames)}");
                
                var sprites = new List<Sprite>();
                
                // Load all sprites from bundle (including individual sprites, not atlases)
                var allSprites = assetBundle.LoadAllAssets<Sprite>();
                if (allSprites.Length > 0)
                {
                    sprites.AddRange(allSprites);
                    Debug.Log($"[AssetBundleLoader] Loaded {allSprites.Length} sprites from bundle");
                }
                
                Debug.Log($"[AssetBundleLoader] Successfully loaded {sprites.Count} Sprites from bundle");
                return sprites.ToArray();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AssetBundleLoader] Error loading Sprites from bundle: {e.Message}");
                return new Sprite[0];
            }
        }
        
        /// <summary>
        /// Loads AssetBundle from path
        /// </summary>
        private AssetBundle LoadAssetBundleInternal(string bundlePath)
        {
            if (string.IsNullOrEmpty(bundlePath))
            {
                Debug.LogError("[AssetBundleLoader] Bundle path is null or empty");
                return null;
            }
            
            // Check if bundle is already loaded
            if (_loadedBundles.ContainsKey(bundlePath))
            {
                Debug.Log($"[AssetBundleLoader] Bundle already loaded from: {bundlePath}");
                return _loadedBundles[bundlePath];
            }
            
            try
            {
                var assetBundle = AssetBundle.LoadFromFile(bundlePath);
                if (assetBundle == null)
                {
                    Debug.LogError($"[AssetBundleLoader] Failed to load AssetBundle from: {bundlePath}");
                    return null;
                }
                
                _loadedBundles[bundlePath] = assetBundle;
                Debug.Log($"[AssetBundleLoader] Successfully loaded AssetBundle from: {bundlePath}");
                return assetBundle;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AssetBundleLoader] Error loading AssetBundle from {bundlePath}: {e.Message}");
                return null;
            }
        }
        

        
        /// <summary>
        /// Disposes the loader and unloads all bundles
        /// </summary>
        public void Dispose()
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle?.Unload(true);
            }
            _loadedBundles.Clear();
            Debug.Log("[AssetBundleLoader] Unloaded all AssetBundles");
        }
    }
}
