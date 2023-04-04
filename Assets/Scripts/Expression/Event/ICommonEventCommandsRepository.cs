using System;
using Util.Wolf;
using UnityEngine;

namespace Expression.Event
{
    /// <summary>
    /// コモンイベントのコマンド群を呼び出すためのインターフェース
    /// </summary>
    public interface ICommonEventCommandsRepository
    {
        public CommonEvent GetEvent(int commonEventId);

        public int GetCount();
    }
}