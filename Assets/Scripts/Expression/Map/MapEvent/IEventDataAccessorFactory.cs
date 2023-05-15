using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public interface IEventDataAccessorFactory
    {
        public IEventDataAccessor Create(CommandVisitContext visitContext);
    }
}