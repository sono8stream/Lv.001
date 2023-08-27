
namespace Expression.Map.MapEvent
{
    public class CommandVisitContext
    {
        public MapId MapId { private set; get; }
        public EventId EventId { set; get; }
        public Event.CommonEventId CommonEventId { set; get; }

        // 現在のループ処理を何回実行したか保持
        public int LoopExecuteCount { get; private set; }

        public CommandVisitContext(MapId mapId)
        {
            MapId = mapId;
            EventId = null;
            CommonEventId = null;
            LoopExecuteCount = 0;
        }
    }
}
