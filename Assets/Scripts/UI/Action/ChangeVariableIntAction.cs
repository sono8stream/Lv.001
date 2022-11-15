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
    class ChangeVariableIntAction : ActionBase
    {
        Expression.Map.MapEvent.UpdaterInt[] updaters;

        public ChangeVariableIntAction(Expression.Map.MapEvent.UpdaterInt[] updaters)
        {
            this.updaters = updaters;
        }

        /// <inheritdoc/>i
        public override bool Run()
        {
            for (int i = 0; i < updaters.Length; i++)
            {
                updaters[i].Update();
            }

            return true;
        }
    }
}
