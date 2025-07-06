using System;
using UnityEngine;
using UnityEngine.EventSystems;


public interface IDraggable : IBeginDragHandler, IPointerDownHandler, IDragHandler, IDropHandler
{
    public event Action<IDraggable, Vector2, DragAndDropContext> OnItemDrop;
    public event Action<DragAndDropContext> OnItemStartDragging;
}