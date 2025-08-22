using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using PawnShop.ScriptableObjects;
using System.Collections.Generic;

namespace PawnShop.Editor
{
    public class ItemBundleBuilder : EditorWindow
    {
        private string bundleOutputPath = "Assets/AssetBundles/Items";
        private string resourcesInputPath = "Assets/Resources/ScriptableObjects/Items";
        private string bundleExtension = ".bundle";
        private bool includeDependencies = true;
        private bool showAdvancedCategories = false;
        private BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        
        [MenuItem("Tools/Item Bundle Builder")]
        public static void ShowWindow()
        {
            GetWindow<ItemBundleBuilder>("Item Bundle Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Item Bundle Builder", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // Resources input path (contains both prototypes and sprites)
            EditorGUILayout.BeginHorizontal();
            resourcesInputPath = EditorGUILayout.TextField("Resources Path:", resourcesInputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Resources Path (prototypes + sprites)", resourcesInputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    resourcesInputPath = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Bundle output path
            EditorGUILayout.BeginHorizontal();
            bundleOutputPath = EditorGUILayout.TextField("Output Path:", bundleOutputPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Bundle Output Path", bundleOutputPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    bundleOutputPath = path.Replace(Application.dataPath, "Assets");
                }
            }
            EditorGUILayout.EndHorizontal();
            
            // Bundle extension
            bundleExtension = EditorGUILayout.TextField("Bundle Extension:", bundleExtension);
            
            EditorGUILayout.Space();
            
            // Bundle options
            EditorGUILayout.LabelField("Bundle Type: Single Folder Bundle", EditorStyles.boldLabel);
            includeDependencies = EditorGUILayout.Toggle("Include Dependencies", includeDependencies);
            
            EditorGUILayout.Space();
            showAdvancedCategories = EditorGUILayout.Foldout(showAdvancedCategories, "Advanced Category Settings");
            
            if (showAdvancedCategories)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "All items from the selected folder will be grouped into a single bundle.\n\n" +
                    "Bundle name is automatically generated from folder name.\n" +
                    "Script assigns AssetBundle names. You organize files into folders manually.", 
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space();
            
            // Build target
            buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target:", buildTarget);
            
            EditorGUILayout.Space();
            
            // Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Build All Item Bundles"))
            {
                BuildAllItemBundles();
            }
            if (GUILayout.Button("Clear All Bundles"))
            {
                ClearAllBundles();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // Info
            EditorGUILayout.HelpBox(
                "This tool will create a single AssetBundle from all item prototypes and sprites in the selected folder.\n\n" +
                "Bundle name is automatically generated from folder name.\n" +
                "Script loads prototypes and sprites from the same folder.\n" +
                "You organize files into bundle folders manually.", 
                MessageType.Info);
        }

        private void BuildAllItemBundles()
        {
            try
            {
                // Ensure output directory exists
                if (!Directory.Exists(bundleOutputPath))
                {
                    Directory.CreateDirectory(bundleOutputPath);
                }

                // Load all item prototypes from selected path
                var itemPrototypes = LoadItemPrototypesFromPath(resourcesInputPath);
                Debug.Log($"Found {itemPrototypes.Length} item prototypes in {resourcesInputPath}");

                // Create single bundle from the folder
                BuildSingleBundle(itemPrototypes);

                // Build all bundles
                BuildAssetBundles();
                
                EditorUtility.DisplayDialog("Success", 
                    $"Successfully created {itemPrototypes.Length} item bundles in {bundleOutputPath}", "OK");
                
                AssetDatabase.Refresh();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error building item bundles: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to build bundles: {e.Message}", "OK");
            }
        }

        private void BuildSingleBundle(ItemPrototype[] itemPrototypes)
        {
            // Get folder name for bundle name
            var folderName = Path.GetFileName(resourcesInputPath);
            var bundleName = $"bundle_{folderName}";
            
            Debug.Log($"Creating single bundle '{bundleName}' from folder: {resourcesInputPath}");
            
            // Add all prototypes to this bundle
            foreach (var prototype in itemPrototypes)
            {
                AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(prototype)).assetBundleName = bundleName;
                
                // Add sprite directly if it exists
                if (prototype.Image != null)
                {
                    AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(prototype.Image)).assetBundleName = bundleName;
                }
            }
            
            Debug.Log($"Created bundle '{bundleName}' with {itemPrototypes.Length} prototypes and their sprites");
            Debug.Log($"Bundle '{bundleName}' ready for manual folder organization");
        }

        private ItemPrototype[] LoadItemPrototypesFromPath(string path)
        {
            var prototypes = new List<ItemPrototype>();
            
            // Convert Assets path to full system path
            var fullPath = path.Replace("Assets", Application.dataPath);
            
            if (!Directory.Exists(fullPath))
            {
                Debug.LogError($"Directory does not exist: {fullPath}");
                return prototypes.ToArray();
            }
            
            // Get all .asset files in the directory
            var assetFiles = Directory.GetFiles(fullPath, "*.asset", SearchOption.TopDirectoryOnly);
            
            foreach (var assetFile in assetFiles)
            {
                // Convert system path back to Assets path
                var assetPath = assetFile.Replace(Application.dataPath, "Assets");
                
                // Load the asset
                var asset = AssetDatabase.LoadAssetAtPath<ItemPrototype>(assetPath);
                if (asset != null)
                {
                    prototypes.Add(asset);
                }
            }
            
            return prototypes.ToArray();
        }



        private void BuildAssetBundles()
        {
            var buildPath = Path.Combine(bundleOutputPath, buildTarget.ToString());
            
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }

            var manifest = BuildPipeline.BuildAssetBundles(
                buildPath,
                BuildAssetBundleOptions.None,
                buildTarget);

            Debug.Log($"Asset bundles built successfully. Manifest: {manifest}");
            
            // Rename bundles to add extension
            RenameBundlesWithExtension(buildPath);
        }
        
        private void RenameBundlesWithExtension(string buildPath)
        {
            if (string.IsNullOrEmpty(bundleExtension))
                return;
                
            var bundleFiles = Directory.GetFiles(buildPath).Where(f => !f.EndsWith(".meta") && !f.EndsWith(".manifest")).ToArray();
            
            foreach (var bundleFile in bundleFiles)
            {
                var newName = bundleFile + bundleExtension;
                if (File.Exists(newName))
                {
                    File.Delete(newName);
                }
                File.Move(bundleFile, newName);
                Debug.Log($"Renamed bundle: {Path.GetFileName(bundleFile)} -> {Path.GetFileName(newName)}");
            }
        }

        private void ClearAllBundles()
        {
            if (EditorUtility.DisplayDialog("Clear Bundles", 
                "This will remove all asset bundle assignments. Continue?", "Yes", "No"))
            {
                // Clear all asset bundle names
                var allAssets = AssetDatabase.GetAllAssetPaths()
                    .Where(path => path.StartsWith("Assets/"))
                    .Select(path => AssetImporter.GetAtPath(path))
                    .Where(importer => !string.IsNullOrEmpty(importer.assetBundleName));

                foreach (var importer in allAssets)
                {
                    importer.assetBundleName = "";
                }

                AssetDatabase.Refresh();
                Debug.Log("All asset bundle assignments cleared");
            }
        }
    }
}
