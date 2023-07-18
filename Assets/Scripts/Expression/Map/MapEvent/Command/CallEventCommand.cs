using System;
using System.Collections.Generic;
using Expression.Common;

namespace Expression.Map.MapEvent.Command
{
    public class CallEventCommand : EventCommandBase
    {
        public Common.IDataAccessorFactory<int>[] NumberFactories { get; private set; }

        public string[] StringArgs { get; private set; }

        public IEventDataAccessorFactory EventDataAccessorFactory { get; private set; }

        public bool HasReturnValue { get; private set; }

        // 戻り値を返す先への参照
        public IDataAccessorFactory<int> ReturnDestinationAccessor { get; private set; }

        public CallEventCommand(Common.IDataAccessorFactory<int>[] numberFactories,
            string[] stringArgs, IEventDataAccessorFactory eventDataAccessorFactory,
            bool hasReturnValue, IDataAccessorFactory<int> returnDestinationAccessor)
        {
            NumberFactories = numberFactories;
            StringArgs = stringArgs;

            EventDataAccessorFactory = eventDataAccessorFactory;

            HasReturnValue = hasReturnValue;
            ReturnDestinationAccessor = returnDestinationAccessor;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitCallEventCommand(this);
        }
    }
}
