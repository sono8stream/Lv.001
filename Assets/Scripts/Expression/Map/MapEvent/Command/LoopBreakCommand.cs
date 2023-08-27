using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent.Command
{
    public class LoopBreakCommand : EventCommandBase
    {
        public LoopBreakCommand(int indentDepth) : base(indentDepth)
        {
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitLoopBreakCommand(this);
        }
    }
}