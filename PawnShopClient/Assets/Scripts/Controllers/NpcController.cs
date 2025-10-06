using UnityEngine;

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
        [SerializeField] private bool stopWhenReached = true;
        [SerializeField] private bool lookAtTarget = true;
        
        [Header("References")]
        [SerializeField] private CharacterMovement characterMovement;
        
        // State
        private bool isMoving = false;
        private int currentWaypointIndex = 0;
        private bool isWaiting = false;
        private float waitTimer = 0f;
        
        // Events
        public System.Action OnTargetReached;
        public System.Action OnMovementStarted;
        public System.Action OnMovementStopped;
        
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
                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0f)
                    {
                        isWaiting = false;
                        currentWaypointIndex++;
                        
                        if (currentWaypointIndex >= waypoints.Length)
                        {
                            if (stopWhenReached)
                            {
                                StopMovement();
                            }
                            else
                            {
                                currentWaypointIndex = 0;
                            }
                        }
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
            if (characterMovement == null || waypoints == null || currentWaypointIndex >= waypoints.Length) return;
            
            Transform currentWaypoint = waypoints[currentWaypointIndex];
            if (currentWaypoint == null) return;
            
            Vector3 direction = (currentWaypoint.position - transform.position);
            float distance = direction.magnitude;
            
            if (distance <= reachDistance)
            {
                Debug.Log($"Reached waypoint {currentWaypointIndex}, distance={distance}");
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
        
        public void SetWaypoints(Transform[] newWaypoints)
        {
            waypoints = newWaypoints;
            currentWaypointIndex = 0;
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
            currentWaypointIndex = 0;
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
            return currentWaypointIndex;
        }
        
        public Transform GetCurrentWaypoint()
        {
            if (waypoints == null || currentWaypointIndex >= waypoints.Length) return null;
            return waypoints[currentWaypointIndex];
        }
        
    }
}
