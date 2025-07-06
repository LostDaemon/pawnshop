using UnityEngine;
using UnityEngine.EventSystems;

public interface IDraggable : IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemModel Payload { get; }
}