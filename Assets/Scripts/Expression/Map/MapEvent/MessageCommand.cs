using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    public class MessageCommand:EventCommandBase
    {
        public string MessageText { get; set; }

        public MessageCommand(string messageText)
        {
            MessageText = messageText;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMessageCommand(this);
        }
    }
}