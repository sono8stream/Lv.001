using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ForkByVariableIntCommand : EventCommandBase
    {
        public int IndentDepth { get; private set; }
        public ConditionInt[] Conditions { get; private set; }

        public ForkByVariableIntCommand(int indentDepth, ConditionInt[] conditions)
        {
            IndentDepth = indentDepth;
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