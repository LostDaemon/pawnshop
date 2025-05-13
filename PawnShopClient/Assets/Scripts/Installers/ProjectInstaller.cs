using Zenject;

public class ProjectInstaller : MonoInstaller
{
    // public GridSettingsScriptableObject GridSettings;
    // public CameraSettingsScriptableObject CameraSettings;

    public override void InstallBindings()
    {
        Container.Bind<GameBootstrapper>().FromComponentInHierarchy().AsSingle();
        Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
        Container.Bind<IItemRepositoryService>().To<ItemRepositoryService>().AsSingle();
        Container.Bind<GameStateMachine>().AsSingle();
        Container.Bind<BootstrapState>().AsSingle();
        Container.Bind<LoadLevelState>().AsSingle();
        Container.Bind<GameLoopState>().AsSingle();



        // Container.Bind<TouchInputSystem>().AsSingle();
        // Container.Bind<InputManager>().AsSingle();
        // Container.Bind<GridSettingsScriptableObject>().FromInstance(GridSettings).AsSingle();
        // Container.Bind<CameraSettingsScriptableObject>().FromInstance(CameraSettings).AsSingle();
        // Container.Bind<GameSceneManager>().AsSingle();
        // Container.Bind<GameManager>().AsSingle();
        // Container.Bind<LootRepository>().AsSingle();
        // Container.Bind<RewardService>().AsSingle();
        // Container.Bind<ItemsFactory>().AsSingle();
        // Container.Bind<TimeManager>().AsSingle();
        // Container.Bind<EventScheduler>().AsSingle();
    }
}