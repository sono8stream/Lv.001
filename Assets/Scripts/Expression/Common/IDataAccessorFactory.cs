using System;
using System.Collections.Generic;

namespace Expression.Common
{
    public interface IDataAccessorFactory<T>
    {
        public Common.IDataAccessor<T> Create(Map.MapEvent.CommandVisitContext context);
    }
}
