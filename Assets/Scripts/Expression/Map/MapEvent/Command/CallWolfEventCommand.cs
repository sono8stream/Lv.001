using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class CallWolfEventCommand : EventCommandBase
    {
        public int[] NumberArgs { get; private set; }

        public string[] StringArgs { get; private set; }

        public CallWolfEventCommand(int[] numberArgs,string[] stringArgs)
        {
            NumberArgs = numberArgs;
            StringArgs = stringArgs;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitCallEventCommand(this);
        }
    }
}
