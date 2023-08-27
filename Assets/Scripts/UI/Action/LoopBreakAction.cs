using Expression.Event;

namespace UI.Action
{
    class LoopBreakAction : ActionBase
    {
        ActionControl controlInfo;

        public LoopBreakAction(ActionControl controlInfo)
        {
            this.controlInfo = controlInfo;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // ループから強制的に抜ける処理
            // 【暫定】本来なら制御も含めAction内に完結させたい。
            controlInfo.ReserveLoopBreak();
            return true;
        }
    }
}
