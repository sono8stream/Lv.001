using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent.Command
{
    public class MessageCommand : EventCommandBase
    {
        public CommandFactory.IStringFactory StringFactory { get; private set; }

        public MessageCommand(int indentDepth,
            CommandFactory.IStringFactory stringFactory) : base(indentDepth)
        {
            StringFactory = stringFactory;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMessageCommand(this);
        }
    }
}