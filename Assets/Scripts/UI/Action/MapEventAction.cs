using System.Collections.Generic;
using Expression.Event;
using Expression.Map.MapEvent;
using UnityEngine;

namespace UI.Action
{
    /// <summary>
    /// 1つのマップイベント全体を保持するアクション
    /// アクションを複数個まとめ、さらに分岐などを制御できる
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class MapEventAction : EventActionBase
    {
        private EventId prevId, currentId;

        public MapEventAction(EventId eventId,
            EventCommandBase[] commands,
            ActionEnvironment actionEnv,
            CommandVisitContext context) : base(commands, actionEnv, context)
        {
            this.currentId = eventId;
        }

        /// <inheritdoc/>
        public override void OnStart()
        {
            prevId = context.EventId;
            context.EventId = currentId;

            base.OnStart();
        }

        /// <inheritdoc/>
        public override void OnEnd()
        {
            base.OnEnd();
            context.EventId = prevId;
        }
    }
}
