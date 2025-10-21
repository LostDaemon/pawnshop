using UnityEngine;
using PawnShop.Services;
using PawnShop.Models;
using PawnShop.Models.Events;
using PawnShop.Models.Characters;
using PawnShop.Models.Npc;
using Zenject;
using PawnShop.Repositories;

namespace PawnShop.Controllers
{
    /// <summary>
    /// Controller for spawning buyers in the game
    /// </summary>
    public class BuyerSpawnerController : MonoBehaviour
    {
        [SerializeField] private GameObject buyerPrefab;

        private ITimeService _timeService;
        private ICustomerFactoryService _customerFactory;
        private INavigationRepository _navigationRepository;
        private ICustomerService _customerService;
        private IWalletService _walletService;
        private IStorageLocatorService _storageLocator;

        [Inject]
        public void Construct(ITimeService timeService, ICustomerFactoryService customerFactory, INavigationRepository navigationRepository, ICustomerService customerService, IWalletService walletService, IStorageLocatorService storageLocator)
        {
            _timeService = timeService;
            _customerFactory = customerFactory;
            _navigationRepository = navigationRepository;
            _customerService = customerService;
            _walletService = walletService;
            _storageLocator = storageLocator;
        }

        private void Start()
        {
            // Subscribe to time service events
            if (_timeService != null)
            {
                _timeService.OnEventTriggered += OnEventTriggered;
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (_timeService != null)
            {
                _timeService.OnEventTriggered -= OnEventTriggered;
            }
        }

        private void OnEventTriggered(IGameEvent gameEvent)
        {
            // Check if this is a customer buyer event
            if (gameEvent.EventType == GameEventType.CustomerBuyer)
            {
                SpawnBuyerCustomer();
            }
        }

        private void SpawnBuyerCustomer()
        {
            if (_customerFactory == null)
            {
                Debug.LogError("[BuyerSpawnerController] CustomerFactory is null!");
                return;
            }

            if (buyerPrefab == null)
            {
                Debug.LogError("[BuyerSpawnerController] Buyer prefab is not assigned!");
                return;
            }

            // Generate a buyer customer
            var customer = _customerFactory.GenerateCustomer(NpcType.Buyer);

            if (customer != null)
            {
                Debug.Log($"[BuyerSpawnerController] Spawning buyer customer: ID={customer.Id}, Item={customer.OwnedItem?.Name}");

                // Spawn buyer prefab
                GameObject buyerInstance = Instantiate(buyerPrefab, transform.position, transform.rotation, transform);

                // Get components from spawned buyer
                var npcController = buyerInstance.GetComponent<NpcController>();
                var characterMovement = buyerInstance.GetComponent<CharacterMovement>();

                // Manually inject services into components since they're created via Instantiate
                if (characterMovement != null)
                {
                    characterMovement.Construct(_timeService);
                }
                if (npcController != null)
                {
                    npcController.Construct(_timeService, _navigationRepository, _customerService, _walletService, _storageLocator);
                    npcController.Init(customer);
                }
            }
            else
            {
                Debug.LogError("[BuyerSpawnerController] Failed to generate customer!");
            }
        }
    }
}
