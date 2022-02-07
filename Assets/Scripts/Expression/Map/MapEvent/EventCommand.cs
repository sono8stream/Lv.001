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
            // Šî’ê‚Í‰½‚à‚µ‚È‚¢
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.NoOperation());
        }
    }
}