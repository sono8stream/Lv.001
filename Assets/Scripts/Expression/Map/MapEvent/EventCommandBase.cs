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
        /// 自分自身が指定されたラベルと対応しているかを判定
        /// </summary>
        /// <param name="label">チェックしたいラベル</param>
        /// <returns></returns>
        public virtual bool VerifyLabel(CommandLabel label)
        {
            return false;
        }
    }
}