using System.Collections.Generic;
using UnityEngine;

namespace UI.Action
{
    class CloseMessageAction : ActionBase
    {
        bool isCloseAll;

        ActionEnvironment commands; 

        public CloseMessageAction(ActionEnvironment commands, bool isCloseAll)
        {
            this.commands = commands;
            this.isCloseAll = isCloseAll;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            List<GameObject> windows = commands.windows;
            if (isCloseAll)
            {
                for (int i = windows.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Object.Destroy(windows[i]);
                    windows.RemoveAt(i);
                }
            }
            else if (windows.Count > 0)
            {
                int winNo = windows.Count - 1;
                UnityEngine.Object.Destroy(windows[winNo]);
                windows.RemoveAt(winNo);
            }
            // 【暫定】セルフ変数処理を準備できたら有効にする
            /*
            if (once)
            {
                SetSelfVar();
            }
            */
            return true;
        }
    }
}
