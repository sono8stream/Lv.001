using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class ShowPictureCommand : EventCommandBase
    {
        public string FilePath { get; private set; }
        public PicturePivotPattern PivotPattern { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }

        public ShowPictureCommand(string filePath, PicturePivotPattern posPattern, int x, int y)
        {
            FilePath = String.Copy(filePath);
            PivotPattern = posPattern;
            X = x;
            Y = y;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitShowPictureCommand(this);
        }
    }

    public enum PicturePivotPattern
    {
        LeftTop,
        Center,
        LeftBottom,
        RightTop,
        RightBottom,
        CenterTop,
        CenterBottom,
    }
}
