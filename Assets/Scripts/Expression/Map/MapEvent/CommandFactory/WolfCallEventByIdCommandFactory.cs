using Util.Wolf;

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

            Common.IDataAccessorFactory<int>[] numberFactories = new Common.IDataAccessorFactory<int>[numberArgCount];
            for(int i = 0; i < numberArgCount; i++)
            {
                numberFactories[i] = new Command.WolfIntAccessorFactory(false, metaCommand.NumberArgs[3 + i]);
            }

            IEventDataAccessorFactory factory = new Command.WolfEventDataAccessorFactory(eventId);

            return new Command.CallEventCommand(numberFactories, metaCommand.StringArgs, factory);
        }
    }
}