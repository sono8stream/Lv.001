using Util.Wolf;
using Expression.Common;
using Expression.Map.MapEvent.Command;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfCallEventByNameCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            int numberArgCount = metaCommand.NumberArgs[2] & 0xF;
            int stringArgCount = (metaCommand.NumberArgs[2] >> 4) & 0xF;
            int stringArgSpecifyType = (metaCommand.NumberArgs[2] >> 12) & 0xF;
            bool haveReturnValue = (metaCommand.NumberArgs[2] >> 24) > 0;

            Common.IDataAccessorFactory<int>[] numberFactories = new Common.IDataAccessorFactory<int>[numberArgCount];
            for (int i = 0; i < numberArgCount; i++)
            {
                numberFactories[i] = new Command.WolfIntAccessorFactory(false, metaCommand.NumberArgs[3 + i]);
            }

            string eventName = metaCommand.StringArgs[0];
            IEventDataAccessorFactory factory = new Command.WolfEventDataAccessorFromNameFactory(eventName);

            int returnDestinationRaw = haveReturnValue
                ? metaCommand.NumberArgs[3 + numberArgCount + stringArgCount] : 0;
            IDataAccessorFactory<int> returnDataAccessorFactory
                = new WolfIntAccessorFactory(false, returnDestinationRaw);

            return new Command.CallEventCommand(numberFactories, metaCommand.StringArgs, factory,
                haveReturnValue, returnDataAccessorFactory);
        }
    }
}