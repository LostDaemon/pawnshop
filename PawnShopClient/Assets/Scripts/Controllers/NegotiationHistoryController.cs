using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System;

public class NegotiationHistoryController : MonoBehaviour
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _dialogItemPrefab;
    [SerializeField] private ScrollRect _scrollRect;

    private INegotiationHistoryService _historyService;
    private INegotiationService _negotiateService;
    private Action<ItemModel> _onItemChangedHandler;

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
        var instance = Instantiate(_dialogItemPrefab, _contentRoot);
        var text = instance.GetComponentInChildren<TMP_Text>();

        if (text != null)
            text.text = record.Message;
        else
            Debug.LogWarning("DialogItem prefab missing TMP_Text");

        Canvas.ForceUpdateCanvases();
        _scrollRect.verticalNormalizedPosition = 0f;
    }

    public void Clear()
    {
        foreach (Transform child in _contentRoot)
            Destroy(child.gameObject);
    }
}