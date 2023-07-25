using System.Collections.Generic;
using Expression.Event;
using Expression.Map.MapEvent;
using UnityEngine;

namespace UI.Action
{
    /// <summary>
    /// 入れ子になったアクションの制御情報を管理するクラス
    /// </summary>
    public class ActionControl
    {
        public int CurrentActNo { get; private set; }

        public CommandLabel SkipLabel { get; private set; }

        public Stack<int> LoopStartNos { get; private set; }

        private TransitType transitType;

        public ActionControl()
        {
            Initialize();
        }

        /// <summary>
        /// 設定を初期化
        /// </summary>
        public void Initialize()
        {
            CurrentActNo = 0;
            SkipLabel = null;
            LoopStartNos = new Stack<int>();
            transitType = TransitType.Sequential;
        }

        /// <summary>
        /// 特定のアクションまでスキップするための予約を行う
        /// </summary>
        public void ReserveSkip(CommandLabel label)
        {
            if (label == null)
            {
                return;
            }

            transitType = TransitType.Jump;
            SkipLabel = label;
        }

        public void SetLoopStart()
        {
            LoopStartNos.Push(CurrentActNo);
        }

        public void ReserveLoopTopJump()
        {
            transitType = TransitType.LoopTop;
        }

        /// <summary>
        /// 特定のアクションまでスキップするための予約を行う
        /// </summary>
        public void ReserveLoopBreak(CommandLabel label)
        {
            if (label == null)
            {
                return;
            }

            LoopStartNos.Pop();
            transitType = TransitType.Jump;
            SkipLabel = label;
        }

        public void TransitToNext(EventCommandBase[] commands)
        {
            switch (transitType)
            {
                case TransitType.Sequential:
                    {
                        CurrentActNo++;
                    }
                    break;
                case TransitType.Jump:
                    {
                        // スキップ要求があるのでラベルまでスキップさせる
                        // 基本は次以降のアクションを優先するが、存在しない場合は通常通り進める
                        int nextActNo = CurrentActNo;
                        for (int i = 1; i < commands.Length; i++)
                        {
                            int now = (CurrentActNo + i) % commands.Length;
                            if (commands[now].VerifyLabel(SkipLabel))
                            {
                                nextActNo = now;
                                break;
                            }
                        }
                        if (nextActNo == CurrentActNo)
                        {
                            // 遷移先を見つけられなかったので次のアクションに遷移する
                            CurrentActNo++;
                        }
                        else
                        {
                            // ラベルの次のアクションを実行
                            CurrentActNo = nextActNo + 1;
                        }
                        SkipLabel = null;
                    }
                    break;
                case TransitType.LoopTop:
                    {
                        if (LoopStartNos.Count == 0)
                        {
                            throw new System.Exception("ループ内にいないのは想定外");
                        }

                        CurrentActNo = LoopStartNos.Peek();
                    }
                    break;
            }

            // 遷移後はシーケンシャル進行に戻る
            transitType = TransitType.Sequential;
        }

        private enum TransitType
        {
            Sequential,
            Jump,
            LoopTop,
        }
    }
}
