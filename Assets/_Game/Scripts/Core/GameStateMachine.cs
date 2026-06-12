using System;
using UnityEngine;

namespace Isekai12Realms.Core
{
    public enum GameState
    {
        Boot,
        Title,
        CharacterCreation,
        MainTown,
        WorldMap,
        Adventure,
        Battle,
        Result
    }

    public interface IGameStateMachine
    {
        GameState CurrentState { get; }
        event Action<GameState, GameState> OnStateChanged; // (previousState, newState)
        void TransitionTo(GameState newState);
    }

    public class GameStateMachine : IGameStateMachine
    {
        public GameState CurrentState { get; private set; } = GameState.Boot;
        public event Action<GameState, GameState> OnStateChanged;

        public void TransitionTo(GameState newState)
        {
            if (CurrentState == newState)
            {
                Debug.LogWarning($"Already in state {newState}");
                return;
            }

            GameState previousState = CurrentState;
            CurrentState = newState;
            Debug.Log($"[GameStateMachine] Transition: {previousState} -> {newState}");

            OnStateChanged?.Invoke(previousState, newState);
        }
    }
}
