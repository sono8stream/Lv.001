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
    class ForkByVariableIntAction : ActionBase
    {
        ActionControl controlInfo;
        int indentDepth;
        Expression.Map.MapEvent.ConditionInt[] conditions;

        public ForkByVariableIntAction(ActionControl controlInfo, int indentDepth,
            Expression.Map.MapEvent.ConditionInt[] conditions)
        {
            this.controlInfo = controlInfo;
            this.indentDepth = indentDepth;
            this.conditions = conditions;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            int forkId = -1;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].CheckIsTrue())
                {
                    forkId = i + 1;
                }
            }

            ActionLabel label = new ActionLabel($"{indentDepth}.{forkId}");
            controlInfo.ReserveSkip(label);
            return true;
        }
    }
}
