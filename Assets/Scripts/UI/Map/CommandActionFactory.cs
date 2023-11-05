using UnityEngine;
using System.Collections.Generic;
using UI.Action;
using Expression.Map.MapEvent;
using Expression.Map.MapEvent.Command;
using Expression.Common;

namespace UI.Map
{
    /// <summary>
    /// 一連のイベントコマンドから一つのイベントアクションを生成して返します
    /// 内部で呼び出しの度にgeneratedActionやcontrolInfoを共有するため、使いまわしできないので注意
    /// </summary>
    public class CommandActionFactory : ICommandVisitor
    {
        // 生成結果を保持するためにメンバとして持つ。排他に注意
        public ActionBase GeneratedAction { get; private set; }

        private ActionEnvironment actionEnv;

        private ActionControl controlInfo;

        private CommandVisitContext commandVisitContext;

        public CommandActionFactory(ActionEnvironment actionEnv, CommandVisitContext commandVisitContext,
            ActionControl controlInfo)
        {
            this.actionEnv = actionEnv;
            this.commandVisitContext = commandVisitContext;
            this.controlInfo = controlInfo;
        }

        public void OnVisitBaseCommand(EventCommandBase command)
        {
            GeneratedAction = new ActionBase();
        }

        public void OnVisitMessageCommand(MessageCommand command)
        {
            List<ActionBase> actions = new List<ActionBase>();
            string message = command.StringFactory.GenerateMessage(commandVisitContext);
            actions.Add(new ShowMessageAction(message, actionEnv));
            actions.Add(new WaitForInputAction());
            actions.Add(new CloseMessageAction(actionEnv, false));

            GeneratedAction = new MultiAction(actions);
        }

        public void OnVisitChoiceForkCommand(ChoiceForkCommand command)
        {
            // MultiActionにせずともonStart、onEndで処理しきれるので統合可能
            List<ActionBase> actions = new List<ActionBase>();
            actions.Add(new ShowChoiceAction(command.IndentDepth, command.ChoiceStrings, actionEnv));
            actions.Add(new WaitForChoiceAction(actionEnv));
            actions.Add(new CloseChoiceAction(actionEnv, false, controlInfo));

            GeneratedAction = new MultiAction(actions);
        }

        public void OnVisitForkBeginCommand(ForkBeginCommand command)
        {
            GeneratedAction = new ForkBeginAction(controlInfo, command.IndentDepth, command.LabelString);
        }

        public void OnVisitForkEndCommand(ForkEndCommand command)
        {
            GeneratedAction = new ForkEndAction(command.LabelString);
        }

        public void OnVisitForkByVariableIntCommand(ForkByVariableIntCommand command)
        {
            GeneratedAction = new ForkByVariableIntAction(controlInfo, command.IndentDepth,
                command.Conditions, commandVisitContext);
        }

        public void OnVisitChangeVariableIntCommand(ChangeVariableIntCommand command)
        {
            GeneratedAction = new ChangeVariableIntAction(command.Updaters, commandVisitContext);
        }

        public void OnVisitMovePositionCommand(MovePositionCommand command)
        {
            GeneratedAction = new MovePositionAction(actionEnv, command.MapId, command.X, command.Y);
        }

        public void OnVisitShowPictureCommand(ShowPictureCommand command)
        {
            string imagePath = $"{Application.streamingAssetsPath}/Data/"
                + command.FilePathFactory.GenerateMessage(commandVisitContext);
            byte[] baseTexBytes = Util.Common.FileLoader.LoadSync(imagePath);

            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(baseTexBytes);
            texture.Apply();
            GeneratedAction = new ShowPictureAction(command.Id, texture, actionEnv, command.PivotPattern, command.X, command.Y, command.Scale);
        }

        public void OnVisitShowMessageAsPictureCommand(ShowMessageAsPictureCommand command)
        {
            int id = command.IdFactory.GetInt(commandVisitContext);
            string message = command.MessageFactory.GenerateMessage(commandVisitContext);
            int x = command.XFactory.GetInt(commandVisitContext);
            int y = command.YFactory.GetInt(commandVisitContext);

            // デバッグ用にキー待ち
            List<ActionBase> actions = new List<ActionBase>();
            actions.Add(new ShowMessageAsPictureAction(id, message, actionEnv, command.PivotPattern, x, y));
            actions.Add(new WaitForInputAction());
            //actions.Add(new CloseChoiceAction(actionEnv, false, controlInfo));

            GeneratedAction = new MultiAction(actions);
        }

        public void OnVisitRemovePictureCommand(RemovePictureCommand command)
        {
            GeneratedAction = new RemovePictureAction(command.Id, actionEnv);
        }

        public void OnVisitCallEventCommand(CallEventCommand command)
        {
            var actionFactory = new EventActionFactory(actionEnv, commandVisitContext, command.NumberFactories,
                command.HasReturnValue, command.ReturnDestinationAccessor);
            IEventDataAccessor accessor = command.EventDataAccessorFactory.Create(commandVisitContext);
            GeneratedAction = actionFactory.GenerateAction(accessor.GetEvent());
        }

        public void OnVisitLoopStartCommand(LoopStartCommand command)
        {
            // この段階ではループ開始位置が不定なので、Action実行時に注入するようにする。
            // 【暫定】色んな所に関心ごとを要求してしまうので、美味い設計を整備したい
            var loopControlInfo = new LoopControlInfo(command.IndentDepth,
                command.IsInfinite, command.LoopCountAccessorFactory.GetInt(commandVisitContext));
            GeneratedAction = new LoopStartAction(controlInfo, loopControlInfo);
        }

        public void OnVisitLoopEndCommand(LoopEndCommand command)
        {
            GeneratedAction = new LoopEndAction(controlInfo);
        }

        public void OnVisitLoopBreakCommand(LoopBreakCommand command)
        {
            GeneratedAction=new LoopBreakAction(controlInfo);
        }
    }
}
