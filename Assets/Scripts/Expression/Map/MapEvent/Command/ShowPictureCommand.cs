using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class ShowPictureCommand : EventCommandBase
    {
        public string FilePath { get; private set; }
        public PicturePosPattern PosPattern { get; private set; }
        public int PivotX { get; private set; }
        public int PivotY { get; private set; }

        public ShowPictureCommand(string filePath,PicturePosPattern posPattern,
            int pivotX, int pivotY)
        {
            FilePath = String.Copy(filePath);
            PosPattern = posPattern;
            PivotX = pivotX;
            PivotY = pivotY;
        }

        public override void Visit(ICommandVisitor visitor)
        {
            visitor.OnVisitShowPictureCommand(this);
        }
    }

    public enum PicturePosPattern
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
