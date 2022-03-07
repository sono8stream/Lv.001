using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    // 【暫定】StackEventsToを移動後は状態を持たないのでインターフェース化
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            // 基底は何もしない
            visitor.OnVisitBaseCommand(this);
        }
    }
}