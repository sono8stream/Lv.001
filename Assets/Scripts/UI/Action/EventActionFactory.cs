using System.Linq;
using Expression.Common;
using Expression.Event;
using Expression.Map.MapEvent;

namespace UI.Action
{
    /// <summary>
    /// Event型にアクセスしてActionを生成する
    /// </summary>
    public class EventActionFactory : EventVisitorBase
    {
        private EventActionBase generatedAction;
        private ActionEnvironment actionEnv;
        private CommandVisitContext commandVisitContext;
        private IDataAccessorFactory[] numberFactories;

        private bool hasReturnValue;
        private IDataAccessorFactory returnDestinationAccessor;

        public EventActionFactory(ActionEnvironment actionEnv, CommandVisitContext commandVisitContext,
            IDataAccessorFactory[] numberFactories, bool hasReturnValue,
            IDataAccessorFactory returnDestinationAccessor)
        {
            this.actionEnv = actionEnv;
            this.commandVisitContext = commandVisitContext;
            this.numberFactories = numberFactories;

            this.hasReturnValue = hasReturnValue;
            this.returnDestinationAccessor = returnDestinationAccessor;
        }

        public ActionBase GenerateAction(IEvent eventData)
        {
            Visit(eventData);
            return generatedAction;
        }

        public override void OnVisitCommonEvent(CommonEvent commonEvent)
        {
            // 【暫定】文字列引数も付与しておく
            generatedAction = new CommonEventAction(commonEvent.Id,
                commonEvent.EventCommands,
                actionEnv, commandVisitContext,
                hasReturnValue, returnDestinationAccessor, commonEvent.ReturnValueAccessorFactory);
            
            // 引数をEventオブジェクトに割り当てる。
            int[] numberArgs = numberFactories.Select(factory => factory.GetInt(commandVisitContext)).ToArray();
            commonEvent.SetNumberArgs(numberArgs);
        }

        public override void OnVisitMapEvent(EventData mapEvent)
        {
            generatedAction = new MapEventAction(mapEvent.Id,
                mapEvent.PageData[0].CommandDataArray,
                actionEnv, commandVisitContext);
        }
    }
}