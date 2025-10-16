using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
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
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _rejectButton;
        [SerializeField] private Button _roundButton;

        private ICustomerService _customerService;
        private ITagService _tagService;
        private ITagFactory _tagFactory;
        private IWalletService _walletService;
        private ISlotStorageService<ItemModel> _inventoryStorage;
        private ItemModel _currentItem;

        private List<CardController> _playerCardControllers = new List<CardController>();
        private List<CardController> _customerCardControllers = new List<CardController>();
        private List<CardSlotController> _playerRoundSlots = new List<CardSlotController>();
        private List<CardSlotController> _customerRoundSlots = new List<CardSlotController>();
        private DiContainer _container;

        [Inject]
        public void Construct(ICustomerService customerService, ITagService tagService, ITagFactory tagFactory,
            IWalletService walletService,
            [Inject(Id = StorageType.InventoryStorage)] ISlotStorageService<ItemModel> inventoryStorage,
            DiContainer container)
        {
            _customerService = customerService;
            _tagService = tagService;
            _tagFactory = tagFactory;
            _walletService = walletService;
            _inventoryStorage = inventoryStorage;
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
            InitializeButtons();
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

            // Calculate multiplier from item tags
            foreach (var tag in _currentItem.Tags)
            {
                if (tag != null)
                {
                    totalMultiplier *= tag.PriceMultiplier;
                }
            }

            // Calculate multiplier from player round slots
            foreach (var slot in _playerRoundSlots)
            {
                var cardController = slot.GetComponentInChildren<CardController>();
                if (cardController?.Model != null)
                {
                    totalMultiplier *= cardController.Model.PriceMultiplier;
                }
            }

            // Calculate multiplier from customer round slots
            foreach (var slot in _customerRoundSlots)
            {
                var cardController = slot.GetComponentInChildren<CardController>();
                if (cardController?.Model != null)
                {
                    totalMultiplier *= cardController.Model.PriceMultiplier;
                }
            }

            // Apply multiplier to base price and round to nearest integer
            float calculatedPrice = basePrice * totalMultiplier;
            long newOffer = Mathf.RoundToInt(calculatedPrice);
            _currentItem.CurrentOffer = newOffer;

            Debug.Log($"[RecalculateOffer] Item tags: {_currentItem.Tags.Count}, Player cards: {_playerRoundSlots.Count(c => c.GetComponentInChildren<CardController>()?.Model != null)}, Customer cards: {_customerRoundSlots.Count(c => c.GetComponentInChildren<CardController>()?.Model != null)}, Total multiplier: {totalMultiplier:F2}, Price: {basePrice} → {newOffer}");
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

        private void InitializeButtons()
        {
            if (_acceptButton != null)
            {
                _acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            }

            if (_rejectButton != null)
            {
                _rejectButton.onClick.AddListener(OnRejectButtonClicked);
            }

            if (_roundButton != null)
            {
                _roundButton.onClick.AddListener(OnRoundButtonClicked);
            }
        }

        private void OnAcceptButtonClicked()
        {
            Debug.Log("[TradeNegotiationController] Accept button clicked - Trade accepted!");

            if (_currentItem == null)
            {
                Debug.LogWarning("[TradeNegotiationController] No current item to accept!");
                return;
            }

            // Try to deduct money for purchase (taken from NegotiationService logic)
            var success = _walletService.TransactionAttempt(CurrencyType.Money, -_currentItem.CurrentOffer);
            if (!success)
            {
                Debug.LogWarning("[TradeNegotiationController] Not enough money to buy the item!");
                return;
            }

            // Move item to inventory storage (taken from NegotiationService logic)
            if (_inventoryStorage.Put(_currentItem))
            {
                Debug.Log($"[TradeNegotiationController] Item {_currentItem.Name} purchased and moved to inventory for {_currentItem.CurrentOffer}!");

                // Send customer to city
                _customerService.SetCustomerAction(NpcAction.ReturnToZone);

                // Clear customer and unload scene
                _customerService.ClearCustomer();
                UnloadNegotiationScene();
            }
            else
            {
                Debug.LogWarning("[TradeNegotiationController] Failed to move item to inventory - inventory full!");
                // Refund money if inventory is full
                _walletService.TransactionAttempt(CurrencyType.Money, _currentItem.CurrentOffer);
            }
        }

        private void OnRejectButtonClicked()
        {
            Debug.Log("[TradeNegotiationController] Reject button clicked - Trade rejected!");

            // Send customer to city
            _customerService.SetCustomerAction(NpcAction.ReturnToZone);

            // Just clear customer and unload scene without making deal
            _customerService.ClearCustomer();
            UnloadNegotiationScene();
        }

        private void OnRoundButtonClicked()
        {
            Debug.Log("[TradeNegotiationController] Round button clicked - Starting new round!");
            StartCoroutine(ProcessRoundEnd());
        }

        private System.Collections.IEnumerator ProcessRoundEnd()
        {
            // Add tags from round cards to item and reveal them to player
            AddRoundCardsTagsToItem();

            // Clear current round cards first
            ClearRoundCards();

            // Wait one frame for cards to be destroyed
            yield return null;

            // Deal new cards
            PlayerTakeCards();
            CustomerTakeCards();

            // Recalculate price with new round cards (old cards are now part of item tags)
            RecalculateOffer();
            UpdateNegotiatedPrice();
        }

        private void AddRoundCardsTagsToItem()
        {
            if (_currentItem == null) return;

            // Add tags from player round cards
            foreach (var slot in _playerRoundSlots)
            {
                if (slot != null)
                {
                    var cardController = slot.GetComponentInChildren<CardController>();
                    if (cardController?.Model != null)
                    {
                        // Add tag to item and reveal it to player
                        var tagModel = cardController.Model;
                        _currentItem.Tags.Add(tagModel);
                        tagModel.IsRevealedToPlayer = true;
                        Debug.Log($"[TradeNegotiationController] Added player tag {tagModel.DisplayName} to item");
                    }
                }
            }

            // Add tags from customer round cards
            foreach (var slot in _customerRoundSlots)
            {
                if (slot != null)
                {
                    var cardController = slot.GetComponentInChildren<CardController>();
                    if (cardController?.Model != null)
                    {
                        // Add tag to item and reveal it to player
                        var tagModel = cardController.Model;
                        _currentItem.Tags.Add(tagModel);
                        tagModel.IsRevealedToPlayer = true;
                        Debug.Log($"[TradeNegotiationController] Added customer tag {tagModel.DisplayName} to item");
                    }
                }
            }

            // Price will be recalculated after clearing round cards
        }

        private void ClearRoundCards()
        {
            // Clear player round cards
            foreach (var slot in _playerRoundSlots)
            {
                if (slot != null)
                {
                    var cardController = slot.GetComponentInChildren<CardController>();
                    if (cardController != null)
                    {
                        Destroy(cardController.gameObject);
                    }
                }
            }

            // Clear customer round cards
            foreach (var slot in _customerRoundSlots)
            {
                if (slot != null)
                {
                    var cardController = slot.GetComponentInChildren<CardController>();
                    if (cardController != null)
                    {
                        Destroy(cardController.gameObject);
                    }
                }
            }
        }

        private void UnloadNegotiationScene()
        {
            Debug.Log("[TradeNegotiationController] Unloading NegotiationScene");
            SceneManager.UnloadSceneAsync("NegotiationScene");
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

            // Unsubscribe from button events
            if (_acceptButton != null)
            {
                _acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
            }

            if (_rejectButton != null)
            {
                _rejectButton.onClick.RemoveListener(OnRejectButtonClicked);
            }

            if (_roundButton != null)
            {
                _roundButton.onClick.RemoveListener(OnRoundButtonClicked);
            }
        }
    }
}