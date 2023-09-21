using Util.Wolf;
using Expression.Common;
using Expression.Map.MapEvent.Command;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfKeyWaitCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            IDataAccessorFactory returnDataAccessorFactory
                = new WolfIntAccessorFactory(false, metaCommand.NumberArgs[1]);

            return new EventCommandBase(metaCommand.IndentDepth);
        }
    }
}