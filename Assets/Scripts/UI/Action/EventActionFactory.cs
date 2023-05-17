using System.Linq;
using Expression.Common;
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
        private IDataAccessorFactory<int>[] numberFactories;

        public EventActionFactory(ActionEnvironment actionEnv, CommandVisitContext commandVisitContext,
            IDataAccessorFactory<int>[] numberFactories)
        {
            this.actionEnv = actionEnv;
            this.commandVisitContext = commandVisitContext;
            this.numberFactories = numberFactories;
        }

        public ActionBase GenerateAction(IEvent eventData)
        {
            Visit(eventData);
            return generatedAction;
        }

        public override void OnVisitCommonEvent(CommonEvent commonEvent)
        {
            // �y�b��z������������t�^���Ă���
            commandVisitContext.CommonEventId = commonEvent.Id;
            generatedAction = new EventAction(commonEvent.EventCommands,
                actionEnv, commandVisitContext);
            
            // ������Event�I�u�W�F�N�g�Ɋ��蓖�Ă�B
            int[] numberArgs = numberFactories.Select(factory => factory.Create(commandVisitContext).Get()).ToArray();
            commonEvent.SetNumberArgs(numberArgs);
        }

        public override void OnVisitMapEvent(EventData mapEvent)
        {
            generatedAction = new EventAction(mapEvent.PageData[0].CommandDataArray,
                actionEnv, commandVisitContext);
        }
    }
}