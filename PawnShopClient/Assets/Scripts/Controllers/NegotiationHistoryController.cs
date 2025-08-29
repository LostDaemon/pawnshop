using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System;
using System.Collections;
using PawnShop.Services;
using PawnShop.Models;

namespace PawnShop.Controllers
{
    public class NegotiationHistoryController : MonoBehaviour
    {
        [Header("Display Settings")]
        [SerializeField] private float _minIntervalBetweenMessages = 500f; // Minimum time between messages in milliseconds
        
        [Header("UI References")]
        [SerializeField] private Transform _contentRoot;
        [SerializeField] private GameObject _playerDialogItemPrefab;
        [SerializeField] private GameObject _customerDialogItemPrefab;
        [SerializeField] private GameObject _systemDialogItemPrefab;
        [SerializeField] private ScrollRect _scrollRect;

        private INegotiationHistoryService _historyService;
        private INegotiationService _negotiateService;
        private Action<ItemModel> _onItemChangedHandler;
        
        private float _lastMessageTime = 0f;

        [Inject]
        public void Construct(INegotiationHistoryService historyService, INegotiationService negotiateService)
        {
            _historyService = historyService;
            _negotiateService = negotiateService;

            foreach (var record in _historyService.History)
                Append(record);

            _historyService.OnRecordAdded += Append;

            _onItemChangedHandler = _ => Clear();
            _negotiateService.OnCurrentItemChanged += _onItemChangedHandler;
        }

        private void OnDestroy()
        {
            if (_historyService != null)
                _historyService.OnRecordAdded -= Append;

            if (_negotiateService != null)
                _negotiateService.OnCurrentItemChanged -= _onItemChangedHandler;
        }

        private void Append(IHistoryRecord record)
        {
            Debug.Log($"[NegotiationHistoryController] Appending record: Source={record.Source}, Message={record.Message}");
            
            float currentTime = Time.time;
            float timeSinceLastMessage = currentTime - _lastMessageTime;
            
            // If enough time has passed or this is the first message, display immediately
            if (timeSinceLastMessage >= _minIntervalBetweenMessages / 1000f || _lastMessageTime == 0f)
            {
                DisplayRecord(record);
                _lastMessageTime = currentTime;
            }
            else
            {
                // Wait for the remaining time and then display
                float remainingTime = (_minIntervalBetweenMessages / 1000f) - timeSinceLastMessage;
                StartCoroutine(DisplayWithDelay(record, remainingTime));
            }
        }

        private IEnumerator DisplayWithDelay(IHistoryRecord record, float delay)
        {
            yield return new WaitForSeconds(delay);
            DisplayRecord(record);
            _lastMessageTime = Time.time;
        }

        private void DisplayRecord(IHistoryRecord record)
        {
            GameObject instance = null;

            switch (record.Source)
            {
                case HistoryRecordSource.Player:
                    instance = Instantiate(_playerDialogItemPrefab, _contentRoot);
                    break;
                case HistoryRecordSource.Customer:
                    instance = Instantiate(_customerDialogItemPrefab, _contentRoot);
                    break;
                case HistoryRecordSource.System:
                    instance = Instantiate(_systemDialogItemPrefab, _contentRoot);
                    break;
                default:
                    Debug.LogWarning($"Unknown history record source: {record.Source}");
                    return;
            }

            var text = instance.GetComponentInChildren<TMP_Text>();

            if (text != null)
            {
                text.text = record.Message;
                Debug.Log($"[NegotiationHistoryController] Set text to: {record.Message}");
            }
            else
                Debug.LogWarning("DialogItem prefab missing TMP_Text");

            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        public void Clear()
        {
            // Stop any pending display coroutines
            StopAllCoroutines();
            _lastMessageTime = 0f;
            
            // Clear UI elements
            foreach (Transform child in _contentRoot)
                Destroy(child.gameObject);
        }
    }
}