using System;

namespace Common
{
    public enum GameState
    {
        Initializing,
        Login,
        Lobby,
        Studying,
        Paused
    }
    
    public class StateManager : SingletonBehaviour<StateManager>
    {
        public GameState CurrentState { get; private set; }
        public event Action<GameState, GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            GameState oldState = CurrentState;
            CurrentState = newState;
            OnStateChanged?.Invoke(oldState, newState);
            Logger.Log($"State changed from {oldState} to {newState}");
        }
    }
}