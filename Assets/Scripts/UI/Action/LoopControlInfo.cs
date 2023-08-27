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

        // ループ制御のための情報を保持
        public bool IsInfiniteLoop { get; private set; }

        public int MaxLoopCount { get; private set; }

        public int CurrentLoopCount { get; private set; }

        public LoopControlInfo(int indentDepth, bool isInfiniteLoop, int maxLoopCount)
        {
            IndentDepth = indentDepth;
            LoopStartPos = -1;

            IsInfiniteLoop = isInfiniteLoop;
            MaxLoopCount = maxLoopCount;
            CurrentLoopCount = 0;
        }

        public void InitializePosition(int loopStartPos)
        {
            LoopStartPos = loopStartPos;
        }

        public void RecordLoopExecution()
        {
            if (LoopStartPos == -1)
            {
                throw new System.Exception("開始位置を初期化していないのに実行されるのは実装ミス");
            }

            CurrentLoopCount++;
        }

        public bool IsExecutable()
        {
            // 無限ループか、ループ回数が上限に達するまでしか実行できない
            return IsInfiniteLoop || CurrentLoopCount < MaxLoopCount;
        }
    }
}
