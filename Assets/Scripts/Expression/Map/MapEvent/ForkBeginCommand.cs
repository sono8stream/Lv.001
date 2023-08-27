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
        /// 自分自身が指定されたラベルと対応しているかを判定
        /// </summary>
        /// <param name="label">チェックしたいラベル</param>
        /// <returns></returns>
        public override bool VerifyLabel(CommandLabel label)
        {
            return LabelString == label.LabelName;
        }
    }
}