using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class RemovePictureCommand : EventCommandBase
    {
        public int Id { get; private set; }

        public RemovePictureCommand(int indentDepth, int id) : base(indentDepth)
        {
            Id = id;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitRemovePictureCommand(this);
        }
    }
}
