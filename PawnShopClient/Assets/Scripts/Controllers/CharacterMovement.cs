using UnityEngine;
using System;

namespace PawnShop.Controllers
{
    /// <summary>
    /// Character Movement Controller with physics-based movement
    /// Supports horizontal movement and sprite flipping
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 4f;
        
        // Components
        private Rigidbody2D rb;
        private Animator animator;
        
        // Movement state
        private float horizontalInput;
        private bool isFacingRight = false;
        
        // Events
        public System.Action<float> OnHorizontalMovement;
        
        private void Awake()
        {
            // Get required components
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }
        
        private void Start()
        {
            // Set up rigidbody for 2D platformer
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            
            // Initialize facing direction based on current sprite scale
            isFacingRight = transform.localScale.x > 0;
        }
        
        private void Update()
        {
            HandleAnimations();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        /// <summary>
        /// Handle horizontal movement with physics
        /// </summary>
        private void HandleMovement()
        {
            // Apply horizontal movement (preserve Y velocity)
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            
            // Flip sprite based on movement direction
            FlipSprite();
            
            // Invoke movement event
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                OnHorizontalMovement?.Invoke(horizontalInput);
            }
        }
        
        /// <summary>
        /// Flip sprite based on movement direction
        /// </summary>
        private void FlipSprite()
        {
            if (horizontalInput > 0.1f && !isFacingRight)
            {
                // Moving right, should face right
                isFacingRight = true;
                Vector3 ls = transform.localScale;
                ls.x = Mathf.Abs(ls.x);
                transform.localScale = ls;
            }
            else if (horizontalInput < -0.1f && isFacingRight)
            {
                // Moving left, should face left
                isFacingRight = false;
                Vector3 ls = transform.localScale;
                ls.x = -Mathf.Abs(ls.x);
                transform.localScale = ls;
            }
        }
        
        
        /// <summary>
        /// Handle animation parameters
        /// </summary>
        private void HandleAnimations()
        {
            if (animator == null) return;
            
            // Set animation parameters
            animator.SetFloat("xVelocity", Math.Abs(rb.linearVelocity.x));
        }
        
        /// <summary>
        /// Set horizontal movement input
        /// </summary>
        public void SetHorizontalInput(float input)
        {
            horizontalInput = Mathf.Clamp(input, -1f, 1f);
        }
        
        /// <summary>
        /// Move character left
        /// </summary>
        public void MoveLeft()
        {
            SetHorizontalInput(-1f);
        }
        
        /// <summary>
        /// Move character right
        /// </summary>
        public void MoveRight()
        {
            SetHorizontalInput(1f);
        }
        
        /// <summary>
        /// Stop horizontal movement
        /// </summary>
        public void StopMovement()
        {
            SetHorizontalInput(0f);
        }
        
        
        /// <summary>
        /// Set movement speed
        /// </summary>
        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
        }
        
        
        /// <summary>
        /// Get current horizontal input
        /// </summary>
        public float GetHorizontalInput()
        {
            return horizontalInput;
        }
        
        
        /// <summary>
        /// Get facing direction
        /// </summary>
        public bool IsFacingRight()
        {
            return isFacingRight;
        }
        
        /// <summary>
        /// Force set facing direction
        /// </summary>
        public void SetFacingRight(bool facingRight)
        {
            if (isFacingRight != facingRight)
            {
                isFacingRight = facingRight;
                Vector3 ls = transform.localScale;
                ls.x = Mathf.Abs(ls.x) * (facingRight ? 1f : -1f);
                transform.localScale = ls;
            }
        }
        
        /// <summary>
        /// Force set facing direction by input
        /// </summary>
        public void SetFacingDirection(float direction)
        {
            if (direction > 0.1f)
            {
                SetFacingRight(true);
            }
            else if (direction < -0.1f)
            {
                SetFacingRight(false);
            }
        }
        
    }
}
