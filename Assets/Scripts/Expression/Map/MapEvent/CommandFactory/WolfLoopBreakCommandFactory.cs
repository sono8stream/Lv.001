
namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfLoopBreakCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            return new Command.LoopBreakCommand(metaCommand.IndentDepth);
        }
    }
}
