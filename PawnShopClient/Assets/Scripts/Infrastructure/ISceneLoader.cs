using System;

public interface ISceneLoader
{
    void Load(string sceneName, Action onLoaded = null);
}