using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Service.Interfaces
{
    public interface IGameEventManager
    {
        void RegisterNewGameEventState(string gameId);

        void RemoveGameEventState(string gameId);
        void RaiseEventOfGame(string gameId, EventType eventType);
        void SubscribeToEvent(string gameId, EventHandler handler, EventType eventType);
        void UnsubscribeFromEvent(string gameId, EventHandler handler, EventType eventType);
    }
}
