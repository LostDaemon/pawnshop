using System;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropContext
{
    public Transform InitialParent { get; set; }
    public Vector2 InitialPosition { get; set; }
    public Vector2 CurrentPosition { get; set; }
    public Canvas Canvas { get; set; }
    public GraphicRaycaster Raycaster { get; set; }
    public StorageType Source { get; set; }
    public StorageType Target { get; set; }
    public ITransferable Payload { get; set; }
}