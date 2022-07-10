using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expression.Map
{
    public class Hd2dMapData
    {
        public MapId Id { get; private set; }

        public GameObject BaseObject { get; private set; }

        public Hd2dBlock[] Blocks { get; private set; }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public MovableInfo[,] MovableGrid { get; private set; }

        public MapEvent.EventData[] EventDataArray { get; private set; }

        public Hd2dMapData(MapId id, Hd2dBlock[] blocks, int width, int height,
         MovableInfo[,] movableGrid, MapEvent.EventData[] eventDataArray)
        {
            Id = id;
            Blocks = blocks;
            Width = width;
            Height = height;
            MovableGrid = movableGrid;
            EventDataArray = eventDataArray;

            BaseObject = new GameObject("Hd2dMap");
            BaseObject.transform.position = Vector3.zero;
            foreach (Hd2dBlock block in Blocks)
            {
                block.transform.SetParent(BaseObject.transform);
            }
        }
    }
}
