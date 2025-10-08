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

        [Inject]
        public void Construct(ICustomerService customerService)
        {
            _customerService = customerService;
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
                if (npcController != null && waypoints != null && waypoints.Length > 0)
                {
                    npcController.SetWaypoints(waypoints);
                }
            }
        }
    }
}
