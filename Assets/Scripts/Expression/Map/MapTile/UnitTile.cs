using System;
using System.Collections.Generic;

namespace Expression.Map.MapTile
{
    class UnitTile
    {
        public MovableType MovableTypeValue { get; private set; }

        public Dictionary<DirectionType, bool> IsCrossDict { get; private set; }

        public bool IsCounter { get; private set; }

        public int TagNumber { get; private set; }

        public UnitTile(MovableType movableTypeValue, Dictionary<DirectionType, bool> isCrossDict, bool isCounter, int tagNumber)
        {
            MovableTypeValue = movableTypeValue;
            IsCrossDict = isCrossDict;
            IsCounter = isCounter;
            TagNumber = tagNumber;
        }
    }

    // 通行許可設定
    enum MovableType
    {

    }

    enum DirectionType
    {
        Up,
        Right,
        Down,
        Left
    }
}