using UnityEngine;
using PawnShop.Controllers.Teleport;
using PawnShop.Models;
using Zenject;
using PawnShop.Services;

namespace PawnShop.Controllers
{
    /// <summary>
    /// NPC Controller - moves NPC to target transform
    /// Uses CharacterMovement for smooth movement
    /// </summary>
    public class NpcController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float reachDistance = 0.5f;
        [SerializeField] private float waitTimeAtWaypoint = 2f;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private bool lookAtTarget = true;
        [SerializeField] private float searchVerticalThreshold = 0.1f; // Threshold for vertical level checking

        [Header("NPC Action")]
        [SerializeField] private NpcAction npcAction = NpcAction.Undefined;
        [SerializeField] private float thresholdX = 0.5f; // Threshold for X coordinate checking

        [Header("References")]
        [SerializeField] private CharacterMovement characterMovement;
        [SerializeField] private TeleportClientController teleportClientController;

        // Services
        private ITimeService timeService;

        // State
        private bool isMoving = false;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        private bool needTeleport = false;
        private float currentTargetOffset = 0f; // Random offset for current target

        // Events
        public System.Action OnTargetReached;
        public System.Action OnMovementStarted;
        public System.Action OnMovementStopped;

        [Inject]
        public void Construct(ITimeService timeService)
        {
            this.timeService = timeService;
        }

        private void Awake()
        {
            // Auto-find CharacterMovement if not assigned
            if (characterMovement == null)
            {
                characterMovement = GetComponent<CharacterMovement>();
                if (characterMovement == null)
                {
                    characterMovement = GetComponentInParent<CharacterMovement>();
                }
            }

            // Auto-find TeleportClientController if not assigned
            if (teleportClientController == null)
            {
                teleportClientController = GetComponent<TeleportClientController>();
                if (teleportClientController == null)
                {
                    teleportClientController = GetComponentInParent<TeleportClientController>();
                }
            }
        }

        private void Start()
        {
            if (waypoints != null && waypoints.Length > 0)
            {
                StartMovement();
            }
        }

