
namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfLoopEndCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            return new Command.LoopEndCommand(metaCommand.IndentDepth);
        }
    }
}
