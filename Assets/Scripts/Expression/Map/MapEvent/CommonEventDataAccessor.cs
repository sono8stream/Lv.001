using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class CommonEventDataAccessor : IEventDataAccessor
    {
        EventId eventId;

        public CommonEventDataAccessor(EventId eventId)
        {
            this.eventId = eventId;
        }

        public EventCommandBase[] GetEvent()
        {
            var repos = DI.DependencyInjector.It().CommonEventCommandsRepository;
            return repos.GetCommands(eventId.Value);
        }
    }
}
