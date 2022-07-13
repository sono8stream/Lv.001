using System;
using UnityEngine;

namespace Expression.Map
{
    public class Hd2dTileInfo
    {
        public Vector3 Offset { get; private set; }

        public Hd2dTileInfo(Vector3 offset)
        {
            Offset = offset;
        }
    }
}