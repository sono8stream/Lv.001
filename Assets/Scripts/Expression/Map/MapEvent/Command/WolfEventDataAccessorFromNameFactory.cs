
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
            var repos = DI.DependencyInjector.It().CommonEventCommandsRepository;
            var eventId = repos.GetIdFromName(name);
            IEventDataAccessor accessor = new Event.CommonEventDataAccessor(eventId);

            return accessor;
        }
    }
}
