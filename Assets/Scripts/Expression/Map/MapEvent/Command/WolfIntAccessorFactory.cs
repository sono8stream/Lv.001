using Expression.Common;
using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfIntAccessorFactory : Common.IDataAccessorFactory<int>
    {
        private WolfIntAccessorCreator creator;

        public WolfIntAccessorFactory(bool isConstValue, int rawVal)
        {
            creator = new WolfIntAccessorCreator(isConstValue, rawVal);
        }

        public int Get(CommandVisitContext context)
        {
            return creator.Create(context).Get();
        }

        public void Set(CommandVisitContext context, int value)
        {
            creator.Create(context).Set(value);
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            return creator.Create(context).TestType(targetType);
        }
    }
}
