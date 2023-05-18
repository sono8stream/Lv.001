using Util.Wolf;
using Expression.Map.MapEvent;
using Expression.Map.MapEvent.CommandFactory;
using Expression.Event.Command;

namespace Expression.Event.CommandFactory
{
    public class WolfDbControlCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            return new DbControlCommand();
        }
    }
}