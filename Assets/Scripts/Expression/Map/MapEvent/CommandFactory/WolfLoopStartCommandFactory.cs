
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
            int loopCountRaw = isInfinite ? 0 : metaCommand.NumberArgs[1];
            var loopCountAccessorFactory = new Command.WolfIntAccessorFactory(false, loopCountRaw);
            return new Command.LoopStartCommand(metaCommand.IndentDepth, isInfinite, loopCountAccessorFactory);
        }
    }
}
