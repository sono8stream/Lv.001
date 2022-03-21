using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ForkByVariableCommand : EventCommandBase
    {
        public int IndentDepth { get; private set; }
        public Condition[] Conditions { get; private set; }

        public ForkByVariableCommand(int indentDepth, Condition[] conditions)
        {
            IndentDepth = indentDepth;
            Conditions = conditions;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitBaseCommand(this);
        }
    }

    public class MetaCondition
    {
    }
}