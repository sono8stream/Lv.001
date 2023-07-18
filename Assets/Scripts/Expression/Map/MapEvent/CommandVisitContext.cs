
namespace Expression.Map.MapEvent
{
    public class CommandVisitContext
    {
        public MapId MapId { private set; get; }
        public EventId EventId { set; get; }
        public Event.CommonEventId CommonEventId { set; get; }

        public CommandVisitContext(MapId mapId)
        {
            MapId = mapId;
            EventId = null;
            CommonEventId = null;
        }
    }
}
