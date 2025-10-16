using UnityEngine;
using PawnShop.Controllers.Teleport;
using PawnShop.Models;
using PawnShop.Repositories;
using Zenject;
using PawnShop.Services;
using PawnShop.Models.Characters;

namespace PawnShop.Controllers
{
    /// <summary>
    /// NPC Controller - moves NPC to target transform
    /// Uses CharacterMovement for smooth movement
    /// </summary>
    public class NpcController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform[] _waypoints;
        [SerializeField] private float _reachDistance = 0.5f;
        [SerializeField] private float _waitTimeAtWaypoint = 2f;

        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 4f;
        [SerializeField] private bool _lookAtTarget = true;
        [SerializeField] private float _searchVerticalThreshold = 0.1f; // Threshold for vertical level checking
        [SerializeField] private float _thresholdX = 0.5f; // Threshold for X coordinate checking

        [Header("NPC Action")]
        [SerializeField] private NpcAction _npcAction = NpcAction.Undefined;

        [Header("References")]
        [SerializeField] private CharacterMovement _characterMovement;
        [SerializeField] private TeleportClientController _teleportClientController;

        // Services
        private ITimeService _timeService;
        private INavigationRepository _navigationRepository;
        private ICustomerService _customerService;

        // State
        private Customer _customer;
        private bool _isMoving = false;
        private bool _isWaiting = false;
        private float _waitTimer = 0f;
        private bool _needTeleport = false;
        private float _currentTargetOffset = 0f; // Random offset for current target

        // Events
        public System.Action OnTargetReached;
        public System.Action OnMovementStarted;
        public System.Action OnMovementStopped;

        /// <summary>
        /// Initialize NPC with character model
        /// </summary>
        public void Init(Customer characterModel)
        {
            _customer = characterModel;
            Debug.Log($"[NpcController] Initialized with customer: {_customer?.GetType().Name}, ID: {_customer?.Id}");

            // Load actions from customer model
            if (_customer != null && _customer.CurrentAction != NpcAction.Undefined)
            {
                SetNpcAction(_customer.CurrentAction);
            }
        }

        /// <summary>
        /// Set NPC action and update waypoints from navigation repository
        /// </summary>
        public void SetNpcAction(NpcAction action)
        {
            _npcAction = action;

            Debug.Log($"[NpcController] Navigation Repository: {_navigationRepository != null}");

            // Get navigation points for this action from repository
            var navigationTransforms = _navigationRepository.GetNavigation(action);

            if (navigationTransforms != null && navigationTransforms.Count > 0)
            {
                // Convert List<Transform> to Transform[]
                _waypoints = navigationTransforms.ToArray();
                Debug.Log($"[NpcController] Set {_waypoints.Length} waypoints for action: {action}");
            }
            else
            {
                Debug.LogWarning($"[NpcController] No navigation points found for action: {action}");
                _waypoints = new Transform[0];
            }
        }

        [Inject]
        public void Construct(ITimeService timeService, INavigationRepository navigationRepository, ICustomerService customerService)
        {
            _timeService = timeService;
            _navigationRepository = navigationRepository;
            _customerService = customerService;

            // Subscribe to customer action changes
            _customerService.OnCustomerActionChanged += OnCustomerActionChanged;
        }

