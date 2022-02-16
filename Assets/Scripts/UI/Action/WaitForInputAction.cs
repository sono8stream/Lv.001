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
            return Input.GetMouseButtonDown(0);
        }
    }
}
