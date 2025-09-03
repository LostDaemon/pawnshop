using UnityEngine;
using UnityEngine.UI;

namespace PawnShop.Controllers
{
    public class SwitchVisibilityController : MonoBehaviour
    {
        [Header("First Group")]
        [SerializeField] private VisibilityController[] firstGroup;
        
        [Header("Second Group")]
        [SerializeField] private VisibilityController[] secondGroup;
        
        [Header("Switch Button")]
        [SerializeField] private Button switchButton;
        
        private bool isFirstGroupVisible = true;
        
        private void Start()
        {
            if (switchButton != null)
            {
                switchButton.onClick.AddListener(OnSwitchButtonClicked);
            }
            
            // Set initial state
            UpdateVisibility();
        }
        
        private void OnDestroy()
        {
            if (switchButton != null)
            {
                switchButton.onClick.RemoveListener(OnSwitchButtonClicked);
            }
        }
        
        private void OnSwitchButtonClicked()
        {
            isFirstGroupVisible = !isFirstGroupVisible;
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
            // Update first group
            foreach (var controller in firstGroup)
            {
                if (controller != null)
                {
                    controller.SetVisibility(isFirstGroupVisible);
                }
            }
            
            // Update second group
            foreach (var controller in secondGroup)
            {
                if (controller != null)
                {
                    controller.SetVisibility(!isFirstGroupVisible);
                }
            }
        }
        
        // Public method to manually set state
        public void SetFirstGroupVisible(bool visible)
        {
            isFirstGroupVisible = visible;
            UpdateVisibility();
        }
        
        // Public method to get current state
        public bool IsFirstGroupVisible => isFirstGroupVisible;
    }
}
