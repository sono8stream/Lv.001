using System;
using UnityEngine;

namespace Expression.Map
{
    [Serializable]
    public class Hd2dTileInfo
    {
        public Vector3 offset;
        public MapBlockType type;
        public Hd2d.NeighborConstraintDict neighborConstraints;

        public Hd2dTileInfo(Vector3 offset, MapBlockType type,
            Hd2d.NeighborConstraintDict neighborConstraints)
        {
            this.offset = offset;
            this.type = type;
            this.neighborConstraints = neighborConstraints;
        }
    }
}