using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    // 【暫定】StackEventsToを移動後は状態を持たないのでインターフェース化
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        // 【暫定】あくまでUnity内の処理はUI層に置き、特定のエンジンに依存する処理は持たせない
        public virtual void StackEventsTo(List<UnityEvent> events, EventCommands commands)
        {
            // 基底は何もしない
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.NoOperation());
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            // 基底は何もしない
        }
    }
}