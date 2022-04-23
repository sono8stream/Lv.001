
namespace UI.Action
{
    /// <summary>
    /// 分岐始点を示すアクション
    /// </summary>
    public class ForkEndAction : ActionBase
    {
        string labelString;

        public ForkEndAction(string labelString)
        {
            this.labelString = labelString;
        }

        /// <summary>
        /// 自分自身が指定されたラベルと対応しているかを判定
        /// </summary>
        /// <param name="label">チェックしたいラベル</param>
        /// <returns></returns>
        public override bool VerifyLabel(ActionLabel label)
        {
            return labelString == label.LabelName;
        }
    }
}
