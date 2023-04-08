using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Expression.Event;
using Expression.Map.MapEvent;

namespace UI.Action
{
    /// <summary>
    /// Event�^�ɃA�N�Z�X����Action�𐶐�����
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
            // �y�b��z�����Ȃǂ�Context�ɕt�^���Ă���
            commandVisitContext.CommonEventId = commonEvent.Id;
            var nextFactory = new Map.MapEventActionFactory(actionEnv, commandVisitContext);
            generatedAction = nextFactory.CreateActionFrom(commonEvent.EventCommands);
            commandVisitContext.CommonEventId = null;
        }

        public override void OnVisitMapEvent(EventData mapEvent)
        {
            var nextFactory = new Map.MapEventActionFactory(actionEnv, commandVisitContext);
            generatedAction = nextFactory.CreateActionFrom(mapEvent.PageData[0].CommandDataArray);
        }
    }
}