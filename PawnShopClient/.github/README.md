# GitHub Actions for Unity Project

This project uses GitHub Actions for automatic Unity project building and testing.

## Setup

### 1. Unity Personal (Free License)

For Unity Personal, **NO ADDITIONAL LICENSE SETUP IS REQUIRED**! Pipelines work "out of the box".

Unity Personal automatically activates in headless mode on GitHub Actions.

### 2. If Additional Setup is Needed

If you encounter problems with automatic activation, you can add Unity license to GitHub Secrets:

1. Go to repository settings → Secrets and variables → Actions
2. Add a new secret named `UNITY_LICENSE`
3. Value can be obtained by running Unity in headless mode on local machine

### 3. Unity Version

Pipelines automatically detect Unity version from `ProjectSettings/ProjectVersion.txt`

## Pipelines

### Unity Build

- **Trigger**: Push to main/develop, Pull Request, manual run
- **Platform**: Windows Standalone (x64)
- **Artifacts**: Built exe file in Builds/Windows folder
- **License**: Unity Personal (automatic activation)

### Unity Test

- **Trigger**: Push to main/develop, Pull Request, manual run
- **Mode**: PlayMode tests
- **Artifacts**: Test results

### Unity Quick Build

- **Trigger**: Push to develop, manual run
- **Platform**: Windows Standalone (x64)
- **Artifacts**: Quick build without caching

## Unity Personal Benefits

✅ **Free** - no additional costs  
✅ **Automatic activation** - no license setup needed  
✅ **Full functionality** - all basic Unity capabilities  
✅ **Suitable for indie games** - revenue up to $100,000 per year

## Caching

Pipelines use caching for:

- `Library/` - Unity Library folder
- `Builds/` - Built files

## Local Development

For local building use Unity Editor or command line:

```bash
Unity.exe -batchmode -quit -projectPath "path/to/project" -buildTarget StandaloneWindows64 -buildPath "Builds/Windows"
```

## Troubleshooting

### License Errors

- Unity Personal works automatically
- If problems - check project settings

### Build Errors

- Check logs in GitHub Actions
- Make sure all dependencies are installed
- Check project settings in Unity

## How to Use

1. **Upload code** to GitHub - pipelines will start automatically
2. **No secrets needed** - Unity Personal works "out of the box"
3. **Built files** will be available in Actions → Artifacts section

## What We Got

🎯 **3 pipelines** for different tasks  
🚀 **Automatic building** on every push  
💾 **Caching** for speed  
🧪 **Automatic testing**  
⚡ **Quick build** for development
