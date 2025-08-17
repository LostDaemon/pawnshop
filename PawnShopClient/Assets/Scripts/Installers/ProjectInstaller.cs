using Zenject;

public class ProjectInstaller : MonoInstaller
{
    private const long StartingMoney = 10000L; // TODO: Load from config later
    private const int DefaultSellSlots = 12;

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

        Container.Bind<INegotiationHistoryService>()
    .To<NegotiationHistoryService>()
    .AsSingle();

        Container.Bind<ISpriteService>()
            .To<SpriteService>()
            .AsSingle()
            .WithArguments("Sprites/ItemsAtlas");

        // Inventory
        Container.Bind<IGameStorageService<ItemModel>>()
            .WithId(StorageType.InventoryStorage)
            .To<InventoryStorage>()
            .AsSingle();

        // Sell storage
        Container.Bind<IGameStorageService<ItemModel>>()
            .WithId(StorageType.SellStorage)
            .To<SellStorageService>()
            .AsSingle();

        Container.Bind<IStorageLocatorService>()
                  .To<StorageLocatorService>()
                  .AsSingle();

        Container.Bind<IItemRepositoryService>()
            .To<ItemRepositoryService>()
            .AsSingle();

        Container.Bind<IStorageRouterService<ItemModel>>()
            .To<StorageRouterService<ItemModel>>()
            .AsSingle();

        Container.Bind<ICustomerFactoryService>()
            .To<CustomerFactoryService>()
            .AsSingle();

        Container.Bind<ICustomerService>()
                .To<CustomerService>()
                .AsSingle();

        Container.Bind<ISellService>()
                .To<SellService>()
                .AsSingle()
                .WithArguments(DefaultSellSlots);

        Container.Bind<INegotiationService>()
        .To<NegotiationService>()
        .AsSingle();

        Container.Bind<IItemProcessingService>()
        .To<ItemProcessingService>()
        .AsSingle();

        // Skills
        Container.Bind<ISkillRepositoryService>()
            .To<SkillRepositoryService>()
            .AsSingle();
            
        Container.Bind<ISkillService>()
            .To<SkillService>()
            .AsSingle();

        // Tags
        Container.Bind<ITagRepositoryService>()
            .To<TagRepositoryService>()
            .AsSingle();
    }
}