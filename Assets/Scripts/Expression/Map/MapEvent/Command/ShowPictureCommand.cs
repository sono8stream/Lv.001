﻿using System;
using System.Collections.Generic;
using Expression.Map.MapEvent.CommandFactory;

namespace Expression.Map.MapEvent.Command
{
    public class ShowPictureCommand : EventCommandBase
    {
        public int Id { get; private set; }
        public IStringFactory FilePathFactory { get; private set; }
        public PicturePivotPattern PivotPattern { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public float Scale { get; private set; }

        public ShowPictureCommand(int indentDepth, int id, string filePath,
            PicturePivotPattern posPattern, int x, int y, float scale) : base(indentDepth)
        {
            Id = id;
            FilePathFactory = new WolfStringFactory(filePath);
            PivotPattern = posPattern;
            X = x;
            Y = y;
            Scale = scale;
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
