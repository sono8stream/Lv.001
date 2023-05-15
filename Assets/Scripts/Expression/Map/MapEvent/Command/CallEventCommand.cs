using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class CallEventCommand : EventCommandBase
    {
        public int[] NumberArgs { get; private set; }

        public string[] StringArgs { get; private set; }

        public IEventDataAccessorFactory EventDataAccessorFactory { get; private set; }

        public CallEventCommand(int[] numberArgs, string[] stringArgs, IEventDataAccessorFactory eventDataAccessorFactory)
        {
            NumberArgs = numberArgs;
            StringArgs = stringArgs;

            EventDataAccessorFactory = eventDataAccessorFactory;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitCallEventCommand(this);
        }
    }
}
