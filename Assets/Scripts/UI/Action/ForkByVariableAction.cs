using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Expression;

namespace UI.Action
{
    class ForkByVariableAction<T> : ActionBase
    {
        int indentDepth;
        Expression.Map.MapEvent.Condition<T>[] conditions;
        ActionControl controlInfo;

        public ForkByVariableAction(int indentDepth,ActionControl controlInfo)
        {
            this.indentDepth = indentDepth;
            this.controlInfo = controlInfo;
        }

        /// <inheritdoc/>
        public override bool Run()
        {

            ActionLabel label = new ActionLabel($"{indentDepth}.{0}");
            controlInfo.ReserveSkip(label);
            return true;
        }
    }
}
