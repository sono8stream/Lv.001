using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public interface IEventDataAccessor
    {
        public Event.IEvent GetEvent();
    }
}