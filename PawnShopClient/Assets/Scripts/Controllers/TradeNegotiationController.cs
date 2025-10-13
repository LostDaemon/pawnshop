using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using PawnShop.Services;
using PawnShop.Models;
using PawnShop.Controllers.Cards;
using PawnShop.Models.Tags;
using System.Collections.Generic;
using System.Linq;
using PawnShop.Controllers.DragNDrop;

namespace PawnShop.Controllers
{
    public class TradeNegotiationController : MonoBehaviour
    {
        private const int MaxCardsOnHand = 5;

        [SerializeField] private Transform _playerDeckContainer;
        [SerializeField] private Transform _customerDeckContainer;
        [SerializeField] private Transform _playerRoundDeck;
        [SerializeField] private Transform _customerRoundDeck;
        [SerializeField] private GameObject _cardPrefab;
        [SerializeField] private GameObject _cardSlotPrefab;
        [SerializeField] private Image _itemImage;
        [SerializeField] private TextMeshProUGUI _itemTitle;
        [SerializeField] private TextMeshProUGUI _itemDescription;
        [SerializeField] private TextMeshProUGUI _negotiatedPrice;

        private ICustomerService _customerService;
        private ITagService _tagService;
        private ITagFactory _tagFactory;
        private ItemModel _currentItem;

        private List<CardController> _playerCardControllers = new List<CardController>();
        private List<CardController> _customerCardControllers = new List<CardController>();
        private List<CardSlotController> _playerRoundSlots = new List<CardSlotController>();
        private List<CardSlotController> _customerRoundSlots = new List<CardSlotController>();
        private DiContainer _container;

        [Inject]
        public void Construct(ICustomerService customerService, ITagService tagService, ITagFactory tagFactory, DiContainer container)
        {
            _customerService = customerService;
            _tagService = tagService;
            _tagFactory = tagFactory;
            _container = container;
        }

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _currentItem = _customerService.CurrentCustomer?.OwnedItem;
            _currentItem.CurrentOffer = _currentItem.BasePrice;

            UpdateItemImage();
            UpdateItemTitle();
            UpdateItemDescription();
            UpdateNegotiatedPrice();
            PlayerTakeCards();
            CustomerTakeCards();
            InitializePlayerRoundSlots();
            InitializeCustomerRoundSlots();
        }

        private void UpdateItemImage()
        {
            if (_itemImage != null && _currentItem != null)
            {
                _itemImage.sprite = _currentItem.Image;
            }
        }

        private void UpdateItemTitle()
        {
            if (_itemTitle != null && _currentItem != null)
            {
                _itemTitle.text = _currentItem.Name;
            }
        }

        private void UpdateItemDescription()
        {
            if (_itemDescription != null && _currentItem != null)
            {
                _itemDescription.text = _currentItem.Description;
            }
        }

        private void UpdateNegotiatedPrice()
        {
            if (_negotiatedPrice != null && _currentItem != null)
            {
                _negotiatedPrice.text = _currentItem.CurrentOffer.ToString("F0");
            }
        }

        public void RecalculateOffer()
        {
            if (_currentItem == null) return;

            float basePrice = _currentItem.BasePrice;
            float totalMultiplier = 1f;
            
            Debug.Log($"[RecalculateOffer] Starting calculation - Base Price: {basePrice}");
            
            // Calculate multiplier from player round slots
            Debug.Log($"[RecalculateOffer] Processing {_playerRoundSlots.Count} player round slots");
            foreach (var slot in _playerRoundSlots)
            {
                var cardController = slot.GetComponentInChildren<CardController>();
                if (cardController?.Model != null)
                {
                    float multiplier = cardController.Model.PriceMultiplier;
                    totalMultiplier *= multiplier;
                    Debug.Log($"[RecalculateOffer] Player card multiplier: {multiplier}, Total multiplier now: {totalMultiplier}");
                }
            }

            // Calculate multiplier from customer round slots
            Debug.Log($"[RecalculateOffer] Processing {_customerRoundSlots.Count} customer round slots");
            foreach (var slot in _customerRoundSlots)
            {
                var cardController = slot.GetComponentInChildren<CardController>();
                if (cardController?.Model != null)
                {
                    float multiplier = cardController.Model.PriceMultiplier;
                    totalMultiplier *= multiplier;
                    Debug.Log($"[RecalculateOffer] Customer card multiplier: {multiplier}, Total multiplier now: {totalMultiplier}");
                }
            }

            // Apply multiplier to base price and round down
            float newOffer = Mathf.Floor(basePrice * totalMultiplier);
            Debug.Log($"[RecalculateOffer] Final calculation - Base: {basePrice} × Total Multiplier: {totalMultiplier} = {basePrice * totalMultiplier}, Floored: {newOffer}");
            _currentItem.CurrentOffer = (long)newOffer;
            Debug.Log($"[RecalculateOffer] Final offer set to: {_currentItem.CurrentOffer}");
        }

