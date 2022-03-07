﻿using System.Collections.Generic;

namespace UI.Action
{
    /// <summary>
    /// アクションを複数個まとめたアクション
    /// Compositeパターンのようにふるまう
    /// </summary>
    public class MultiAction : ActionBase
    {
        List<ActionBase> actions;
        int currentActNo;

        public MultiAction(List<ActionBase> actions)
        {
            this.actions = actions;
        }

        /// <inheritdoc/>
        public override void onStart()
        {
            currentActNo = 0;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            if (actions[currentActNo].Run())
            {
                currentActNo++;
            }

            if (currentActNo < actions.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}