using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class MessageCommand:EventCommandBase
    {
        public string MessageText { get; set; }

        public MessageCommand(string messageText)
        {
            MessageText = messageText;
        }

        public override void StackEventsTo(List<UnityEvent> events, EventCommands commands)
        {
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMessageCommand(this);
        }
    }
}