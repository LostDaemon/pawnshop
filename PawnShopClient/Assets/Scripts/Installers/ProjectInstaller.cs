using PawnShop.Controllers;
using PawnShop.Gamestate;
using PawnShop.Infrastructure;
using PawnShop.Models;
using PawnShop.Models.EventsSystem;
using PawnShop.Repositories;
using PawnShop.Services;
using PawnShop.Services.EventSystem;
using Zenject;

namespace PawnShop.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        private const long StartingMoney = 10000L; // TODO: Load from config later
        // Time settings
        private static readonly GameTime InitialTime = new(1, 7, 50);

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
            Container.Bind<ITimeService>().To<TimeService>().AsSingle();
            Container.Bind<GameTime>().FromInstance(InitialTime).WhenInjectedInto<TimeService>();
            Container.Bind<IEventsQueueService>().To<EventsQueueService>().AsSingle();
            Container.Bind<ILocalizationService>().To<LocalizationService>().AsSingle();
            Container.Bind<INavigationService>().To<NavigationService>().AsSingle();

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
            Container.Bind<ISlotStorageService<ItemModel>>()
                .WithId(StorageType.InventoryStorage)
                .To<InventoryStorage>()
                .AsSingle();

            // Sell storage
            Container.Bind<ISlotStorageService<ItemModel>>()
                .WithId(StorageType.SellStorage)
                .To<SellStorageService>()
                .AsSingle();

            // Workshop storage
            Container.Bind<ISlotStorageService<ItemModel>>()
                .WithId(StorageType.WorkshopStorage)
                .To<WorkshopStorageService>()
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

            // Item Transfer
            Container.Bind<IItemTransferService>()
                .To<ItemTransferService>()
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

            // Shelf Service
            Container.Bind<IShelfService>()
                    .To<ShelfService>()
                    .AsSingle();

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

            // Drag and Drop
            Container.Bind<IDragNDropService<ItemModel>>()
                .To<DragNDropService<ItemModel>>()
                .AsSingle();

            // --- Event System ---
            Container.Bind<IEventProcessingService>()
                .To<EventProcessingService>()
                .AsSingle();

            Container.Bind<ISystemEventInitializer>()
                .To<SystemEventInitializer>()
                .AsSingle();

            // Event Processors
            Container.Bind<EventProcessorBase<SystemEvent>>()
                .To<SystemEventProcessor>()
                .AsSingle();

            Container.Bind<EventProcessorBase<CustomerEvent>>()
                .To<CustomerEventProcessor>()
                .AsSingle();
        }
    }
}