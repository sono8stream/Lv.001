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

        public Stack<LoopControlInfo> LoopControlInfos { get; private set; }

        // 本来は遷移ロジックをクラス化してストラテジっぽくするのが良いのだろう
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
            LoopControlInfos = new Stack<LoopControlInfo>();
            transitType = TransitType.Sequential;
        }

        /// <summary>
        /// 特定のアクションまでジャンプするための予約を行う
        /// </summary>
        public void ReserveJump(CommandLabel label)
        {
            if (label == null)
            {
                return;
            }

            transitType = TransitType.Jump;
            SkipLabel = label;
        }

        public void StartLoop(LoopControlInfo loopControlInfo)
        {
            loopControlInfo.InitializePosition(CurrentActNo);
            LoopControlInfos.Push(loopControlInfo);
            transitType = TransitType.DoLoop;// ループ初回実行
        }

        /// <summary>
        /// ループ突入フローに入る
        /// </summary>
        public void ReserveDoLoop()
        {
            transitType = TransitType.DoLoop;
        }

        /// <summary>
        /// ループ処理の終端までジャンプする遷移を予約
        /// </summary>
        public void ReserveLoopBreak(int indentDepth)
        {
            transitType = TransitType.LoopBreak;
        }

        public void TransitToNext(in EventCommandBase[] commands)
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
                case TransitType.DoLoop:
                    {
                        // ループに突入できるならループ先頭に戻る。だめならループを抜ける
                        if (LoopControlInfos.Count == 0)
                        {
                            throw new System.Exception("ループ内にいないのは想定外");
                        }

                        var info = LoopControlInfos.Peek();
                        if (info.IsExecutable())
                        {
                            info.RecordLoopExecution();// ループ開始したことを記録しておく
                            CurrentActNo = info.LoopStartPos + 1;
                        }
                        else
                        {
                            // ループブレークと同じ処理
                            BreakLoop(commands);
                        }
                    }
                    break;
                case TransitType.LoopBreak:
                    {
                        BreakLoop(commands);
                    }
                    break;
            }

            // 遷移後はシーケンシャル進行に戻る
            transitType = TransitType.Sequential;
        }

        private void BreakLoop(in EventCommandBase[] commands)
        {
            // 次に同じインデント深さになる位置までジャンプする
            int jumpActNo = -1;

            var loopInfo = LoopControlInfos.Peek();
            for (int i = loopInfo.LoopStartPos + 1; i < commands.Length; i++)
            {
                if (commands[i].IndentDepth == loopInfo.IndentDepth)
                {
                    jumpActNo = i;
                    break;
                }
            }
            if (jumpActNo == -1)
            {
                throw new System.Exception("インデントが閉じられていないのは想定外");
            }

            // ジャンプするのはループ末尾の次のコマンド
            CurrentActNo = jumpActNo + 1;
            LoopControlInfos.Pop();
        }

        private enum TransitType
        {
            Sequential,
            Jump,
            DoLoop,
            LoopBreak,
        }
    }
}