        public void PlayerTakeCards()
        {
            // Calculate how many cards to add to reach MaxCardsOnHand
            int cardsToAdd = MaxCardsOnHand - _playerCardControllers.Count;

            // Get all negative tags
            var negativeTags = _tagService.GetNegativeTags().ToList();

            for (int i = 0; i < cardsToAdd; i++)
            {
                // Instantiate card slot
                var slotInstance = _container.InstantiatePrefab(_cardSlotPrefab, _playerDeckContainer);

                // Instantiate card
                var cardInstance = _container.InstantiatePrefab(_cardPrefab, slotInstance.transform);

                // Get card controller and initialize with random negative tag
                var cardController = cardInstance.GetComponent<CardController>();
                if (cardController != null && negativeTags.Count > 0)
                {
                    // Get random negative tag
                    var randomTag = negativeTags[Random.Range(0, negativeTags.Count)];
                    var tagModel = _tagFactory.Create(randomTag);

                    // Initialize card with tag
                    cardController.Init(tagModel);
                    _playerCardControllers.Add(cardController);
                }
            }
        }

        public void CustomerTakeCards()
        {
            // Calculate how many cards to add to reach MaxCardsOnHand
            int cardsToAdd = MaxCardsOnHand - _customerCardControllers.Count;

            // Get all positive tags
            var positiveTags = _tagService.GetPositiveTags().ToList();

            for (int i = 0; i < cardsToAdd; i++)
            {
                // Instantiate card slot
                var slotInstance = _container.InstantiatePrefab(_cardSlotPrefab, _customerDeckContainer);
                // Instantiate card
                var cardInstance = _container.InstantiatePrefab(_cardPrefab, slotInstance.transform);
                // Get card controller and initialize with random positive tag
                var cardController = cardInstance.GetComponent<CardController>();

                if (cardController != null && positiveTags.Count > 0)
                {
                    // Get random positive tag
                    var randomTag = positiveTags[Random.Range(0, positiveTags.Count)];
                    var tagModel = _tagFactory.Create(randomTag);

                    // Initialize card with tag
                    cardController.Init(tagModel);
                    _customerCardControllers.Add(cardController);
                }
            }
        }

        private void InitializePlayerRoundSlots()
        {
            // Find all CardSlotController components in player round deck
            if (_playerRoundDeck != null)
            {
                _playerRoundSlots.Clear();
                var playerSlots = _playerRoundDeck.GetComponentsInChildren<CardSlotController>();
                _playerRoundSlots.AddRange(playerSlots);

                // Subscribe to OnItemDroppedEvent and OnItemStartDragEvent for each player slot
                foreach (var slot in _playerRoundSlots)
                {
                    slot.OnItemDroppedEvent += OnPlayerSlotItemDropped;
                    slot.OnItemStartDragEvent += OnPlayerSlotItemStartDrag;
                }
            }
        }

        private void InitializeCustomerRoundSlots()
        {
            // Find all CardSlotController components in customer round deck
            if (_customerRoundDeck != null)
            {
                _customerRoundSlots.Clear();
                var customerSlots = _customerRoundDeck.GetComponentsInChildren<CardSlotController>();
                _customerRoundSlots.AddRange(customerSlots);

                // Subscribe to OnItemDroppedEvent and OnItemStartDragEvent for each customer slot
                foreach (var slot in _customerRoundSlots)
                {
                    slot.OnItemDroppedEvent += OnCustomerSlotItemDropped;
                    slot.OnItemStartDragEvent += OnCustomerSlotItemStartDrag;
                }
            }
        }

        private void OnPlayerSlotItemDropped(DraggableItemController<BaseTagModel> draggableItem)
        {
            StartCoroutine(RecalculateAfterCardMoved());
        }

        private void OnCustomerSlotItemDropped(DraggableItemController<BaseTagModel> draggableItem)
        {
            StartCoroutine(RecalculateAfterCardMoved());
        }

        private System.Collections.IEnumerator RecalculateAfterCardMoved()
        {
            // Wait one frame for the card to be actually moved to its new parent
            yield return null;
            
            RecalculateOffer();
            UpdateNegotiatedPrice();
        }

        private void OnPlayerSlotItemStartDrag(DraggableItemController<BaseTagModel> draggableItem)
        {
            RecalculateOffer();
            UpdateNegotiatedPrice();
        }

        private void OnCustomerSlotItemStartDrag(DraggableItemController<BaseTagModel> draggableItem)
        {
            RecalculateOffer();
            UpdateNegotiatedPrice();
        }

        private void OnDestroy()
        {
            // Unsubscribe from all player slots
            foreach (var slot in _playerRoundSlots)
            {
                if (slot != null)
                {
                    slot.OnItemDroppedEvent -= OnPlayerSlotItemDropped;
                    slot.OnItemStartDragEvent -= OnPlayerSlotItemStartDrag;
                }
            }

            // Unsubscribe from all customer slots
            foreach (var slot in _customerRoundSlots)
            {
                if (slot != null)
                {
                    slot.OnItemDroppedEvent -= OnCustomerSlotItemDropped;
                    slot.OnItemStartDragEvent -= OnCustomerSlotItemStartDrag;
                }
            }
        }
    }
}