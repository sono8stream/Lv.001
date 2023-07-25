
namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfLoopStartCommandFactory : WolfEventCommandFactoryInterface
    {
        private bool isInfinite;

        public WolfLoopStartCommandFactory(bool isInfinite)
        {
            this.isInfinite = isInfinite;
        }

        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            int loopCount = isInfinite ? 0 : metaCommand.NumberArgs[1];
            return new Command.LoopStartCommand(metaCommand.IndentDepth, isInfinite, loopCount);
        }
    }
}
