using System.Collections.Generic;
using Expression.Event;
using Expression.Map.MapEvent;
using UnityEngine;

namespace UI.Action
{
    /// <summary>
    /// ループの制御に必要な情報を管理するクラス
    /// </summary>
    public class LoopControlInfo
    {
        public int IndentDepth { get; private set; }

        public int LoopStartPos { get; private set; }

        public LoopControlInfo(int indentDepth, int loopStartPos)
        {
            IndentDepth = indentDepth;
            LoopStartPos = loopStartPos;
        }
    }
}
