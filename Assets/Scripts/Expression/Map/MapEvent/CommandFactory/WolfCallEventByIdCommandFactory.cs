using Util.Wolf;
using Expression.Common;
using Expression.Map.MapEvent.Command;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfCallEventByIdCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            int eventId = metaCommand.NumberArgs[1];
            int numberArgCount = metaCommand.NumberArgs[2] & 0xF;
            int stringArgCount = (metaCommand.NumberArgs[2] >> 4) & 0xF;
            int stringArgSpecifyType = (metaCommand.NumberArgs[2] >> 12) & 0xF;
            bool haveReturnValue = (metaCommand.NumberArgs[2] >> 24) > 0;

            Common.IDataAccessorFactory[] numberFactories = new Common.IDataAccessorFactory[numberArgCount];
            for(int i = 0; i < numberArgCount; i++)
            {
                numberFactories[i] = new Command.WolfIntAccessorFactory(false, metaCommand.NumberArgs[3 + i]);
            }

            IEventDataAccessorFactory factory = new Command.WolfEventDataAccessorFactory(eventId);

            int returnDestinationRaw = haveReturnValue
                ? metaCommand.NumberArgs[3 + numberArgCount + stringArgCount] : 0;
            IDataAccessorFactory returnDataAccessorFactory
                = new WolfIntAccessorFactory(false, returnDestinationRaw);

            return new Command.CallEventCommand(metaCommand.IndentDepth,
                numberFactories, metaCommand.StringArgs, factory,
                haveReturnValue, returnDataAccessorFactory);
        }
    }
}