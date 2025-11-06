using GuessifyBackend.Models;
using GuessifyBackend.Models.Enum;
using System.Collections.Concurrent;

namespace GuessifyBackend.Service
{
    public class GameEventManager
    {

        private ConcurrentDictionary<string, GameRoundEventState> _gameEvents;

        public GameEventManager()
        {
            _gameEvents = new ConcurrentDictionary<string, GameRoundEventState>();
        }

        public void RegisterNewGameEventState(string gameId)
        {
            _gameEvents.TryAdd(gameId, new GameRoundEventState());

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

    }
}
