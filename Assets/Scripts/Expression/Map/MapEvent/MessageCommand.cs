using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class MessageCommand:EventCommand
    {
        private string MessageText { get; set; }

        public MessageCommand(string messageText)
        {
            MessageText = messageText;
        }

        public override void StackEventsTo(List<UnityEvent> events, EventCommands commands)
        {
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.WriteMessage(MessageText));
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.WaitForInput());
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.CloseMessage());
        }
    }
}