using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDropService : IDragAndDropService
{
    private readonly EventSystem _eventSystem;
    private Dictionary<IDraggable, DragAndDropContext> _registered = new();

    public event Action<DragAndDropContext> OnDragAndDropSucceed;

    public void Register(IDraggable draggable, DragAndDropContext context)
    {
        if (draggable == null)
        {
            Debug.LogWarning("Draggable is null, cannot register.");
            return;
        }

        if (_registered.ContainsKey(draggable))
        {
            Debug.LogWarning("Draggable already registered.");
            return;
        }

        _registered[draggable] = context;
        draggable.OnItemDrop += HandleDrop;
    }

    public void UpdateContext(IDraggable draggable, DragAndDropContext context)
    {
        _registered[draggable] = context;
    }

    private void HandleDrop(IDraggable draggable, Vector2 vector)
    {
        if (!_registered.TryGetValue(draggable, out var context))
        {
            Debug.LogWarning("Draggable not registered.");
            return;
        }

        try
        {
            var pointerData = new PointerEventData(_eventSystem)
            {
                position = context.CurrentPosition,
            };

            var results = new List<RaycastResult>();
            context.Raycaster.Raycast(pointerData, results);


            foreach (var result in results)
            {
                Debug.Log(result.gameObject.name);
                var target = result.gameObject.GetComponentInParent<IDropTarget>();
                if (target != null)
                {
                    if (target.TryDrop(draggable))
                    {
                        OnDragAndDropSucceed?.Invoke(context);
                    }
                    return;
                }
            }

            Recover(draggable, context);
        }
        finally
        {
            draggable.OnItemDrop -= HandleDrop;
            _registered.Remove(draggable);
        }
    }



    private void Recover(IDraggable draggable, DragAndDropContext context)
    {

        var transform = draggable as Transform;

        if (transform == null || context == null)
        {
            Debug.LogWarning("Draggable or context is null, cannot recover.");
            return;
        }

        transform.SetParent(context.InitialParent);
        transform.position = context.InitialPosition;
    }
}