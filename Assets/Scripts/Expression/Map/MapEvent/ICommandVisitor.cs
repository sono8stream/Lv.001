using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public interface ICommandVisitor
    {
        public void OnVisitMessageCommand(MessageCommand command);
        public void OnVisitChoiceCommand(ChoiceCommand command);

        public void OnVisitBaseCommand(EventCommandBase command);
    }
}