        private void OnDestroy()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerActionChanged -= OnCustomerActionChanged;
            }
        }

        private void OnCustomerActionChanged(NpcAction action)
        {
            Debug.Log($"[NpcController] Received customer action change: {action}");

            // Check if this NPC should respond to this action
            // For now, all NPCs will respond to any action change
            SetNpcAction(action);
            StartMovement();
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

            // Auto-find TeleportClientController if not assigned
            if (_teleportClientController == null)
            {
                _teleportClientController = GetComponent<TeleportClientController>();
                if (_teleportClientController == null)
                {
                    _teleportClientController = GetComponentInParent<TeleportClientController>();
                }
            }
        }

        private void Start()
        {
            if (_waypoints != null && _waypoints.Length > 0)
            {
                StartMovement();
            }
        }

        private void Update()
        {
            if (_isMoving && _waypoints != null && _waypoints.Length > 0)
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
                        RemoveFirstWaypoint();
                        if (_needTeleport)
                        {
                            _isWaiting = false;
                            TryTeleport();
                        }
                        // Check if we need teleport before moving to next waypoint
                    }
                }
                else
                {
                    MoveToCurrentWaypoint();
                }
            }
        }

        private void MoveToCurrentWaypoint()
        {
            if (_characterMovement == null || _waypoints == null || _waypoints.Length == 0) return;
            // Always move to first waypoint (index 0)
            Transform currentWaypoint = _waypoints[0];
            if (currentWaypoint == null) return;

            Debug.Log($"[NpcController] Is postision on the same level: {IsPointOnTheSameLevel(currentWaypoint.position)}");

            // Check if NPC and target are on the same level
            if (!IsPointOnTheSameLevel(currentWaypoint.position))
            {
                Debug.LogWarning($"[NpcController] Cannot move to waypoint - not on the same level. NPC Y: {transform.position.y}, Target Y: {currentWaypoint.position.y}");
                SearchForTeleport();
                return;
            }

            Debug.Log($"[NpcController] Moving towards waypoint 0 at position {currentWaypoint.position}");

            // Generate random offset for this target if not set
            if (_currentTargetOffset == 0f)
            {
                _currentTargetOffset = Random.Range(-_thresholdX, _thresholdX);
            }

            // Calculate target position with offset
            Vector3 targetPosition = new Vector3(currentWaypoint.position.x + _currentTargetOffset, currentWaypoint.position.y, currentWaypoint.position.z);
            Vector3 direction = (targetPosition - transform.position);
            float distance = direction.magnitude;

            if (distance <= _reachDistance)
            {
                Debug.Log($"Reached waypoint 0, distance={distance}, offset={_currentTargetOffset}");

                _characterMovement.SetHorizontalInput(0f);
                _isWaiting = true;
                _waitTimer = _waitTimeAtWaypoint;
                OnTargetReached?.Invoke();
                return;
            }

            float horizontalInput = direction.x > 0 ? 1f : -1f;

            _characterMovement.SetHorizontalInput(horizontalInput);

            if (_lookAtTarget)
            {
                _characterMovement.SetFacingDirection(horizontalInput);
            }
        }

        /// <summary>
        /// Check if target point is on the same level as NPC (Y coordinate)
        /// </summary>
        private bool IsPointOnTheSameLevel(Vector3 targetPoint)
        {
            return Mathf.Abs(transform.position.y - targetPoint.y) <= _searchVerticalThreshold;
        }

        /// <summary>
        /// Search for teleport to reach target level
        /// </summary>
        private void SearchForTeleport()
        {
            _needTeleport = true;

            // If already need teleport, check if first waypoint is already a teleport
            if (_waypoints != null && _waypoints.Length > 0)
            {
                var firstWaypoint = _waypoints[0];
                if (firstWaypoint != null && firstWaypoint.GetComponent<TeleportController>() != null)
                {
                    // Remove existing teleport waypoint first
                    RemoveFirstWaypoint();
                }
            }

            // Find all TeleportController objects on the scene
            var teleportControllers = FindObjectsByType<TeleportController>(FindObjectsSortMode.None);

            // Find teleport on the same level as NPC
            foreach (var teleport in teleportControllers)
            {
                if (IsPointOnTheSameLevel(teleport.transform.position))
                {
                    Debug.Log($"[NpcController] Found teleport on same level: {teleport.name}");

                    // Insert teleport as first waypoint
                    InsertAsFirstWaypoint(teleport.transform);
                    return;
                }
            }

            Debug.LogWarning("[NpcController] No teleport found on the same level");
        }

        /// <summary>
        /// Try to teleport
        /// </summary>
        private void TryTeleport()
        {
            if (_teleportClientController == null)
            {
                Debug.LogWarning("[NpcController] TeleportClientController not found!");
                return;
            }

            if (_waypoints == null || _waypoints.Length == 0)
            {
                Debug.LogWarning("[NpcController] No waypoints to compare!");
                return;
            }

            // Get target waypoint (first in list)
            Transform targetWaypoint = _waypoints[0];
            if (targetWaypoint == null)
            {
                Debug.LogWarning("[NpcController] Target waypoint is null!");
                return;
            }

            // Teleport to target level
            Debug.Log($"[NpcController] Teleporting to target level");
            _teleportClientController.TryTeleportToTarget(targetWaypoint, _searchVerticalThreshold);

            _needTeleport = false;
        }

        /// <summary>
        /// Insert transform as first waypoint
        /// </summary>
        private void InsertAsFirstWaypoint(Transform transformToInsert)
        {
            if (_waypoints == null)
            {
                _waypoints = new Transform[] { transformToInsert };
            }
            else
            {
                // Create new array with transform as first element
                Transform[] newWaypoints = new Transform[_waypoints.Length + 1];
                newWaypoints[0] = transformToInsert;

                // Copy existing waypoints
                for (int i = 0; i < _waypoints.Length; i++)
                {
                    newWaypoints[i + 1] = _waypoints[i];
                }

                _waypoints = newWaypoints;
            }

            Debug.Log($"[NpcController] Inserted as first waypoint: {transformToInsert.name}");
        }

        /// <summary>
        /// Remove the first waypoint from the list and continue movement
        /// </summary>
        private void RemoveFirstWaypoint()
        {
            if (_waypoints == null || _waypoints.Length == 0) return;

            // Reset offset for next target
            _currentTargetOffset = 0f;

            // Create new array without the first element
            Transform[] newWaypoints = new Transform[_waypoints.Length - 1];
            for (int i = 1; i < _waypoints.Length; i++)
            {
                newWaypoints[i - 1] = _waypoints[i];
            }

            _waypoints = newWaypoints;

            // If no waypoints left, stop movement
            if (_waypoints.Length == 0)
            {
                StopMovement();
                return;
            }

            // Continue moving to new first waypoint
            Debug.Log($"Removed first waypoint, {_waypoints.Length} waypoints remaining");
        }

        public void SetWaypoints(Transform[] newWaypoints)
        {
            _waypoints = newWaypoints;
        }

        public void SetWaitTime(float time)
        {
            _waitTimeAtWaypoint = time;
        }

        /// <summary>
        /// Start moving to current target
        /// </summary>
        public void StartMovement()
        {
            Debug.Log($"StartMovement: waypoints={_waypoints != null && _waypoints.Length > 0}, characterMovement={_characterMovement != null}");

            if (_waypoints == null || _waypoints.Length == 0)
            {
                Debug.LogWarning("[NpcController] No waypoints set!");
                return;
            }

            if (_characterMovement == null)
            {
                Debug.LogWarning("[NpcController] No CharacterMovement found!");
                return;
            }

            _isMoving = true;
            _isWaiting = false;
            OnMovementStarted?.Invoke();

            Debug.Log($"Movement started: isMoving={_isMoving}");
        }

        /// <summary>
        /// Stop movement
        /// </summary>
        public void StopMovement()
        {
            _isMoving = false;

            if (_characterMovement != null)
            {
                _characterMovement.SetHorizontalInput(0f);
            }

            OnMovementStopped?.Invoke();
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

        public int GetCurrentWaypointIndex()
        {
            return 0; // Always return 0 since we always target the first waypoint
        }

        public Transform GetCurrentWaypoint()
        {
            if (_waypoints == null || _waypoints.Length == 0) return null;
            return _waypoints[0]; // Always return first waypoint
        }
    }
}
