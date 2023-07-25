using System;
using System.Collections.Generic;
using Expression.Event;

namespace Expression.Map.MapEvent
{
    public class ForkEndCommand : EventCommandBase
    {
        public string LabelString { get; private set; }

        public ForkEndCommand(int indent) : base(indent)
        {
            LabelString = $"{indent}.{0}";
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitForkEndCommand(this);
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