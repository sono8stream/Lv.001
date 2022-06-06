using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    public class ForkEndCommand : EventCommandBase
    {
        public string LabelString { get; private set; }

        public ForkEndCommand(int indent)
        {
            LabelString = $"{indent}.{0}";
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitForkEndCommand(this);
        }
    }
}