using System;

namespace PawnShop.Infrastructure
{
    public interface ISceneLoader
    {
        void Load(string sceneName, Action onLoaded = null);
    }
}