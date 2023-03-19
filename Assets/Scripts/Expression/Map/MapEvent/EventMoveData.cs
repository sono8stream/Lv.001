using System;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    /// <summary>
    /// Eventの移動設定を保持
    /// 【暫定】必要なデータを詰める
    /// </summary>
    public class EventMoveData
    {
        public bool CanPass { get; private set; }

        public EventMoveData(bool canPass)
        {
            CanPass = canPass;
        }
    }
}