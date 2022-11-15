using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Action
{
    class MovePositionAction : ActionBase
    {
        ActionEnvironment actionEnv;

        public MovePositionAction()
        {
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            // 同じマップ内か判定
            // マップが異なる場合はマップの読み込みを実施
            return true;
        }
    }
}
