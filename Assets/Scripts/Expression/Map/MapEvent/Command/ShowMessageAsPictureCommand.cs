using System;
using System.Collections.Generic;
using Expression.Map.MapEvent.CommandFactory;

namespace Expression.Map.MapEvent.Command
{
    public class ShowMessageAsPictureCommand : EventCommandBase
    {
        public int Id { get; private set; }
        public IStringFactory MessageFactory { get; private set; }
        public PicturePivotPattern PivotPattern { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public float Scale { get; private set; }

        public ShowMessageAsPictureCommand(int indentDepth, int id, string messageRaw,
            PicturePivotPattern posPattern, int x, int y, float scale) : base(indentDepth)
        {
            Id = id;
            MessageFactory = new WolfStringFactory(messageRaw);
            PivotPattern = posPattern;
            X = x;
            Y = y;
            Scale = scale;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitShowMessageAsPictureCommand(this);
        }
    }
}
