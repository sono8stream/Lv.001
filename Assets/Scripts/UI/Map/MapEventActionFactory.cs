using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;
using UI.Action;
using Expression.Map.MapEvent;
using Expression.Map.MapEvent.Command;

namespace UI.Map
{
    /// <summary>
    /// 一連のイベントコマンドから一つのイベントアクションを生成して返します
    /// 内部で呼び出しの度にgeneratedActionやcontrolInfoを共有するため、使いまわしできないので注意
    /// </summary>
    public class MapEventActionFactory : Expression.Map.MapEvent.ICommandVisitor
    {
        // 生成結果を保持するためにメンバとして持つ。排他に注意
        private ActionBase generatedAction;

        private ActionEnvironment actionEnv;

        private ActionControl controlInfo;

        public MapEventActionFactory(ActionEnvironment actionEnv)
        {
            this.actionEnv = actionEnv;
        }

        /// <summary>
        /// 一連のコマンドからマルチアクションを生成して返します
        /// 単一ではなく一連のコマンドを用いるのは分岐などを入れ子になったアクションとして生成するため
        /// </summary>
        /// <param name="commands">イベントに含まれる一連のコマンド</param>
        /// <returns></returns>
        public EventAction CreateActionFrom(EventCommandBase[] commands)
        {
            controlInfo = new ActionControl();
            List<ActionBase> actions = new List<ActionBase>();

            for (int i = 0; i < commands.Length; i++)
            {
                commands[i].Visit(this);
                actions.Add(generatedAction);
            }
            return new EventAction(actions, controlInfo);
        }

        public void OnVisitBaseCommand(EventCommandBase command)
        {
            generatedAction = new ActionBase();
        }

        public void OnVisitMessageCommand(MessageCommand command)
        {
            List<ActionBase> actions = new List<ActionBase>();
            actions.Add(new ShowMessageAction(command.MessageText, actionEnv));
            actions.Add(new WaitForInputAction());
            actions.Add(new CloseMessageAction(actionEnv, false));

            generatedAction = new MultiAction(actions);
        }

        public void OnVisitChoiceForkCommand(ChoiceForkCommand command)
        {
            // MultiActionにせずともonStart、onEndで処理しきれるので統合可能
            List<ActionBase> actions = new List<ActionBase>();
            actions.Add(new ShowChoiceAction(command.IndentDepth, command.ChoiceStrings, actionEnv));
            actions.Add(new WaitForChoiceAction(actionEnv));
            actions.Add(new CloseChoiceAction(actionEnv, false, controlInfo));

            generatedAction = new MultiAction(actions);
        }

        public void OnVisitForkBeginCommand(ForkBeginCommand command)
        {
            generatedAction = new ForkBeginAction(controlInfo, command.IndentDepth, command.LabelString);
        }

        public void OnVisitForkEndCommand(ForkEndCommand command)
        {
            generatedAction = new ForkEndAction(command.LabelString);
        }

        public void OnVisitForkByVariableIntCommand(ForkByVariableIntCommand command)
        {
            generatedAction = new ForkByVariableIntAction(controlInfo, command.IndentDepth, command.Conditions);
        }

        public void OnVisitChangeVariableIntCommand(ChangeVariableIntCommand command)
        {
            generatedAction = new ChangeVariableIntAction(command.Updaters);
        }

        public void OnVisitMovePositionCommand(MovePositionCommand command)
        {
            generatedAction = new MovePositionAction(actionEnv,command.MapId, command.X, command.Y);
        }

        public void OnVisitShowPictureCommand(ShowPictureCommand command)
        {
            string imagePath = $"{Application.streamingAssetsPath}/Data/" + command.FilePath;
            byte[] baseTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
            
            Texture2D texture = new Texture2D(0, 0);
            texture.LoadImage(baseTexBytes);
            texture.Apply();
            generatedAction = new ShowPictureAction(command.Id, texture, actionEnv, command.PivotPattern, command.X, command.Y, command.Scale);
        }

        public void OnVisitRemovePictureCommand(RemovePictureCommand command)
        {
            generatedAction = new RemovePictureAction(command.Id, actionEnv);
        }

        public void OnVisitCallEventCommand(CallWolfEventCommand command)
        {
            // 未実装
            generatedAction = new ActionBase();
        }
    }
}