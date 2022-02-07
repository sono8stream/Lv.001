using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class EventCommand
    {

        public EventCommand()
        {
        }

        public virtual void StackEventsTo(List<UnityEvent> events, EventCommands commands)
        {
            // 基底は何もしない
        }
    }
}