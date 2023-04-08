using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Expression.Event;
using Expression.Map.MapEvent;

namespace UI.Action
{
    /// <summary>
    /// Event型にアクセスしてActionを生成する
    /// </summary>
    public class CommandActionCreator : EventVisitorBase
    {
        private EventAction generatedAction;
        private ActionEnvironment actionEnv;
        private CommandVisitContext commandVisitContext;

        public CommandActionCreator(ActionEnvironment actionEnv,CommandVisitContext commandVisitContext)
        {
            this.actionEnv = actionEnv;
            this.commandVisitContext = commandVisitContext;
        }

        public ActionBase GenerateAction(IEvent eventData)
        {
            Visit(eventData);
            return generatedAction;
        }

        public override void OnVisitCommonEvent(CommonEvent commonEvent)
        {
            // 【暫定】引数や呼び出しているイベント番号などを仕込めるよう、Contextなどに付与しておく
            var nextFactory = new Map.MapEventActionFactory(actionEnv, commandVisitContext);
            generatedAction = nextFactory.CreateActionFrom(commonEvent.EventCommands);
        }

        public override void OnVisitMapEvent(EventData mapEvent)
        {
            var nextFactory = new Map.MapEventActionFactory(actionEnv, commandVisitContext);
            generatedAction = nextFactory.CreateActionFrom(mapEvent.PageData[0].CommandDataArray);
        }
    }
}