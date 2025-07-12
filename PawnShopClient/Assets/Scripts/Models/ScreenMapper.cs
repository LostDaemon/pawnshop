using System.Collections.Generic;
using UnityEngine;

public static class ScreenMapper
{
    public static readonly Dictionary<Vector2Int, ScreenId> PositionToScreen = new()
    {
        [new Vector2Int(0, 0)] = ScreenId.Negotiation,
        [new Vector2Int(1, 0)] = ScreenId.Workshop,
        [new Vector2Int(0, 1)] = ScreenId.Storage,
        [new Vector2Int(-1, 0)] = ScreenId.TradeHall
        // Добавь другие при необходимости
    };

    public static ScreenId GetScreen(Vector2Int position)
    {
        return PositionToScreen.TryGetValue(position, out var screen)
            ? screen
            : ScreenId.Negotiation; // fallback
    }
}