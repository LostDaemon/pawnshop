using System;
using PawnShop.Models;
using UnityEngine;

namespace PawnShop.Services
{
    public interface INavigationService
    {
        Vector2Int CurrentPosition { get; }
        ScreenId CurrentScreen { get; }

        event Action<Vector2Int> OnPositionChanged;
        event Action<Vector3> OnWorldPositionChanged;
        event Action<ScreenId> OnScreenChanged;

        void MoveLeft();
        void MoveRight();
        void MoveUp();
        void MoveDown();
        void MoveTo(Vector2Int newPosition);
    }
}