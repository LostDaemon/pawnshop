using System;
using System.Collections.Generic;
using Zenject;

public class GameStateMachine
{
    private readonly DiContainer _container;
    private readonly Dictionary<Type, IGameState> _states = new();
    private IGameState _activeState;

    public GameStateMachine(DiContainer container)
    {
        _container = container;
    }

    public void Enter<TState>() where TState : IGameState
    {
        _activeState?.Exit();

        var stateType = typeof(TState);

        if (!_states.TryGetValue(stateType, out var newState))
        {
            newState = _container.Instantiate<TState>();
            _states.Add(stateType, newState);
        }

        _activeState = newState;
        _activeState.Enter();
    }
}