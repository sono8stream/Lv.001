using System;

namespace Expression.Event
{
    public class CommonEvent : IEvent
    {
        public CommonEventId Id { get; private set; }

        // �y�b��zEventPageData�܂�EventCommand��Event���O��ԂɈړ�����
        public Map.MapEvent.EventCommandBase[] EventCommands { get; private set; }

        public int[] NumberVariables { get; set; }

        public string[] StringVariables { get; set; }

        public CommonEvent(CommonEventId id, Map.MapEvent.EventCommandBase[] eventCommands)
        {
            Id = id;
            EventCommands = eventCommands;
        }

        public void Visit(EventVisitorBase visitor)
        {
            visitor.OnVisitCommonEvent(this);
        }
    }
}