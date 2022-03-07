using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ChoiceCommand : EventCommandBase
    {
        public string MessageText { get; set; }

        public ChoiceCommand(string messageText)
        {
            MessageText = messageText;
        }

        public override void StackEventsTo(List<UnityEvent> events, ActionEnvironment commands)
        {
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitChoiceCommand(this);
        }
    }
}