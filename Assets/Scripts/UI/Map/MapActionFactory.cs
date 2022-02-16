using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Expression;
using UI.Action;
using Expression.Map.MapEvent;

namespace UI.Map
{
    public class MapActionFactory:Expression.Map.MapEvent.ICommandVisitor
    {
        // �������ʂ�ێ����邽�߂Ƀ����o�Ƃ��Ď��B�r���ɒ���
        private ActionBase generatedAction;

        private EventCommands commands;

        public MapActionFactory(EventCommands commands)
        {
            this.commands = commands;
        }

        public ActionBase CreateActionFrom(EventCommandBase command)
        {
            command.Visit(this);
            return generatedAction;
        }

        public void OnVisitMessageCommand(MessageCommand command)
        {
            List<ActionBase> actions = new List<ActionBase>();
            actions.Add(new ShowMessageAction(command.MessageText, commands));
            actions.Add(new WaitForInputAction());
            actions.Add(new CloseMessageAction(commands, false));

            generatedAction = new MultiAction(actions);
        }
    }
}