using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent.Command
{
    public class LoopStartCommand : EventCommandBase
    {
        public bool IsInfinite { get; private set; }
        public int LoopCount { get; private set; }

        public LoopStartCommand(int indentDepth, bool isInfinite, int loopCount) : base(indentDepth)
        {
            IsInfinite = isInfinite;
            LoopCount = loopCount;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitLoopStartCommand(this);
        }
    }
}