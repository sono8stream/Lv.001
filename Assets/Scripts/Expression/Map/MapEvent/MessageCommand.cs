using System;
using System.Collections.Generic;
using System.Linq;

namespace Expression.Map.MapEvent
{
    public class MessageCommand : EventCommandBase
    {
        public List<Common.IDataAccessorFactory<string>> StringFactories { get; private set; }

        public MessageCommand(List<Common.IDataAccessorFactory<string>> stringFactories)
        {
            StringFactories = stringFactories;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitMessageCommand(this);
        }

        public List<Common.IDataAccessor<string>> GetAccessors(CommandVisitContext context)
        {
            var accessors = StringFactories.Select(a => a.Create(context)).ToList();
            return accessors;
        }
    }
}