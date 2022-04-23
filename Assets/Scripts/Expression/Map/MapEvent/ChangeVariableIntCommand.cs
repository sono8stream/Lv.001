using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ChangeVariableIntCommand : EventCommandBase
    {
        public UpdaterInt[] Updaters { get; private set; }

        public ChangeVariableIntCommand(UpdaterInt[] updaters)
        {
            Updaters = updaters;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitChangeVariableIntCommand(this);
        }
    }
}