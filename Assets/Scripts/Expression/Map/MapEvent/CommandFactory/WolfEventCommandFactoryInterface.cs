using Util.Wolf;

namespace Expression.Map.MapEvent.CommandFactory
{
    public interface WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand);
    }
}