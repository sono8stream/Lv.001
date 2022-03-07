using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    // �y�b��zStackEventsTo���ړ���͏�Ԃ������Ȃ��̂ŃC���^�[�t�F�[�X��
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            // ���͉������Ȃ�
            visitor.OnVisitBaseCommand(this);
        }
    }
}