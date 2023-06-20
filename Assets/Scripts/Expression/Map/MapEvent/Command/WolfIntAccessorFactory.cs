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

        public Common.IDataAccessor<int> Create(CommandVisitContext context)
        {
            return creator.Create(context);
        }
    }
}
