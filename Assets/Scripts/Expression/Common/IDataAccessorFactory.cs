using Expression.Map.MapEvent;

namespace Expression.Common
{
    public interface IDataAccessorFactory
    {
        public int GetInt(Map.MapEvent.CommandVisitContext context);
        public string GetString(Map.MapEvent.CommandVisitContext context);

        public void SetInt(Map.MapEvent.CommandVisitContext context, int value);
        public void SetString(Map.MapEvent.CommandVisitContext context, string value);

        public bool TestType(CommandVisitContext context, VariableType targetType);
    }
}
