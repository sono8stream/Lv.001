using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Expression.Map
{
    public class Hd2dMapData:BaseMapData
    {
        public GameObject BaseObject { get; private set; }

        public Hd2dBlock[] Blocks { get; private set; }

        public Hd2dMapData(MapId id, Hd2dBlock[] blocks, int width, int height,
         MovableInfo[,] movableGrid, MapEvent.EventData[] eventDataArray)
            :base(id, width, height, movableGrid, eventDataArray)
        {
            Blocks = blocks;

            BaseObject = new GameObject("Hd2dMap");
            BaseObject.transform.position = Vector3.zero;
            foreach (Hd2dBlock block in Blocks)
            {
                block.transform.SetParent(BaseObject.transform);
            }
        }
    }
}
