using GuessifyBackend.Models.Enum;

namespace GuessifyBackend.Models
{
    public class GameEventState
    {

        private event EventHandler EveryoneAnsweredInGame = null!;
        private event EventHandler EveryOneVotedInGame = null!;

        public void SubscribeToEvent(EventHandler handler, EventType eventType)
        {
            if (eventType == EventType.EVERYONE_ANSWERED)
            {
                EveryoneAnsweredInGame += handler;
            }
            if (eventType == EventType.EVERYONE_VOTED)
            {
                EveryOneVotedInGame += handler;
            }

        }

        public void UnsubscribeFromEvent(EventHandler handler, EventType eventType)
        {
            if (eventType == EventType.EVERYONE_ANSWERED)
            {
                EveryoneAnsweredInGame -= handler;
            }
            if (eventType == EventType.EVERYONE_VOTED)
            {
                EveryOneVotedInGame -= handler;
            }

        }

        public void RaiseEvent(EventType eventType)
        {
            if (eventType == EventType.EVERYONE_ANSWERED)
            {
                EveryoneAnsweredInGame?.Invoke(this, EventArgs.Empty);
            }
            if (eventType == EventType.EVERYONE_VOTED)
            {
                EveryOneVotedInGame?.Invoke(this, EventArgs.Empty);
            }

        }
    }
}
