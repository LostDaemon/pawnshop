using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using System;

public class NegotiationLogController : MonoBehaviour
{
    [SerializeField] private Transform _contentRoot;
    [SerializeField] private GameObject _dialogItemPrefab;
    [SerializeField] private ScrollRect _scrollRect;

    private INegotiateService _negotiateService;
    private Action<ItemModel> _onItemChangedHandler;

    [Inject]
    public void Construct(INegotiateService negotiateService)
    {
        _negotiateService = negotiateService;

        foreach (var record in _negotiateService.History)
            Append(record);

        _negotiateService.OnRecordAdded += Append;

        _onItemChangedHandler = _ => Clear();
        _negotiateService.OnCurrentItemChanged += _onItemChangedHandler;
    }

    private void OnDestroy()
    {
        if (_negotiateService != null)
        {
            _negotiateService.OnRecordAdded -= Append;
            _negotiateService.OnCurrentItemChanged -= _onItemChangedHandler;
        }
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