using System;

namespace Expression.Map.MapEvent
{
    public class EventPageData
    {
        public EventCommandData CommandData { get; private set; }

        public EventPageData(EventCommandData commandData)
        {
            CommandData = commandData;
        }
    }
}