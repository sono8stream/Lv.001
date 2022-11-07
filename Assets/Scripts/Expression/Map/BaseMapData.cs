using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expression.Map
{
    public class BaseMapData
    {
        public MapId Id { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        // �y�b��z�e�`�b�v�̔ԍ����������`�b�v����ێ�����悤�ɏC��
        public MovableInfo[,] MovableGrid { get; private set; }

        public MapEvent.EventData[] EventDataArray { get; private set; }

        public BaseMapData(MapId id, int width, int height,
         MovableInfo[,] movableGrid, MapEvent.EventData[] eventDataArray)
        {
            Id = id;
            Width = width;
            Height = height;
            MovableGrid = movableGrid;
            EventDataArray = eventDataArray;
        }
    }
}
