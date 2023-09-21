using System;
using System.Collections.Generic;
using Expression.Map.MapEvent.CommandFactory;

namespace Expression.Map.MapEvent.Command
{
    public class ShowMessageAsPictureCommand : EventCommandBase
    {
        public Common.IDataAccessorFactory IdFactory { get; private set; }
        public IStringFactory MessageFactory { get; private set; }
        public PicturePivotPattern PivotPattern { get; private set; }
        public Common.IDataAccessorFactory XFactory { get; private set; }
        public Common.IDataAccessorFactory YFactory { get; private set; }
        public float Scale { get; private set; }

        public ShowMessageAsPictureCommand(int indentDepth, Common.IDataAccessorFactory idFactory,
            string messageRaw, PicturePivotPattern posPattern,
            Common.IDataAccessorFactory xFactory,
            Common.IDataAccessorFactory yFactory, float scale) : base(indentDepth)
        {
            IdFactory = idFactory;
            MessageFactory = new WolfStringFactory(messageRaw);
            PivotPattern = posPattern;
            XFactory = xFactory;
            YFactory = yFactory;
            Scale = scale;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitShowMessageAsPictureCommand(this);
        }
    }
}
