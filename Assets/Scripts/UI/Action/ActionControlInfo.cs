
namespace UI.Action
{
    /// <summary>
    /// 入れ子になったアクションの制御情報を管理するクラス
    /// </summary>
    public class ActionControlInfo
    {
        public bool IsSkipMode { get; private set; }

        public ActionLabel SkipLabel { get; private set; }

        public ActionControlInfo()
        {
            IsSkipMode = false;
            SkipLabel = null;
        }

        /// <summary>
        /// 特定のアクションまでスキップするための予約を行う
        /// </summary>
        public void ReserveJump(ActionLabel label)
        {
            if (label == null)
            {
                return;
            }

            IsSkipMode = true;
            SkipLabel = label;
        }
    }
}
