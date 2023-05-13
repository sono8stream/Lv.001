using System.Collections.Generic;
using Expression.Event;
using Expression.Map.MapEvent;

namespace UI.Action
{
    /// <summary>
    /// 入れ子になったアクションの制御情報を管理するクラス
    /// </summary>
    public class ActionControl
    {
        public int CurrentActNo { get; private set; }

        public bool IsSkipMode { get; private set; }

        public CommandLabel SkipLabel { get; private set; }

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
            IsSkipMode = false;
            SkipLabel = null;
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

            IsSkipMode = true;
            SkipLabel = label;
        }

        public void TransitToNext(EventCommandBase[] commands)
        {
            if (IsSkipMode)
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

                IsSkipMode = false;
                SkipLabel = null;
            }
            else
            {
                CurrentActNo++;
            }
        }
    }
}
