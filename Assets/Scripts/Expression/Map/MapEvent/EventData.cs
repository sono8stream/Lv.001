using System;

namespace Expression.Map.MapEvent
{
    public class EventData
    {
        public int EventId { get; private set; }

        public int PosX { get; private set; }

        public int PosY { get; private set; }

        public EventPageData[] PageData { get; private set; }

        public EventData(int eventId, int posX, int posY, EventPageData[] pageData)
        {
            EventId = eventId;
            PosX = posX;
            PosY = posY;
            PageData = pageData;
        }
    }
}