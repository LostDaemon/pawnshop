using Zenject;

public class ProjectInstaller : MonoInstaller
{
    private const long StartingMoney = 10000L; // TODO: Load from config later

    public override void InstallBindings()
    {
        // --- Core Game ---
        Container.Bind<GameBootstrapper>().FromComponentInHierarchy().AsSingle();
        Container.Bind<GameStateMachine>().AsSingle();
        Container.Bind<ISceneLoader>().To<SceneLoader>().AsSingle();

        // --- Game States ---
        Container.Bind<BootstrapState>().AsSingle();
        Container.Bind<LoadLevelState>().AsSingle();
        Container.Bind<GameLoopState>().AsSingle();

        // --- Services: Utility ---
        Container.Bind<INavigationService>().To<NavigationService>().AsSingle();
        Container.Bind<ITimeService>().To<TimeService>().AsSingle();

        // --- Game Systems ---
        Container.Bind<IWalletService>()
            .To<WalletService>()
            .AsSingle()
            .WithArguments(StartingMoney);

        Container.Bind<IGameStorageService<ItemModel>>()
            .To<GameStorageService<ItemModel>>()
            .AsSingle();

        Container.Bind<IItemRepositoryService>()
            .To<ItemRepositoryService>()
            .AsSingle();

        Container.Bind<ICustomerFactoryService>()
            .To<CustomerFactoryService>()
            .AsSingle();

        Container.Bind<ICustomerService>()
                .To<CustomerService>()
                .AsSingle();

        Container.Bind<INegotiateService>()
        .To<NegotiateService>()
        .AsSingle();
    }
}