using System.Collections.Generic;
using PawnShop.Models;
using UnityEngine;

namespace PawnShop.Helpers
{
    public static class ScreenMapper
    {
        public static readonly Dictionary<Vector2Int, ScreenId> PositionToScreen = new()
        {
            [new Vector2Int(0, 0)] = ScreenId.Negotiation,
            [new Vector2Int(1, 0)] = ScreenId.Workshop,
            [new Vector2Int(0, 1)] = ScreenId.TradeHall,
            [new Vector2Int(-1, 0)] = ScreenId.Laboratory
        };

        public static ScreenId GetScreen(Vector2Int position)
        {
            return PositionToScreen.TryGetValue(position, out var screen)
                ? screen
                : ScreenId.Negotiation; // fallback
        }
    }
}