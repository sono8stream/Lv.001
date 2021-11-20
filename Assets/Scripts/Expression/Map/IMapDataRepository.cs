using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Expression;

namespace Expression.Map
{
    public interface IMapDataRepository
    {
        public MapData Find(MapId id);
    }
}
