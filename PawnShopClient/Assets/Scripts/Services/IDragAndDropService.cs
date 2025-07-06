using System;

public interface IDragAndDropService
{
    public void Register(IDraggable draggable, DragAndDropContext context);
    public void UpdateContext(IDraggable draggable, DragAndDropContext context);
    public event Action<DragAndDropContext> OnDragAndDropSucceed;

}