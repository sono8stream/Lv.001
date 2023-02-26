using Util.Wolf;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfShowTextCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            string text = metaCommand.StringArgs[0];
            return new MessageCommand(text);
        }
    }
}