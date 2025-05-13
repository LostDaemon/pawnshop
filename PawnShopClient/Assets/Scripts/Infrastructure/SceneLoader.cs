using System;
using UnityEngine.SceneManagement;

public class SceneLoader : ISceneLoader
{
    public void Load(string sceneName, Action onLoaded = null)
    {
        var operation = SceneManager.LoadSceneAsync(sceneName);

        if (onLoaded != null)
        {
            operation.completed += HandleCompleted;

            void HandleCompleted(UnityEngine.AsyncOperation op)
            {
                operation.completed -= HandleCompleted;
                onLoaded();
            }
        }
    }
}