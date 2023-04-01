using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class MapEventDataAccessor : IEventDataAccessor
    {
        MapId mapId;
        EventId eventId;

        public MapEventDataAccessor(MapId mapId, EventId eventId)
        {
            this.mapId = mapId;
            this.eventId = eventId;
        }

        public EventCommandBase[] GetEvent()
        {
            var repos = DI.DependencyInjector.It().MapDataRepository;
            return repos.Find(mapId).EventDataArray[eventId.Value].PageData[0].CommandDataArray;
        }
    }
}
