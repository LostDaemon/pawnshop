using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PawnShop.Models.Characters;
using PawnShop.Models;
using PawnShop.Services;
using System.Linq;

namespace PawnShop.Controllers
{
    [System.Serializable]
    public class CustomerFaceData
    {
        public float PatienceThreshold;
        public Sprite FaceSprite;
        public Color IndicatorColor;
    }

    public class CustomerPatienceController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _faceRenderer;
        [SerializeField] private CustomerFaceData[] _faceData;
        [SerializeField] private Image _impatienceIndicator;

        private ICustomerService _customerService;

        [Inject]
        public void Construct(ICustomerService customerService)
        {
            _customerService = customerService;

            _customerService.OnCustomerChanged += OnCustomerChanged;
            _customerService.OnPatienceChanged += OnPatienceChanged;
        }

        private void OnDestroy()
        {
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
                _customerService.OnPatienceChanged -= OnPatienceChanged;
            }
        }

        private void OnCustomerChanged(Customer customer)
        {
            if (customer == null)
            {
                // Hide face when no customer
                if (_faceRenderer != null)
                    _faceRenderer.sprite = null;
                
                // Reset impatience indicator
                UpdateImpatienceIndicator(0f);
            }
            else
            {
                // Show appropriate face for customer patience
                UpdateCustomerFace(customer.Patience);
                UpdateImpatienceIndicator(customer.Patience);
            }
        }

        private void OnPatienceChanged(float patience)
        {
            // Update face when patience changes
            UpdateCustomerFace(patience);
            UpdateImpatienceIndicator(patience);
        }

        private void UpdateCustomerFace(float patience)
        {
            if (_faceRenderer == null || _faceData == null || _faceData.Length == 0)
                return;

            // Sort face data by threshold descending (highest first)
            var sortedFaceData = _faceData.OrderByDescending(x => x.PatienceThreshold).ToArray();

            // Find the appropriate face sprite for current patience
            Sprite faceSprite = null;
            foreach (var faceData in sortedFaceData)
            {
                if (patience >= faceData.PatienceThreshold)
                {
                    faceSprite = faceData.FaceSprite;
                    break;
                }
            }

            // If no sprite found, use the lowest threshold sprite
            if (faceSprite == null && sortedFaceData.Length > 0)
            {
                faceSprite = sortedFaceData[sortedFaceData.Length - 1].FaceSprite;
            }

            _faceRenderer.sprite = faceSprite;
        }

        private void UpdateImpatienceIndicator(float patience)
        {
            if (_impatienceIndicator == null)
                return;

            // Calculate impatience as percentage: 100 - patience
            float impatience = 100f - patience;
            float fillAmount = impatience / 100f; // Convert to 0-1 range for Image.fillAmount
            
            _impatienceIndicator.fillAmount = fillAmount;
            
            // Update indicator color based on patience level
            UpdateIndicatorColor(patience);
        }

        private void UpdateIndicatorColor(float patience)
        {
            if (_impatienceIndicator == null || _faceData == null || _faceData.Length == 0)
                return;

            // Sort face data by threshold ascending (lowest first) for impatience logic
            var sortedFaceData = _faceData.OrderBy(x => x.PatienceThreshold).ToArray();

            // Find the appropriate color for current impatience level
            // Lower patience = higher impatience = more red color
            Color indicatorColor = Color.white; // Default color
            foreach (var faceData in sortedFaceData)
            {
                if (patience <= faceData.PatienceThreshold)
                {
                    indicatorColor = faceData.IndicatorColor;
                    break;
                }
            }

            // If no color found, use the highest threshold color
            if (indicatorColor == Color.white && sortedFaceData.Length > 0)
            {
                indicatorColor = sortedFaceData[sortedFaceData.Length - 1].IndicatorColor;
            }

            _impatienceIndicator.color = indicatorColor;
        }
    }
}
