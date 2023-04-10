using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Event
{
    public class CommonEventDataAccessor : Map.MapEvent.IEventDataAccessor
    {
        CommonEventId eventId;

        public CommonEventDataAccessor(CommonEventId eventId)
        {
            this.eventId = eventId;
        }

        public Event.IEvent GetEvent()
        {
            var repos = DI.DependencyInjector.It().CommonEventCommandsRepository;
            return repos.GetEvent(eventId);
        }
    }
}
