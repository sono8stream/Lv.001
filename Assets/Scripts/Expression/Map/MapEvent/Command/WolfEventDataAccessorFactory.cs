﻿using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfEventDataAccessorFactory : IEventDataAccessorFactory
    {
        private int rawEventId;

        public WolfEventDataAccessorFactory(int rawEventId)
        {
            this.rawEventId = rawEventId;
        }

        public IEventDataAccessor Create(CommandVisitContext visitContext)
        {
            IEventDataAccessor accessor;
            if (rawEventId < 500000)
            {
                EventId id = new EventId(rawEventId);
                accessor = new MapEventDataAccessor(visitContext.MapId, id);
            }
            else
            {
                EventId id = new EventId(rawEventId - 500000);
                accessor = new CommonEventDataAccessor(id);
            }

            return accessor;
        }
    }
}
