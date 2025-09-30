using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PawnShop.Controllers.Cards;
using PawnShop.Controllers.DragNDrop;
using PawnShop.Models.Tags;
using PawnShop.Services;
using Zenject;
using System;

namespace PawnShop.Controllers.Cards
{
    public class NegotiationRoundController : MonoBehaviour
    {
        [SerializeField] private CardSlotController _playerCardSlot;
        [SerializeField] private CardSlotController _customerCardSlot;
        [SerializeField] private Text _multiplierIndicator;
        
        private ILocalizationService _localizationService;
        
        public CardSlotController PlayerCardSlot => _playerCardSlot;
        public CardSlotController CustomerCardSlot => _customerCardSlot;
        
        public float EffectMultiplier { get; private set; }
        
        // Event for multiplier changes
        public event Action<float> OnMultiplierChanged;
        
        [Inject]
        public void Construct(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }
        
        private void OnEnable()
        {
            SubscribeToSlotEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromSlotEvents();
        }
        
        private void SubscribeToSlotEvents()
        {
            if (_playerCardSlot != null)
            {
                _playerCardSlot.OnItemDroppedEvent += OnPlayerCardDropped;
            }
            
            if (_customerCardSlot != null)
            {
                _customerCardSlot.OnItemDroppedEvent += OnCustomerCardDropped;
            }
        }
        
        private void UnsubscribeFromSlotEvents()
        {
            if (_playerCardSlot != null)
            {
                _playerCardSlot.OnItemDroppedEvent -= OnPlayerCardDropped;
            }
            
            if (_customerCardSlot != null)
            {
                _customerCardSlot.OnItemDroppedEvent -= OnCustomerCardDropped;
            }
        }
        
        private void OnPlayerCardDropped(DraggableItemController<BaseTagModel> draggableItem, PointerEventData eventData)
        {
            // Get the other card from customer slot
            var customerCard = _customerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(draggableItem.Payload, customerCard?.Payload);
        }
        
        private void OnCustomerCardDropped(DraggableItemController<BaseTagModel> draggableItem, PointerEventData eventData)
        {
            // Get the other card from player slot
            var playerCard = _playerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(playerCard?.Payload, draggableItem.Payload);
        }
        
        private float CalculateEffect(BaseTagModel playerTag = null, BaseTagModel customerTag = null)
        {
            float totalMultiplier = 0f; // Start with 0 (no effect)

            // Check player card
            if (playerTag != null)
            {
                float playerEffect = (playerTag.PriceMultiplier - 1f);
                totalMultiplier += playerEffect;
            }
            
            // Check customer card
            if (customerTag != null)
            {
                float customerEffect = (customerTag.PriceMultiplier - 1f);
                totalMultiplier += customerEffect;
            }
            
            // EffectMultiplier is just the total effect (no need to add 1)
            EffectMultiplier = totalMultiplier;
            
            // Update multiplier indicator text with localization
            if (_multiplierIndicator != null)
            {
                var effectText = _localizationService?.GetLocalization("negotiation_round_effect") ?? "Round Effect {0}%";
                var difference = (totalMultiplier * 100).ToString("+0;-0;0");
                _multiplierIndicator.text = string.Format(effectText, difference);
                
                // Set color based on multiplier value
                _multiplierIndicator.color = totalMultiplier > 0f ? new Color(0.2f, 0.8f, 0.2f) : Color.red;
            }
            
            // Invoke multiplier changed event
            OnMultiplierChanged?.Invoke(EffectMultiplier);
            
            return EffectMultiplier;
        }
        
    }
}
