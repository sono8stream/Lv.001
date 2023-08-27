using System;
using System.Collections.Generic;
using System.Linq;
using Expression.Common;

namespace Expression.Map.MapEvent.Command
{
    public class LoopStartCommand : EventCommandBase
    {
        public bool IsInfinite { get; private set; }
        public IDataAccessorFactory<int> LoopCountAccessorFactory { get; private set; }

        public LoopStartCommand(int indentDepth, bool isInfinite,
            IDataAccessorFactory<int> loopCountAccessorFactory) : base(indentDepth)
        {
            IsInfinite = isInfinite;
            LoopCountAccessorFactory = loopCountAccessorFactory;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitLoopStartCommand(this);
        }
    }
}