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
    public class EventActionFactory : EventVisitorBase
    {
        private EventAction generatedAction;
        private ActionEnvironment actionEnv;
        private CommandVisitContext commandVisitContext;

        public EventActionFactory(ActionEnvironment actionEnv, CommandVisitContext commandVisitContext)
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
            generatedAction = new EventAction(commonEvent.EventCommands,
                actionEnv, commandVisitContext);
        }

        public override void OnVisitMapEvent(EventData mapEvent)
        {
            generatedAction = new EventAction(mapEvent.PageData[0].CommandDataArray,
                actionEnv, commandVisitContext);
        }
    }
}