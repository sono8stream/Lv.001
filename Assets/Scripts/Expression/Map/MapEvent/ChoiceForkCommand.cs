using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ChoiceForkCommand : EventCommandBase
    {
        public string[] ChoiceStrings { get; private set; }

        public ChoiceForkCommand(string[] choiceStrings)
        {
            ChoiceStrings = choiceStrings;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitChoiceForkCommand(this);
        }
    }
}