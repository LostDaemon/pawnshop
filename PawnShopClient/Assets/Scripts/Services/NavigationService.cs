using System;
using UnityEngine;

public class NavigationService : INavigationService
{
    public Vector2Int CurrentPosition { get; private set; } = Vector2Int.zero;
    public Vector2 RoomSize { get; }

    public event Action<Vector2Int> OnPositionChanged;
    public event Action<Vector3> OnWorldPositionChanged;
    public event Action<ScreenId> OnScreenChanged;

    private ScreenId _currentScreen;
    public ScreenId CurrentScreen
    {
        get => _currentScreen;
        private set
        {
            if (_currentScreen != value)
            {
                _currentScreen = value;
                Debug.Log($"[Navigation] Screen changed to: {_currentScreen}");
                OnScreenChanged?.Invoke(_currentScreen);
            }
        }
    }

    public NavigationService(Vector2 roomSize)
    {
        RoomSize = roomSize;
        CurrentScreen = ScreenMapper.GetScreen(CurrentPosition);
    }

    public void MoveLeft() => MoveTo(CurrentPosition + Vector2Int.left);
    public void MoveRight() => MoveTo(CurrentPosition + Vector2Int.right);
    public void MoveUp() => MoveTo(CurrentPosition + Vector2Int.up);
    public void MoveDown() => MoveTo(CurrentPosition + Vector2Int.down);

    public void MoveTo(Vector2Int newPosition)
    {
        if (newPosition == CurrentPosition)
            return;

        CurrentPosition = newPosition;

        Vector3 worldPosition = new Vector3(
            CurrentPosition.x * RoomSize.x,
            CurrentPosition.y * RoomSize.y,
            0f
        );

        OnPositionChanged?.Invoke(CurrentPosition);
        OnWorldPositionChanged?.Invoke(worldPosition);
        CurrentScreen = ScreenMapper.GetScreen(CurrentPosition);
    }
}