        private void Update()
        {
            if (isMoving && waypoints != null && waypoints.Length > 0)
            {
                if (isWaiting)
                {
                    // Use time multiplier to scale wait timer
                    float timeMultiplier = timeService?.TimeMultiplier ?? 60f;
                    float timeScale = timeMultiplier / 60f;
                    waitTimer -= Time.deltaTime * timeScale;
                    if (waitTimer <= 0f)
                    {
                        isWaiting = false;
                        // Remove the reached waypoint (index 0) and continue to new first waypoint
                        RemoveFirstWaypoint();
                        if (needTeleport)
                        {
                            isWaiting = false;
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
            if (characterMovement == null || waypoints == null || waypoints.Length == 0) return;
            // Always move to first waypoint (index 0)
            Transform currentWaypoint = waypoints[0];
            if (currentWaypoint == null) return;

            // Check if NPC and target are on the same level
            if (!IsPointOnTheSameLevel(currentWaypoint.position))
            {
                Debug.LogWarning($"[NpcController] Cannot move to waypoint - not on the same level. NPC Y: {transform.position.y}, Target Y: {currentWaypoint.position.y}");
                SearchForTeleport();
                return;
            }

            // Generate random offset for this target if not set
            if (currentTargetOffset == 0f)
            {
                currentTargetOffset = Random.Range(-thresholdX, thresholdX);
            }

            // Calculate target position with offset
            Vector3 targetPosition = new Vector3(currentWaypoint.position.x + currentTargetOffset, currentWaypoint.position.y, currentWaypoint.position.z);
            Vector3 direction = (targetPosition - transform.position);
            float distance = direction.magnitude;

            if (distance <= reachDistance)
            {
                Debug.Log($"Reached waypoint 0, distance={distance}, offset={currentTargetOffset}");

                characterMovement.SetHorizontalInput(0f);
                isWaiting = true;
                waitTimer = waitTimeAtWaypoint;
                OnTargetReached?.Invoke();
                return;
            }

            float horizontalInput = direction.x > 0 ? 1f : -1f;

            characterMovement.SetHorizontalInput(horizontalInput);

            if (lookAtTarget)
            {
                characterMovement.SetFacingDirection(horizontalInput);
            }
        }

        /// <summary>
        /// Check if target point is on the same level as NPC (Y coordinate)
        /// </summary>
        private bool IsPointOnTheSameLevel(Vector3 targetPoint)
        {
            return Mathf.Abs(transform.position.y - targetPoint.y) <= searchVerticalThreshold;
        }

        /// <summary>
        /// Search for teleport to reach target level
        /// </summary>
        private void SearchForTeleport()
        {
            needTeleport = true;

            // If already need teleport, check if first waypoint is already a teleport
            if (waypoints != null && waypoints.Length > 0)
            {
                var firstWaypoint = waypoints[0];
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
            if (teleportClientController == null)
            {
                Debug.LogWarning("[NpcController] TeleportClientController not found!");
                return;
            }

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning("[NpcController] No waypoints to compare!");
                return;
            }

            // Get target waypoint (first in list)
            Transform targetWaypoint = waypoints[0];
            if (targetWaypoint == null)
            {
                Debug.LogWarning("[NpcController] Target waypoint is null!");
                return;
            }

            // Teleport to target level
            Debug.Log($"[NpcController] Teleporting to target level");
            teleportClientController.TryTeleportToTarget(targetWaypoint, searchVerticalThreshold);

            needTeleport = false;
        }

        /// <summary>
        /// Insert transform as first waypoint
        /// </summary>
        private void InsertAsFirstWaypoint(Transform transformToInsert)
        {
            if (waypoints == null)
            {
                waypoints = new Transform[] { transformToInsert };
            }
            else
            {
                // Create new array with transform as first element
                Transform[] newWaypoints = new Transform[waypoints.Length + 1];
                newWaypoints[0] = transformToInsert;

                // Copy existing waypoints
                for (int i = 0; i < waypoints.Length; i++)
                {
                    newWaypoints[i + 1] = waypoints[i];
                }

                waypoints = newWaypoints;
            }

            Debug.Log($"[NpcController] Inserted as first waypoint: {transformToInsert.name}");
        }

        /// <summary>
        /// Remove the first waypoint from the list and continue movement
        /// </summary>
        private void RemoveFirstWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            // Reset offset for next target
            currentTargetOffset = 0f;

            // Create new array without the first element
            Transform[] newWaypoints = new Transform[waypoints.Length - 1];
            for (int i = 1; i < waypoints.Length; i++)
            {
                newWaypoints[i - 1] = waypoints[i];
            }

            waypoints = newWaypoints;

            // If no waypoints left, stop movement
            if (waypoints.Length == 0)
            {
                StopMovement();
                return;
            }

            // Continue moving to new first waypoint
            Debug.Log($"Removed first waypoint, {waypoints.Length} waypoints remaining");
        }

        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
        }

        public void SetWaitTime(float time)
        {
            waitTimeAtWaypoint = time;
        }

        /// <summary>
        /// Start moving to current target
        /// </summary>
        public void StartMovement()
        {
            Debug.Log($"StartMovement: waypoints={waypoints != null && waypoints.Length > 0}, characterMovement={characterMovement != null}");

            if (waypoints == null || waypoints.Length == 0)
            {
                Debug.LogWarning("[NpcController] No waypoints set!");
                return;
            }

            if (characterMovement == null)
            {
                Debug.LogWarning("[NpcController] No CharacterMovement found!");
                return;
            }

            isMoving = true;
            isWaiting = false;
            OnMovementStarted?.Invoke();

            Debug.Log($"Movement started: isMoving={isMoving}");
        }

        /// <summary>
        /// Stop movement
        /// </summary>
        public void StopMovement()
        {
            isMoving = false;

            if (characterMovement != null)
            {
                characterMovement.SetHorizontalInput(0f);
            }

            OnMovementStopped?.Invoke();
        }


        /// <summary>
        /// Set reach distance
        /// </summary>
        public void SetReachDistance(float distance)
        {
            reachDistance = distance;
        }

        /// <summary>
        /// Set movement speed
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
            if (characterMovement != null)
            {
                characterMovement.SetMoveSpeed(speed);
            }
        }

        /// <summary>
        /// Check if character is moving
        /// </summary>
        public bool IsMoving()
        {
            return isMoving;
        }

        public int GetCurrentWaypointIndex()
        {
            return 0; // Always return 0 since we always target the first waypoint
        }

        public Transform GetCurrentWaypoint()
        {
            if (waypoints == null || waypoints.Length == 0) return null;
            return waypoints[0]; // Always return first waypoint
        }

    }
}
