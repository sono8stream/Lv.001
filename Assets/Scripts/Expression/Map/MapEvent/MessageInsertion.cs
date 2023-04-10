using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent
{
    /// <summary>
    /// メッセージに変数を埋め込むための基底クラス。
    /// 派生先で必要に応じて情報を仕込み、取り出せるようにしておく
    /// </summary>
    public abstract class MessageInsertionBase : IComparable<MessageInsertionBase>
    {
        private Common.IDataAccessorFactory<int> intFactory;
        private Common.IDataAccessorFactory<string> stringFactory;
        private int insertPosition;

        public MessageInsertionBase(int insertPosition)
        {
            this.intFactory = intFactory;
            this.stringFactory = stringFactory;
            this.insertPosition = insertPosition;
        }

        public abstract string GetText(CommandVisitContext context);

        public int CompareTo(MessageInsertionBase other)
        {
            return other.insertPosition.CompareTo(insertPosition);
        }
    }
}