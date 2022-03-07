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
        // �������ʂ�ێ����邽�߂Ƀ����o�Ƃ��Ď��B�r���ɒ���
        private ActionBase generatedAction;

        private ActionEnvironment actionEnv;

        public MapActionFactory(ActionEnvironment actionEnv)
        {
            this.actionEnv = actionEnv;
        }

        /// <summary>
        /// ��A�̃R�}���h����}���`�A�N�V�����𐶐����ĕԂ��܂�
        /// �P��ł͂Ȃ���A�̃R�}���h��p����͕̂���Ȃǂ����q�ɂȂ����A�N�V�����Ƃ��Đ������邽��
        /// </summary>
        /// <param name="commands">�C�x���g�Ɋ܂܂���A�̃R�}���h</param>
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
            // ��A�̕����A�N�V�����������ɋl�߂ĕԂ�

        }
    }
}