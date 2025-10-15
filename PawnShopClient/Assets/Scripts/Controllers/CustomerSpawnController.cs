using UnityEngine;
using Zenject;
using PawnShop.Services;
using PawnShop.Models.Characters;

namespace PawnShop.Controllers
{
    public class CustomerSpawnController : MonoBehaviour
    {
        [SerializeField] private GameObject customerPrefab;
        [SerializeField] private Transform[] waypoints;

        private ICustomerService _customerService;
        private ITimeService _timeService;

        [Inject]
        public void Construct(ICustomerService customerService, ITimeService timeService)
        {
            _customerService = customerService;
            _timeService = timeService;
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
            if (customerPrefab != null)
            {
                GameObject customerInstance = Instantiate(customerPrefab, transform.position, transform.rotation, transform);
                
                // Assign waypoints to the spawned customer
                var npcController = customerInstance.GetComponent<NpcController>();
                var characterMovement = customerInstance.GetComponent<CharacterMovement>();
                
                if (npcController != null && waypoints != null && waypoints.Length > 0)
                {
                    npcController.SetWaypoints(waypoints);
                }
                
                // Manually inject TimeService into components since they're created via Instantiate
                if (characterMovement != null)
                {
                    characterMovement.Construct(_timeService);
                }
                if (npcController != null)
                {
                    npcController.Construct(_timeService);
                }
            }
        }
    }
}
