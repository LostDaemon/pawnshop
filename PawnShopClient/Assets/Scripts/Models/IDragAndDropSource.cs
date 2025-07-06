using UnityEngine.EventSystems;

public interface IDragAndDropSource : IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public StorageType StorageType { get; }
}

public interface IDragAndDropTarget : IDropHandler
{
    public StorageType StorageType { get; }
}

