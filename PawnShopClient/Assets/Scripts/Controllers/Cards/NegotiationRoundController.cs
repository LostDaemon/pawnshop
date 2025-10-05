using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PawnShop.Controllers.Cards;
using PawnShop.Controllers.DragNDrop;
using PawnShop.Models.Tags;
using PawnShop.Models;
using PawnShop.Services;
using Zenject;
using System;
using System.Collections;

namespace PawnShop.Controllers.Cards
{
    public class NegotiationRoundController : MonoBehaviour
    {
        [SerializeField] private CardSlotController _playerCardSlot;
        [SerializeField] private CardSlotController _customerCardSlot;
        [SerializeField] private Text _multiplierIndicator;

        private ILocalizationService _localizationService;
        private ICardNegotiationService _cardNegotiationService;

        public CardSlotController PlayerCardSlot => _playerCardSlot;
        public CardSlotController CustomerCardSlot => _customerCardSlot;

        public float EffectMultiplier { get; private set; }
        public int RoundNumber { get; private set; }

        // Event for multiplier changes
        public event Action<float> OnMultiplierChanged;

        [Inject]
        public void Construct(ILocalizationService localizationService, ICardNegotiationService cardNegotiationService)
        {
            _localizationService = localizationService;
            _cardNegotiationService = cardNegotiationService;
        }

        public void InitializeRound(int roundNumber)
        {
            RoundNumber = roundNumber;
            UpdateSlotActivity();
        }

        private void UpdateSlotActivity()
        {
            if (_playerCardSlot != null)
            {
                _playerCardSlot.canReceiveDragged = (_cardNegotiationService.CurrentRound == RoundNumber);
            }
        }

        public void SetPlayerCard(CardController cardController)
        {
            if (cardController == null || _playerCardSlot == null) return;
            Debug.Log($"[NegotiationRoundController] SetPlayerCard: {cardController?.Payload?.DisplayName}");

            // Clear existing card in player slot
            ClearSlot(_playerCardSlot);

            // Set the new card as child of player slot
            cardController.transform.SetParent(_playerCardSlot.transform);
            cardController.transform.localPosition = Vector3.zero;
            CalculateEffect(cardController?.Payload, null);
        }

        public void SetCustomerCard(CardController cardController)
        {
            if (cardController == null || _customerCardSlot == null) return;
            Debug.Log($"[NegotiationRoundController] SetCustomerCard: {cardController?.Payload?.DisplayName}");

            // Clear existing card in customer slot
            ClearSlot(_customerCardSlot);

            // Set the new card as child of customer slot
            cardController.transform.SetParent(_customerCardSlot.transform);
            cardController.transform.localPosition = Vector3.zero;
            var playerCard = _playerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(playerCard?.Payload, cardController?.Payload);
        }

        private void ClearSlot(CardSlotController slot)
        {
            if (slot == null) return;

            // Remove all existing cards from the slot
            foreach (Transform child in slot.transform)
            {
                if (child.GetComponent<CardController>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void OnEnable()
        {
            SubscribeToSlotEvents();
            SubscribeToServiceEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromSlotEvents();
            UnsubscribeFromServiceEvents();
        }

        private void SubscribeToSlotEvents()
        {
            if (_playerCardSlot != null)
            {
                _playerCardSlot.OnItemDroppedEvent += OnPlayerCardDropped;
                _playerCardSlot.OnItemStartDragEvent += OnPlayerCardStartDrag;
            }

            if (_customerCardSlot != null)
            {
                _customerCardSlot.OnItemDroppedEvent += OnCustomerCardDropped;
                _customerCardSlot.OnItemStartDragEvent += OnCustomerCardStartDrag;
            }
        }

        private void UnsubscribeFromSlotEvents()
        {
            if (_playerCardSlot != null)
            {
                _playerCardSlot.OnItemDroppedEvent -= OnPlayerCardDropped;
                _playerCardSlot.OnItemStartDragEvent -= OnPlayerCardStartDrag;
            }

            if (_customerCardSlot != null)
            {
                _customerCardSlot.OnItemDroppedEvent -= OnCustomerCardDropped;
                _customerCardSlot.OnItemStartDragEvent -= OnCustomerCardStartDrag;
            }
        }

        private void UnsubscribeFromServiceEvents()
        {
            if (_cardNegotiationService != null)
            {
                _cardNegotiationService.OnRoundChanged -= OnRoundChanged;
            }
        }

        private void SubscribeToServiceEvents()
        {
            if (_cardNegotiationService != null)
            {
                _cardNegotiationService.OnRoundChanged += OnRoundChanged;
            }
        }

        private void OnRoundChanged(int roundNumber)
        {
            UpdateSlotActivity();
        }


        private void OnPlayerCardDropped(DraggableItemController<BaseTagModel> draggableItem)
        {
            if (draggableItem?.Payload == null)
            {
                return;
            }

            // Get the other card from customer slot
            var customerCard = _customerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(draggableItem.Payload, customerCard?.Payload);
            _cardNegotiationService.PlayerPlay(draggableItem.Payload);
        }

        private void OnCustomerCardDropped(DraggableItemController<BaseTagModel> draggableItem)
        {
            // Get the other card from player slot
            var playerCard = _playerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(playerCard?.Payload, draggableItem.Payload);
        }

        private void OnPlayerCardStartDrag(DraggableItemController<BaseTagModel> draggableItem)
        {
            // Get the other card from customer slot
            var customerCard = _customerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(null, customerCard?.Payload);
        }

        private void OnCustomerCardStartDrag(DraggableItemController<BaseTagModel> draggableItem)
        {
            // Get the other card from player slot
            var playerCard = _playerCardSlot?.GetComponentInChildren<CardController>();
            CalculateEffect(playerCard?.Payload, null);
        }

        private float CalculateEffect(BaseTagModel playerTag = null, BaseTagModel customerTag = null)
        {
            float totalMultiplier = 0f; // Start with 0 (no effect)
            Debug.Log($"[NegotiationRoundController] CalculateEffect");
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
