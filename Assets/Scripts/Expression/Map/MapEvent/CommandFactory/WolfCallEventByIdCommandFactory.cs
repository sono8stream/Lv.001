using Util.Wolf;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfCallEventByIdCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            return new Command.CallWolfEventCommand(metaCommand.NumberArgs, metaCommand.StringArgs);
        }
    }
}