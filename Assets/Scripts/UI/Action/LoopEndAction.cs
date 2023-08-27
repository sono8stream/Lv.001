using Expression.Event;

namespace UI.Action
{
    class LoopEndAction : ActionBase
    {
        ActionControl controlInfo;

        public LoopEndAction(ActionControl controlInfo)
        {
            this.controlInfo = controlInfo;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // ループに入りなおす制御
            // 【暫定】本来なら制御も含めAction内に完結させたい。
            controlInfo.ReserveDoLoop();
            return true;
        }
    }
}
