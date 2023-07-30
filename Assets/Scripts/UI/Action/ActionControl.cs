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

        public Stack<LoopControlInfo> LoopStartNos { get; private set; }

        // 本来は遷移ロジックをクラス化してストラテジっぽくするのが良いのだろう
        private TransitType transitType;
        private int jumpIndentDepth;
        private int jumpSteps;// 分岐で飛び越えるカウント

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
            LoopStartNos = new Stack<LoopControlInfo>();
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

        /// <summary>
        /// 特定のアクションまでスキップする
        /// </summary>
        public void Reserve()
        {

        }

        public void SetLoopStart(int indent)
        {
            LoopStartNos.Push(new LoopControlInfo(indent, CurrentActNo));
        }

        /// <summary>
        /// ループの始点までジャンプし、条件チェックを再度行う
        /// </summary>
        public void ReserveLoopTopJump()
        {
            transitType = TransitType.LoopTop;
        }

        /// <summary>
        /// ループ処理の終端までジャンプする遷移を予約
        /// </summary>
        public void ReserveLoopBreak(int indentDepth)
        {
            LoopStartNos.Pop();
            transitType = TransitType.LoopBreak;
            jumpIndentDepth = indentDepth;
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

                        CurrentActNo = LoopStartNos.Peek().LoopStartPos;
                    }
                    break;
                case TransitType.LoopBreak:
                    {
                        // 次に同じインデント深さになる位置までジャンプする
                        int jumpActNo = -1;
                        for (int i = CurrentActNo + 1; i < commands.Length; i++)
                        {
                            if (commands[i].IndentDepth == jumpIndentDepth)
                            {
                                jumpActNo = i;
                                break;
                            }
                        }
                        if (jumpActNo == -1)
                        {
                            throw new System.Exception("インデントが閉じられていないのは想定外");
                        }

                        CurrentActNo = jumpActNo + 1;
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
            LoopBreak,
        }
    }
}
