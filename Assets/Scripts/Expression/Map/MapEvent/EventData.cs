using Expression.Event;
using System;

namespace Expression.Map.MapEvent
{
    public class EventData : Event.IEvent
    {
        public EventId Id { get; private set; }

        public int PosX { get; private set; }

        public int PosY { get; private set; }

        public EventPageData[] PageData { get; private set; }

        public EventData(EventId id, int posX, int posY, EventPageData[] pageData)
        {
            Id = id;
            PosX = posX;
            PosY = posY;
            PageData = pageData;
        }

        public void Visit(EventVisitorBase visitor)
        {
            visitor.OnVisitMapEvent(this);
        }
    }
}
