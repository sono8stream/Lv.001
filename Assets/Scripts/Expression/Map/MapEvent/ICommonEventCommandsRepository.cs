using System;
using Util.Wolf;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    /// <summary>
    /// コモンイベントのコマンド群を呼び出すためのインターフェース
    /// </summary>
    public interface ICommonEventCommandsRepository
    {
        public EventCommandBase[] GetCommands(int commonEventId);
    }
}