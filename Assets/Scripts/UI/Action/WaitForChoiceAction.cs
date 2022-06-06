using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Action
{
    class WaitForChoiceAction : ActionBase
    {

        ActionEnvironment actionEnv;

        public WaitForChoiceAction(ActionEnvironment actionEnv)
        {
            this.actionEnv = actionEnv;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // 【暫定】選択肢が空白の場合に正常動作しないので直す
            return !actionEnv.ChoiceName.Equals("");
        }
    }
}
