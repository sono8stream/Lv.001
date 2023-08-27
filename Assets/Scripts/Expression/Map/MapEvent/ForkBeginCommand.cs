using System;
using System.Collections.Generic;
using Expression.Event;

namespace Expression.Map.MapEvent
{
    public class ForkBeginCommand : EventCommandBase
    {
        public string LabelString { get; private set; }

        public ForkBeginCommand(int indent, int choiceNo) : base(indent)
        {
            LabelString = $"{indent}.{choiceNo}";
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitForkBeginCommand(this);
        }

        /// <summary>
        /// �������g���w�肳�ꂽ���x���ƑΉ����Ă��邩�𔻒�
        /// </summary>
        /// <param name="label">�`�F�b�N���������x��</param>
        /// <returns></returns>
        public override bool VerifyLabel(CommandLabel label)
        {
            return LabelString == label.LabelName;
        }
    }
}