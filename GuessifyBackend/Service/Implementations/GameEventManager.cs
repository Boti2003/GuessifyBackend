using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using GuessifyBackend.Service.Interfaces;
using System.Collections.Concurrent;

namespace GuessifyBackend.Service.Implementations
{
    public class GameEventManager : IGameEventManager
    {

        private ConcurrentDictionary<string, GameEventState> _gameEvents;

        public GameEventManager()
        {
            _gameEvents = new ConcurrentDictionary<string, GameEventState>();
        }

        public void RegisterNewGameEventState(string gameId)
        {
            _gameEvents.TryAdd(gameId, new GameEventState());

        }

        public void RaiseEventOfGame(string gameId, EventType eventType)
        {
            _gameEvents[gameId].RaiseEvent(eventType);

        }

        public void SubscribeToEvent(string gameId, EventHandler handler, EventType eventType)
        {
            _gameEvents[gameId].SubscribeToEvent(handler, eventType);
        }

        public void UnsubscribeFromEvent(string gameId, EventHandler handler, EventType eventType)
        {
            _gameEvents[gameId].UnsubscribeFromEvent(handler, eventType);
        }

        public void RemoveGameEventState(string gameId)
        {
            _gameEvents.TryRemove(gameId, out _);
        }
    }
}
