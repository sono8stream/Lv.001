using System;

namespace Expression.Event
{
    public interface IEvent
    {
        public void Visit(EventVisitorBase visitor);
    }
}