using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Event
{
    public abstract class EventVisitorBase
    {
        public void Visit(IEvent eventData)
        {
            eventData.Visit(this);
        }

        public abstract void OnVisitCommonEvent(CommonEvent commonEvent);

        public abstract void OnVisitMapEvent(Map.MapEvent.EventData mapEvent);
    }
}