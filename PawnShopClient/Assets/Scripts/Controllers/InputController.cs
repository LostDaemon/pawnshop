using UnityEngine;

namespace PawnShop.Controllers
{
    /// <summary>
    /// Input Controller for handling keyboard and gamepad input
    /// Manages CharacterMovement based on user input
    /// </summary>
    public class InputController : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private bool useGamepad = true;
        [SerializeField] private string horizontalAxis = "Horizontal";
        [SerializeField] private KeyCode runKey = KeyCode.LeftShift;
        
        [Header("References")]
        [SerializeField] private CharacterMovement characterMovement;
        
        // Input state
        private float horizontalInput;
        private bool runInput;
        
        // Events
        public System.Action<float> OnHorizontalInput;
        public System.Action<bool> OnRunInput;
        
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
        
        private void Update()
        {
            HandleInput();
            ApplyInputToMovement();
        }
        
        /// <summary>
        /// Handle input from keyboard and gamepad
        /// </summary>
        private void HandleInput()
        {
            // Get horizontal input
            horizontalInput = Input.GetAxis(horizontalAxis);
            
            // Handle run input
            runInput = Input.GetKey(runKey);
            
            // Keyboard fallback if no gamepad input
            if (useGamepad && Mathf.Abs(horizontalInput) < 0.1f)
            {
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                    horizontalInput = -1f;
                else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                    horizontalInput = 1f;
            }
            
            // Invoke input events
            OnHorizontalInput?.Invoke(horizontalInput);
            OnRunInput?.Invoke(runInput);
        }
        
        /// <summary>
        /// Apply input to CharacterMovement
        /// </summary>
        private void ApplyInputToMovement()
        {
            if (characterMovement == null) return;
            
            // Apply horizontal input to movement
            characterMovement.SetHorizontalInput(horizontalInput);
        }
        
        /// <summary>
        /// Set CharacterMovement reference
        /// </summary>
        public void SetCharacterMovement(CharacterMovement movement)
        {
            characterMovement = movement;
        }
        
        /// <summary>
        /// Enable/disable gamepad input
        /// </summary>
        public void SetGamepadEnabled(bool enabled)
        {
            useGamepad = enabled;
        }
        
        /// <summary>
        /// Get current horizontal input
        /// </summary>
        public float GetHorizontalInput()
        {
            return horizontalInput;
        }
        
        /// <summary>
        /// Get current run input
        /// </summary>
        public bool GetRunInput()
        {
            return runInput;
        }
        
        /// <summary>
        /// Check if any movement input is active
        /// </summary>
        public bool IsMovementInputActive()
        {
            return Mathf.Abs(horizontalInput) > 0.1f;
        }
        
        /// <summary>
        /// Disable input processing
        /// </summary>
        public void DisableInput()
        {
            horizontalInput = 0f;
            runInput = false;
            if (characterMovement != null)
            {
                characterMovement.SetHorizontalInput(0f);
            }
        }
        
        /// <summary>
        /// Enable input processing
        /// </summary>
        public void EnableInput()
        {
            // Input will be processed normally in Update
        }
    }
}
