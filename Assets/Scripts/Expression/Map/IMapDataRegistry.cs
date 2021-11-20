using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Expression;

namespace Expression.Map
{
    public interface IMapDataRegistry
    {
        public MapData Find(MapId id);
    }
}
