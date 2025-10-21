using UnityEngine;
using Zenject;
using PawnShop.Services;
using PawnShop.Models.Characters;
using PawnShop.Repositories;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class CustomerSpawnController : MonoBehaviour
    {
        [SerializeField] private GameObject customerPrefab;

        private ICustomerService _customerService;
        private ITimeService _timeService;
        private INavigationRepository _navigationRepository;
        private IWalletService _walletService;
        private IStorageLocatorService _storageLocator;

        [Inject]
        public void Construct(ICustomerService customerService, ITimeService timeService, INavigationRepository navigationRepository, IWalletService walletService, IStorageLocatorService storageLocator)
        {
            _customerService = customerService;
            _timeService = timeService;
            _navigationRepository = navigationRepository;
            _walletService = walletService;
            _storageLocator = storageLocator;
        }

        private void Start()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged += OnCustomerChanged;
            }
        }

        private void OnDestroy()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
            }
        }

        private void OnCustomerChanged(Customer customer)
        {
            Debug.Log($"[CustomerSpawnController] OnCustomerChanged triggered. Customer: {(customer != null ? customer.Id.ToString() : "null")}");

            if (customer == null || customerPrefab == null)
            {
                return;
            }

            GameObject customerInstance = Instantiate(customerPrefab, transform.position, transform.rotation, transform);

            // Get components from spawned customer
            var npcController = customerInstance.GetComponent<NpcController>();
            var characterMovement = customerInstance.GetComponent<CharacterMovement>();

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
    }
}
