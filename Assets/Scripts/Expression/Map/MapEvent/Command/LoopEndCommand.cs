using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent.Command
{
    public class LoopEndCommand : EventCommandBase
    {
        public LoopEndCommand(int indentDepth) : base(indentDepth)
        {
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitLoopEndCommand(this);
        }
    }
}