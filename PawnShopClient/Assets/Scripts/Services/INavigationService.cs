using System;
using UnityEngine;

public interface INavigationService
{
    Vector2Int CurrentPosition { get; }
    Vector2 RoomSize { get; }

    event Action<Vector2Int> OnPositionChanged;
    event Action<Vector3> OnWorldPositionChanged;

    void MoveLeft();
    void MoveRight();
    void MoveUp();
    void MoveDown();
    void MoveTo(Vector2Int newPosition);
}