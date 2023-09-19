using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Expression.Map.MapEvent
{
    public class ChangeVariableIntCommand : EventCommandBase
    {
        public VariableUpdater[] Updaters { get; private set; }

        public ChangeVariableIntCommand(int indentDepth, VariableUpdater[] updaters) : base(indentDepth)
        {
            Updaters = updaters;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitChangeVariableIntCommand(this);
        }
    }
}