using Expression.Event;

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

        /// <inheritdoc/>
        public override bool Run()
        {
            // 前段の分岐ブロックから遷移してきたので，分岐終端までスキップする
            CommandLabel label = new CommandLabel($"{indentDepth}.{0}");
            controlInfo.ReserveSkip(label);

            return base.Run();
        }
    }
}
