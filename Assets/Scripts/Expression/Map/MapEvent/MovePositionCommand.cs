using System;

namespace Expression.Map.MapEvent
{
    public class MovePositionCommand : EventCommandBase
    {
        public EventId EventId { get; private set; }

        public MapId MapId { get; private set; }

        public int X { get; private set; }

        public int Y { get; private set; }

        public MovePositionCommand(int indentDepth, EventId eventId, int x, int y, MapId mapId) : base(indentDepth)
        {
            EventId = eventId;
            X = x;
            Y = y;
            MapId = mapId;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMovePositionCommand(this);
        }
    }
}
