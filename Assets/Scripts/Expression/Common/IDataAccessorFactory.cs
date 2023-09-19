using Expression.Map.MapEvent;

namespace Expression.Common
{
    public interface IDataAccessorFactory<T>
    {
        public T Get(Map.MapEvent.CommandVisitContext context);

        public void Set(Map.MapEvent.CommandVisitContext context, T value);

        public bool TestType(CommandVisitContext context, VariableType targetType);
    }
}
