using System;

namespace Expression.Event
{
    public class CommonEvent : IEvent
    {
        public CommonEventId Id { get; private set; }

        public string Name { get; private set; } 

        // 【暫定】EventPageData含むEventCommandをEvent名前空間に移動する
        public Map.MapEvent.EventCommandBase[] EventCommands { get; private set; }

        public int[] NumberVariables { get; set; }

        public string[] StringVariables { get; set; }

        public CommonEvent(CommonEventId id,string name, Map.MapEvent.EventCommandBase[] eventCommands)
        {
            Id = id;
            Name = name;
            EventCommands = eventCommands;

            NumberVariables = new int[95];
            StringVariables = new string[5];
        }

        public void Visit(EventVisitorBase visitor)
        {
            visitor.OnVisitCommonEvent(this);
        }
    }
}