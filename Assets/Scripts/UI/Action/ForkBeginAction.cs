
namespace UI.Action
{
    /// <summary>
    /// 分岐始点を示すアクション
    /// </summary>
    public class ForkBeginAction : ActionBase
    {
        ActionControl controlInfo;
        int indentDepth;
        string labelString;

        public ForkBeginAction(ActionControl controlInfo, int indentDepth, 
            string labelString)
        {
            this.controlInfo = controlInfo;
            this.indentDepth = indentDepth;
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

        /// <inheritdoc/>
        public override bool Run()
        {
            // 前段の分岐ブロックから遷移してきたので，分岐終端までスキップする
            ActionLabel label = new ActionLabel($"{indentDepth}.{0}");
            controlInfo.ReserveSkip(label);

            return base.Run();
        }
    }
}
