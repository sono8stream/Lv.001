using System;
using System.Collections.Generic;
using Expression.Event;

namespace Expression.Map.MapEvent
{
    public class EventCommandBase
    {

        public EventCommandBase()
        {
        }

        public virtual void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitBaseCommand(this);
        }

        /// <summary>
        /// �������g���w�肳�ꂽ���x���ƑΉ����Ă��邩�𔻒�
        /// </summary>
        /// <param name="label">�`�F�b�N���������x��</param>
        /// <returns></returns>
        public virtual bool VerifyLabel(CommandLabel label)
        {
            return false;
        }
    }
}