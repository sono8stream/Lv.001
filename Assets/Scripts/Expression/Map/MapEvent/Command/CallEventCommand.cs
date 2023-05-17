using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class CallEventCommand : EventCommandBase
    {
        public Common.IDataAccessorFactory<int>[] NumberFactories { get; private set; }

        public string[] StringArgs { get; private set; }

        public IEventDataAccessorFactory EventDataAccessorFactory { get; private set; }

        public CallEventCommand(Common.IDataAccessorFactory<int>[] numberFactories,
            string[] stringArgs, IEventDataAccessorFactory eventDataAccessorFactory)
        {
            NumberFactories = numberFactories;
            StringArgs = stringArgs;

            EventDataAccessorFactory = eventDataAccessorFactory;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitCallEventCommand(this);
        }
    }
}
