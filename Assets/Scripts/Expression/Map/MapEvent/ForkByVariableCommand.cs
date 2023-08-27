using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ForkByVariableIntCommand : EventCommandBase
    {
        public ConditionInt[] Conditions { get; private set; }

        public ForkByVariableIntCommand(int indentDepth, ConditionInt[] conditions) : base(indentDepth)
        {
            Conditions = conditions;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitForkByVariableIntCommand(this);
        }
    }

    public class MetaCondition
    {
    }
}