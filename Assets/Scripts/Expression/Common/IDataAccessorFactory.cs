using System;
using System.Collections.Generic;

namespace Expression.Common
{
    public interface IDataAccessorFactory<T>
    {
        public T Get(Map.MapEvent.CommandVisitContext context);

        public void Set(Map.MapEvent.CommandVisitContext context, T value);
    }
}
