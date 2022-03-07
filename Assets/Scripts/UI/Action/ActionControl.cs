using System.Collections.Generic;

namespace UI.Action
{
    /// <summary>
    /// 入れ子になったアクションの制御情報を管理するクラス
    /// </summary>
    public class ActionControl
    {
        public int CurrentActNo { get; private set; }

        public bool IsSkipMode { get; private set; }

        public ActionLabel SkipLabel { get; private set; }

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
        public void ReserveSkip(ActionLabel label)
        {
            if (label == null)
            {
                return;
            }

            IsSkipMode = true;
            SkipLabel = label;
        }


        public void TransitToNext(List<ActionBase> actions)
        {
            if (IsSkipMode)
            {
                // スキップ要求があるのでラベルまでスキップさせる
                // 基本は次以降のアクションを優先するが、存在しない場合は通常通り進める
                int nextActNo = CurrentActNo;
                for (int i = 1; i < actions.Count; i++)
                {
                    int now = (CurrentActNo + i) % actions.Count;
                    if (actions[now].VerifyLabel(SkipLabel))
                    {
                        nextActNo = now;
                        break;
                    }
                }
                if (nextActNo == CurrentActNo)
                {
                    CurrentActNo++;
                }
                else
                {
                    CurrentActNo = nextActNo;
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
