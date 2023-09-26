using Expression.Common;
using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfVariableAccessorFactory : Common.IDataAccessorFactory
    {
        private WolfVariableAccessorCreator creator;

        public WolfVariableAccessorFactory(bool isConstValue, int rawVal)
        {
            creator = new WolfVariableAccessorCreator(isConstValue, rawVal);
        }

        public int GetInt(CommandVisitContext context)
        {
            return creator.Create(context).GetInt();
        }

        public string GetString(CommandVisitContext context)
        {
            return creator.Create(context).GetString();
        }

        public void SetInt(CommandVisitContext context, int value)
        {
            creator.Create(context).SetInt(value);
        }

        public void SetString(CommandVisitContext context, string value)
        {
            creator.Create(context).SetString(value);
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            return creator.Create(context).TestType(targetType);
        }
    }
}
