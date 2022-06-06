using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    public class ForkBeginCommand : EventCommandBase
    {
        public int IndentDepth { get; private set; }
        public string LabelString { get; private set; }

        public ForkBeginCommand(int indent, int choiceNo)
        {
            IndentDepth = indent;
            LabelString = $"{indent}.{choiceNo}";
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitForkBeginCommand(this);
        }
    }
}