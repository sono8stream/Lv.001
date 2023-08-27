using Expression.Event;

namespace UI.Action
{
    class LoopStartAction : ActionBase
    {
        ActionControl controlInfo;
        LoopControlInfo loopControlInfo;

        public LoopStartAction(ActionControl controlInfo, LoopControlInfo loopControlInfo)
        {
            this.controlInfo = controlInfo;
            this.loopControlInfo = loopControlInfo;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            controlInfo.StartLoop(loopControlInfo);
            return true;
        }
    }
}
