using PawnShop.Infrastructure;
using PawnShop.Models;
using PawnShop.Repositories;
using PawnShop.Services;
using Zenject;

namespace PawnShop.Gamestate
{
    public class LoadLevelState : IGameState
    {
        private ISceneLoader _sceneLoader;
        private GameStateMachine _stateMachine;
        private IItemRepository _itemRepository;
        private ISkillRepository _skillRepository;
        private ITagRepository _tagRepository;
        private ILocalizationService _localizationService;
        private ILanguageRepository _languageRepository;
        private IPlayerService _playerService;
        private AssetBundleLoader _assetBundleLoader;
        private IStorageLocatorService _storageLocatorService;

        private const int DefaultInventorySlots = 50;
        private const int DefaultSellSlots = 12;


        [Inject]
        public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepository itemRepository, ISkillRepository skillRepository, ITagRepository tagRepository, ILocalizationService localizationService, ILanguageRepository languageRepository, IPlayerService playerService, AssetBundleLoader assetBundleLoader, IStorageLocatorService storageLocatorService)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _itemRepository = itemRepository;
            _skillRepository = skillRepository;
            _tagRepository = tagRepository;
            _localizationService = localizationService;
            _languageRepository = languageRepository;
            _playerService = playerService;
            _assetBundleLoader = assetBundleLoader;
            _storageLocatorService = storageLocatorService;
        }

        public void Enter()
        {
            _itemRepository.Load();
            _skillRepository.Load();
            _tagRepository.Load();
            _languageRepository.Load();

            // Load AssetBundles after repositories
            _assetBundleLoader.LoadAllBundles();

            // Initialize player after loading skill prototypes
            _playerService.InitializePlayer();

            // Initialize storage sizes
            InitializeStorageSizes();

            // Set default language after loading language prototypes
            _localizationService.SwitchLocalization(Language.Russian);

            _sceneLoader.Load("MainScene", () =>
            {
                _stateMachine.Enter<GameLoopState>();
            });
        }

        private void InitializeStorageSizes()
        {
            var inventoryStorage = _storageLocatorService.Get(StorageType.InventoryStorage);
            inventoryStorage.AddSlots(DefaultInventorySlots);

            var sellStorage = _storageLocatorService.Get(StorageType.SellStorage);
            sellStorage.AddSlots(DefaultSellSlots);
        }

        public void Exit() { }
    }
}