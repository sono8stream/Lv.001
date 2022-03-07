using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitBaseCommand(this);
        }
    }
}