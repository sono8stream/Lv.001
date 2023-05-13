using UnityEngine;

namespace UI.Action
{
    class WaitForInputAction : ActionBase
    {

        public WaitForInputAction()
        {
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            return Input.GetMouseButtonUp(0) || Input.GetKeyDown(KeyCode.Z);
        }
    }
}
