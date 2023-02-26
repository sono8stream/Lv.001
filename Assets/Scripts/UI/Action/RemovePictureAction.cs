using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Expression.Map.MapEvent.Command;

namespace UI.Action
{
    class RemovePictureAction : ActionBase
    {
        int pictureId;

        ActionEnvironment actionEnv;

        public RemovePictureAction(int pictureId, ActionEnvironment actionEnv)
        {
            this.pictureId = pictureId;

            this.actionEnv = actionEnv;
        }

        /// <inheritdoc/>
        public override bool Run()
        {
            actionEnv.RemoveImage(pictureId);

            return true;
        }
    }
}
