using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;
using UI.Action;
using Expression.Map.MapEvent;

namespace UI.Map
{
    public class MapActionFactory : Expression.Map.MapEvent.ICommandVisitor
    {
        // 生成結果を保持するためにメンバとして持つ。排他に注意
        private ActionBase generatedAction;

        private ActionEnvironment actionEnv;

        public MapActionFactory(ActionEnvironment actionEnv)
        {
            this.actionEnv = actionEnv;
        }

        /// <summary>
        /// 一連のコマンドからマルチアクションを生成して返します
        /// 単一ではなく一連のコマンドを用いるのは分岐などを入れ子になったアクションとして生成するため
        /// </summary>
        /// <param name="commands">イベントに含まれる一連のコマンド</param>
        /// <returns></returns>
        public ActionBase CreateActionFrom(EventCommandBase[] commands)
        {
            List<ActionBase> actions = new List<ActionBase>();
            for (int i = 0; i < commands.Length; i++)
            {
                commands[i].Visit(this);
                actions.Add(generatedAction);
            }
            return new MultiAction(actions);
        }

        public ActionBase CreateActionFrom(EventCommandBase command)
        {
            command.Visit(this);
            return generatedAction;
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

        public void OnVisitChoiceCommand(ChoiceForkCommand command)
        {
            // 一連の分岐先アクションも内部に詰めて返す

        }
    }
}