using System.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using Vector2 = UnityEngine.Vector2;

public class DragDropService : IDragAndDropService
{
    private IGameStorageService<ItemModel> _source;
    private IGameStorageService<ItemModel> _target;
    private IStorageRouterService _storageRouterService;
    private ItemModel _payload;
    private Vector2 _currentPosition;

    [Inject]
    public void Construct(IStorageRouterService storageRouterService)
    {
        _storageRouterService = storageRouterService;
    }

    public void StartDrag(IGameStorageService<ItemModel> source, ItemModel payload)
    {
        _source = source;
        _target = null;
        _payload = payload;
    }

    public void Drag(IDraggable draggable, PointerEventData eventData)
    {
        var gObject = draggable as MonoBehaviour;
        gObject.transform.position = eventData.position;
        _currentPosition = eventData.position;
    }

    public void Drop(IGameStorageService<ItemModel> target)
    {
        _target = target;
        OnDragAndDropSucceeded();
    }

    private void OnDragAndDropSucceeded()
    {
        if (_source == null || _target == null)
        {
            return;
        }

        if (_source == _target)
        {
            return;
        }

        _storageRouterService.Transfer(_payload, _source, _target);
    }
}