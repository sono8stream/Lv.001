
namespace UI.Action
{
    class ChangeVariableIntAction : ActionBase
    {
        Expression.Map.MapEvent.VariableUpdater[] updaters;
        Expression.Map.MapEvent.CommandVisitContext context;

        public ChangeVariableIntAction(Expression.Map.MapEvent.VariableUpdater[] updaters,
             Expression.Map.MapEvent.CommandVisitContext context)
        {
            this.updaters = updaters;
            this.context = context;
        }

        /// <inheritdoc/>i
        public override bool Run()
        {
            for (int i = 0; i < updaters.Length; i++)
            {
                updaters[i].Update(context);
            }

            return true;
        }
    }
}
