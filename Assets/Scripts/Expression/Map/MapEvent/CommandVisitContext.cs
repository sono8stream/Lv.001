
namespace Expression.Map.MapEvent
{
    public class CommandVisitContext
    {
        public MapId MapId { private set; get; }
        public EventId EventId { private set; get; }

        public CommandVisitContext(MapId mapId, EventId eventId)
        {
            MapId = mapId;
            EventId = eventId;
        }
    }
}
