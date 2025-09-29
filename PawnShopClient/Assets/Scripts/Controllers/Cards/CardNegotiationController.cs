using UnityEngine;
using PawnShop.Services;
using Zenject;
using PawnShop.Models.Tags;
using PawnShop.Models.Characters;
using System.Collections.Generic;

namespace PawnShop.Controllers.Cards
{
    public class CardNegotiationController : MonoBehaviour
    {
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _cardSlotPrefab;
        [SerializeField] private Transform _playerCardContainer;
        
        private ICardNegotiationService _cardNegotiationService;
        private ICustomerService _customerService;
        private DiContainer _container;

        [Inject]
        public void Construct(ICardNegotiationService cardNegotiationService, ICustomerService customerService, DiContainer container)
        {
            _cardNegotiationService = cardNegotiationService;
            _customerService = customerService;
            _container = container;
            
            // Subscribe to customer changes
            _customerService.OnCustomerChanged += OnCustomerChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (_customerService != null)
            {
                _customerService.OnCustomerChanged -= OnCustomerChanged;
            }
        }

        private void OnCustomerChanged(Customer customer)
        {
            // Clear existing cards
            ClearExistingCards();
            
            // TODO: Implement card negotiation UI logic
            
            // Iterate through customer's item tags and create cards
            if (customer?.OwnedItem?.Tags != null)
            {
                foreach (var tag in customer.OwnedItem.Tags)
                {
                    CreateCardForTag(tag);
                }
            }
        }
        
        private void ClearExistingCards()
        {
            // Clear existing card slots (which contain cards) from player card container
            if (_playerCardContainer != null)
            {
                for (int i = _playerCardContainer.childCount - 1; i >= 0; i--)
                {
                    var child = _playerCardContainer.GetChild(i);
                    if (child != null)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        
        private void CreateCardForTag(BaseTagModel tagModel)
        {
            if (_cardPrefab == null || _cardSlotPrefab == null || _playerCardContainer == null) return;
            
            // First instantiate card slot under player card container
            var cardSlotInstance = _container.InstantiatePrefab(_cardSlotPrefab, _playerCardContainer);
            
            // Then instantiate card as child of the card slot
            var cardInstance = _container.InstantiatePrefab(_cardPrefab, cardSlotInstance.transform);
            
            // Get CardController and initialize with tag
            var cardController = cardInstance.GetComponent<CardController>();
            if (cardController != null)
            {
                cardController.Init(tagModel);
            }
        }
    }
}
