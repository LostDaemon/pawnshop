using System;

public interface IDragAndDropSource
{
    public StorageType StorageType { get; }
}

public interface IDragAndDropTarget
{
    public StorageType StorageType { get; }
}

public interface IDragAndDropContext
{
    public IDragAndDropSource Source { get; }
    public IDragAndDropTarget Target { get; }
    public ITransferable Payload { get; }
}


public class DragAndDropContext2 : IDragAndDropContext
{
    public IDragAndDropSource Source { get; set; }
    public IDragAndDropTarget Target { get; set; }
    public ITransferable Payload { get; set; }
}

