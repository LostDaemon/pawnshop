using PawnShop.Gamestate;
using PawnShop.Infrastructure;
using PawnShop.Models;
using PawnShop.Repositories;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        private const long StartingMoney = 10000L; // TODO: Load from config later
        private const int DefaultSellSlots = 12;

        private void Awake()
        {
            // Make this installer persist between scenes
            DontDestroyOnLoad(gameObject);
        }

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
            Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle();

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
                .AsSingle();

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

            Container.Bind<IItemRepository>()
                .To<ItemRepository>()
                .AsSingle();

            Container.Bind<IStorageRouterService<ItemModel>>()
                .To<StorageRouterService<ItemModel>>()
                .AsSingle();

            // Skills
            Container.Bind<ISkillRepository>()
                .To<SkillRepository>()
                .AsSingle();

            Container.Bind<ISkillService>()
                .To<SkillService>()
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
                    
            // Asset Bundle System
            Container.Bind<AssetBundleLoader>().AsSingle();

            Container.Bind<INegotiationService>()
            .To<NegotiationService>()
            .AsSingle();

            Container.Bind<IItemProcessingService>()
            .To<ItemProcessingService>()
            .AsSingle();

            // Evaluation
            Container.Bind<IEvaluationService>()
                .To<EvaluationService>()
                .AsSingle();

            // Inspection
            Container.Bind<IItemInspectionService>()
                .To<ItemInspectionService>()
                .AsSingle();

            // Player
            Container.Bind<IPlayerService>()
                .To<PlayerService>()
                .AsSingle();

            // Tags
            Container.Bind<ITagRepository>()
                .To<TagRepository>()
                .AsSingle();

            // Languages
            Container.Bind<ILanguageRepository>()
                .To<LanguageRepository>()
                .AsSingle();
        }
    }
}