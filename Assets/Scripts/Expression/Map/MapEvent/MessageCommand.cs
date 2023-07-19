using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent
{
    public class MessageCommand : EventCommandBase
    {
        public CommandFactory.IStringFactory StringFactory { get; private set; }

        public MessageCommand(CommandFactory.IStringFactory stringFactory)
        {
            StringFactory = stringFactory;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMessageCommand(this);
        }
    }
}