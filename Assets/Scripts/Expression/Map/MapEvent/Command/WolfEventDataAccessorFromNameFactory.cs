
namespace Expression.Map.MapEvent.Command
{
    public class WolfEventDataAccessorFromNameFactory : IEventDataAccessorFactory
    {
        private string name;

        public WolfEventDataAccessorFromNameFactory(string name)
        {
            this.name = name;
        }

        public IEventDataAccessor Create(CommandVisitContext visitContext)
        {
            IEventDataAccessor accessor;

            var repos = DI.DependencyInjector.It().CommonEventCommandsRepository;
            var eventId = repos.GetIdFromName(name);
            accessor = new Event.CommonEventDataAccessor(eventId);

            return accessor;
        }
    }
}
