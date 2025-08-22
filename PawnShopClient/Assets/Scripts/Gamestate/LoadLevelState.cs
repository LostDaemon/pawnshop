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


        [Inject]
        public void Construct(GameStateMachine stateMachine, ISceneLoader sceneLoader, IItemRepository itemRepository, ISkillRepository skillRepository, ITagRepository tagRepository, ILocalizationService localizationService, ILanguageRepository languageRepository, IPlayerService playerService)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _itemRepository = itemRepository;
            _skillRepository = skillRepository;
            _tagRepository = tagRepository;
            _localizationService = localizationService;
            _languageRepository = languageRepository;
            _playerService = playerService;
        }

        public void Enter()
        {
            _itemRepository.Load();
            _skillRepository.Load();
            _tagRepository.Load();
            _languageRepository.Load();

            // Initialize player after loading skill prototypes
            _playerService.InitializePlayer();

            // Set default language after loading language prototypes
            _localizationService.SwitchLocalization(Language.Russian);

            _sceneLoader.Load("MainScene", () =>
            {
                _stateMachine.Enter<GameLoopState>();
            });
        }

        public void Exit() { }
    }
}