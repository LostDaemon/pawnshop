using UnityEngine;
using PawnShop.Repositories;
using Zenject;
using PawnShop.Services;
using PawnShop.Models.Characters;
using PawnShop.Models.Npc;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    /// <summary>
    /// NPC Controller - moves NPC to target transform
    /// Uses CharacterMovement for smooth movement
    /// </summary>
    public class NpcController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private NpcTask[] _tasks;
        [SerializeField] private float _reachDistance = 0.1f;
        [SerializeField] private float _waitTimeAtWaypoint = 0f;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private bool _lookAtTarget = true;
        [SerializeField] private float _thresholdX = 0.1f; // Threshold for X coordinate checking

        [Header("References")]
        [SerializeField] private CharacterMovement _characterMovement;

        // Services
        private ITimeService _timeService;
        private INavigationRepository _navigationRepository;
        private ICustomerService _customerService;
        private IWalletService _walletService;
        private IStorageLocatorService _storageLocator;

        // State
        private Customer _customer;
        private bool _isMoving = false;
        private bool _isWaiting = false;
        private float _waitTimer = 0f;
        private float _currentTargetOffset = 0f; // Random offset for current target

        public Customer Model => _customer;

        /// <summary>
        /// Initialize NPC with character model
        /// </summary>
        public void Init(Customer characterModel)
        {
            _customer = characterModel;

            // Load actions from customer model
            if (_customer != null)
            {
                SetNpcType(_customer.CustomerType);
            }
        }

        private void SetNpcType(NpcType npcType)
        {
            // Get navigation points for this action from repository
            var npcTasks = _navigationRepository.GetNavigation(npcType);

            if (npcTasks != null && npcTasks.Count > 0)
            {
                // Convert List<Transform> to Transform[]
                _tasks = npcTasks.ToArray();
            }
            else
            {
                Debug.LogWarning($"[NpcController] No navigation points found for action: {npcType}");
                _tasks = new NpcTask[0];
            }
        }

        [Inject]
        public void Construct(ITimeService timeService, INavigationRepository navigationRepository, ICustomerService customerService, IWalletService walletService, IStorageLocatorService storageLocator)
        {
            _timeService = timeService;
            _navigationRepository = navigationRepository;
            _customerService = customerService;
            _walletService = walletService;
            _storageLocator = storageLocator;

            // Subscribe to NPC action events
            _customerService.OnNpcAction += OnNpcActionTriggered;
        }

        private void Awake()
        {
            // Auto-find CharacterMovement if not assigned
            if (_characterMovement == null)
            {
                _characterMovement = GetComponent<CharacterMovement>();
                if (_characterMovement == null)
                {
                    _characterMovement = GetComponentInParent<CharacterMovement>();
                }
            }
        }

        private void Start()
        {
            if (_tasks != null && _tasks.Length > 0)
            {
                StartTasks();
            }
        }

        private void Update()
        {
            if (_isMoving && _tasks != null && _tasks.Length > 0)
            {
                if (_isWaiting)
                {
                    // Use time multiplier to scale wait timer
                    float timeMultiplier = _timeService?.TimeMultiplier ?? 60f;
                    float timeScale = timeMultiplier / 60f;
                    _waitTimer -= Time.deltaTime * timeScale;
                    if (_waitTimer <= 0f)
                    {
                        _isWaiting = false;
                        // Remove the reached waypoint (index 0) and continue to new first waypoint
                        RemoveFirstTask();
                    }
                }
                else
                {
                    ProceedNextTask();
                }
            }
        }

        private void ProceedNextTask()
        {
            if (_characterMovement == null || _tasks == null || _tasks.Length == 0) return;
            // Always move to first waypoint (index 0)
            var currentTask = _tasks[0];
            if (currentTask == null) return;
            if (currentTask.Type == NpcTaskType.WalkTo || currentTask.Type == NpcTaskType.RunTo)
            {
                // Generate random offset for this target if not set
                if (_currentTargetOffset == 0f)
                {
                    _currentTargetOffset = UnityEngine.Random.Range(-_thresholdX, _thresholdX);
                }

                // Calculate target position with offset
                Vector3 targetPosition = new Vector3(currentTask.Target.position.x + _currentTargetOffset, currentTask.Target.position.y, currentTask.Target.position.z);
                Vector3 direction = (targetPosition - transform.position);
                float distance = direction.magnitude;

                if (distance <= _reachDistance)
                {

                    _characterMovement.SetHorizontalInput(0f);
                    _isWaiting = true;
                    _waitTimer = _waitTimeAtWaypoint;
                    return;
                }

                float horizontalInput = direction.x > 0 ? 1f : -1f;

                _characterMovement.SetHorizontalInput(horizontalInput);

                if (_lookAtTarget)
                {
                    _characterMovement.SetFacingDirection(horizontalInput);
                }
            }
        }


        /// <summary>
        /// Remove the first waypoint from the list and continue movement
        /// </summary>
        private void RemoveFirstTask()
        {
            if (_tasks == null || _tasks.Length == 0) return;

            // Reset offset for next target
            _currentTargetOffset = 0f;

            // Create new array without the first element
            var newTasks = new NpcTask[_tasks.Length - 1];
            for (int i = 1; i < _tasks.Length; i++)
            {
                newTasks[i - 1] = _tasks[i];
            }

            _tasks = newTasks;

            // If no waypoints left, stop movement
            if (_tasks.Length == 0)
            {
                EndTasks();
                return;
            }

            // Check if next task is SelfDestroy
            var nextTask = _tasks[0];
            if (nextTask != null && nextTask.Type == NpcTaskType.SelfDestroy)
            {
                ExecuteSelfDestroy();
                return;
            }

            // Continue moving to new first waypoint
        }

        public void SetTasks(NpcTask[] newTasks)
        {
            _tasks = newTasks;
        }

        public void SetWaitTime(float time)
        {
            _waitTimeAtWaypoint = time;
        }

        /// <summary>
        /// Start moving to current target
        /// </summary>
        public void StartTasks()
        {

            if (_tasks == null || _tasks.Length == 0)
            {
                Debug.LogWarning("[NpcController] No tasks set!");
                return;
            }

            if (_characterMovement == null)
            {
                Debug.LogWarning("[NpcController] No CharacterMovement found!");
                return;
            }

            _isMoving = true;
            _isWaiting = false;

        }

        /// <summary>
        /// Stop movement
        /// </summary>
        public void EndTasks()
        {
            _isMoving = false;

            if (_characterMovement != null)
            {
                _characterMovement.SetHorizontalInput(0f);
            }
        }


        /// <summary>
        /// Set reach distance
        /// </summary>
        public void SetReachDistance(float distance)
        {
            _reachDistance = distance;
        }

        /// <summary>
        /// Set movement speed
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = speed;
            if (_characterMovement != null)
            {
                _characterMovement.SetMoveSpeed(speed);
            }
        }

        /// <summary>
        /// Check if character is moving
        /// </summary>
        public bool IsMoving()
        {
            return _isMoving;
        }

        public int GetCurrentTaskIndex()
        {
            return 0; // Always return 0 since we always target the first task
        }

        public NpcTask GetCurrentTask()
        {
            if (_tasks == null || _tasks.Length == 0) return null;
            return _tasks[0]; // Always return first task
        }

        /// <summary>
        /// Handle NPC action events from CustomerService
        /// </summary>
        private void OnNpcActionTriggered(string npcId, NpcAction action)
        {
            // Check if this action is for current customer or for all customers (npcId == null)
            if (_customer != null && (_customer.Id == npcId || npcId == null))
            {
                // Execute action based on trigger
                ExecuteActionByTrigger(action);
            }
        }

        /// <summary>
        /// Execute specific action based on trigger
        /// </summary>
        private void ExecuteActionByTrigger(NpcAction trigger)
        {
            var currentTask = GetCurrentTask();
            if (currentTask == null)
            {
                Debug.LogWarning($"[NpcController] No current task to process trigger: {trigger}");
                return;
            }

            switch (currentTask.Type)
            {
                case NpcTaskType.WaitFor:
                    HandleWaitForTask(currentTask, trigger);
                    break;

                case NpcTaskType.SelfDestroy:
                    ExecuteSelfDestroy();
                    break;

                default:
                    Debug.Log($"[NpcController] Current task type {currentTask.Type} doesn't respond to trigger events");
                    break;
            }
        }

        /// <summary>
        /// Handle WaitFor task - check if trigger matches and proceed to next task
        /// </summary>
        private void HandleWaitForTask(NpcTask task, NpcAction trigger)
        {
            if (task.Trigger == trigger)
            {

                // Execute specific action if it's BuyAttempt
                if (trigger == NpcAction.BuyAttempt)
                {
                    BuyAttempt();
                }

                // Stop waiting and proceed to next task
                _isWaiting = false;
                RemoveFirstTask();

                // Continue with next task if available
                if (_tasks != null && _tasks.Length > 0)
                {
                    ProceedNextTask();
                }
                else
                {
                    EndTasks();
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// Execute SelfDestroy - destroy the controller
        /// </summary>
        private void ExecuteSelfDestroy()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// Handle buy attempt action
        /// </summary>
        private void BuyAttempt()
        {

            if (_customer == null)
            {
                Debug.LogWarning("[NpcController] No customer");
                return;
            }

            // Get sell storage
            var sellStorage = _storageLocator.Get(StorageType.SellStorage);
            if (sellStorage == null)
            {
                Debug.LogError("[NpcController] Sell storage not found");
                return;
            }

            // Check if there are items in sell storage
            var occupiedSlots = sellStorage.GetOccupiedSlotsCount();
            if (occupiedSlots == 0)
            {
                return;
            }

            // Get random item from sell storage
            var randomSlot = UnityEngine.Random.Range(0, occupiedSlots);
            var itemToBuy = sellStorage.Get(randomSlot);
            
            if (itemToBuy == null)
            {
                Debug.LogWarning("[NpcController] Failed to get random item from sell storage");
                return;
            }

            // Get offer price for the item
            var offerPrice = itemToBuy.CurrentOffer;
            if (offerPrice <= 0)
            {
                Debug.LogWarning($"[NpcController] Item {itemToBuy.Name} has no valid offer price");
                return;
            }

            // Check if customer has enough money (assuming customer has unlimited money for now)
            // TODO: Implement customer money system

            // Buy the item - remove from sell storage and add money to player
            sellStorage.Withdraw(randomSlot);
            _walletService.TransactionAttempt(CurrencyType.Money, offerPrice);

        }

        /// <summary>
        /// Cleanup subscriptions when destroyedф
        /// </summary>
        private void OnDestroy()
        {
            if (_customerService != null)
            {
                _customerService.OnNpcAction -= OnNpcActionTriggered;
            }
        }
    }
}
