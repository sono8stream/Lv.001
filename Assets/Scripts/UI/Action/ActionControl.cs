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
                for (int i = 0; i < actions.Count; i++)
                {
                    if (actions[i].VerifyLabel(SkipLabel))
                    {
                        CurrentActNo = i;
                        break;
                    }
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
