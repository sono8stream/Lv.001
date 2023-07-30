using System;
using System.Collections.Generic;
using UnityEngine;
using Expression.Event;

namespace UI.Action
{
    class CloseChoiceAction : ActionBase
    {
        bool isCloseAll;
        ActionEnvironment actionEnv;
        ActionControl controlInfo;

        public CloseChoiceAction(ActionEnvironment actionEnv,bool isCloseAll,ActionControl controlInfo)
        {
            this.actionEnv = actionEnv;
            this.isCloseAll = isCloseAll;
            this.controlInfo = controlInfo;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            List<GameObject> choiceBoxes = actionEnv.choiceBoxes; 
            if (isCloseAll)
            {
                for (int i = choiceBoxes.Count - 1; i >= 0; i--)
                {
                    UnityEngine.Object.Destroy(choiceBoxes[i]);
                    choiceBoxes.RemoveAt(i);
                }
            }
            else if (choiceBoxes.Count > 0)
            {
                int boxNo = choiceBoxes.Count - 1;
                GameObject sub = choiceBoxes[boxNo];
                UnityEngine.Object.Destroy(sub);
                choiceBoxes.RemoveAt(boxNo);
            }

            CommandLabel label = new CommandLabel(actionEnv.ChoiceName);
            controlInfo.ReserveJump(label);
            return true;
        }
    }
}
