using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    // �y�b��zStackEventsTo���ړ���͏�Ԃ������Ȃ��̂ŃC���^�[�t�F�[�X��
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        // �y�b��z�����܂�Unity���̏�����UI�w�ɒu���A����̃G���W���Ɉˑ����鏈���͎������Ȃ�
        public virtual void StackEventsTo(List<UnityEvent> events, EventCommands commands)
        {
            // ���͉������Ȃ�
            events.Add(new UnityEvent());
            events[events.Count - 1].AddListener(() => commands.NoOperation());
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            // ���͉������Ȃ�
        }
    }
}