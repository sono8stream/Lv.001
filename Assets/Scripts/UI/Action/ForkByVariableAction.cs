﻿using Expression.Event;

namespace UI.Action
{
    class ForkByVariableIntAction : ActionBase
    {
        ActionControl controlInfo;
        int indentDepth;
        Expression.Map.MapEvent.ConditionInt[] conditions;
        Expression.Map.MapEvent.CommandVisitContext context;

        public ForkByVariableIntAction(ActionControl controlInfo, int indentDepth,
            Expression.Map.MapEvent.ConditionInt[] conditions,
            Expression.Map.MapEvent.CommandVisitContext context)
        {
            this.controlInfo = controlInfo;
            this.indentDepth = indentDepth;
            this.conditions = conditions;
            this.context = context;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            int forkId = -1;
            for (int i = 0; i < conditions.Length; i++)
            {
                if (conditions[i].CheckIsTrue(context))
                {
                    forkId = i + 1;
                    break;// 最初に満たした条件分岐に飛ぶのが一般的。Wolfも同様。
                }
            }

            CommandLabel label = new CommandLabel($"{indentDepth}.{forkId}");
            controlInfo.ReserveJump(label);
            return true;
        }
    }
}
