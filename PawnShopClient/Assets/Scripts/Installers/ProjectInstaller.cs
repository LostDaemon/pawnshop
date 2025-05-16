using Zenject;

public class ProjectInstaller : MonoInstaller
{
  private const long StartingMoney = 1000L; //TODO LOAD FROM CONFIG

  public override void InstallBindings()
  {
    Container.Bind<GameBootstrapper>().FromComponentInHierarchy().AsSingle();
    Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();
    Container.Bind<IItemRepositoryService>().To<ItemRepositoryService>().AsSingle();
    Container.Bind<INegotiateService>().To<NegotiateService>().AsSingle();
    Container.Bind<INavigationService>().To<NavigationService>().AsSingle();
    Container.Bind<GameStateMachine>().AsSingle();
    Container.Bind<BootstrapState>().AsSingle();
    Container.Bind<LoadLevelState>().AsSingle();
    Container.Bind<GameLoopState>().AsSingle();
    Container.Bind<ITimeService>().To<TimeService>().AsSingle();
    Container.Bind<IGameStorageService<ItemModel>>()
      .To<GameStorageService<ItemModel>>()
      .AsSingle();

    Container.Bind<IWalletService>()
          .To<WalletService>()
          .AsSingle()
          .WithArguments(StartingMoney);
  }
}