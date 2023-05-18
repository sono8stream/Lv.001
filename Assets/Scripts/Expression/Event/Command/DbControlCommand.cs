using System;
using System.Collections.Generic;
using Expression.Map.MapEvent;

namespace Expression.Event.Command
{
    public class DbControlCommand : EventCommandBase
    {
        public UpdaterInt Updater { get; private set; }

        public DbControlCommand()
        {
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitDbControlCommand(this);
        }
    }
}
