using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Infrastructure.Map
{
    public interface IMapDataRegistry
    {
        public MapData Find(int index);
    }
}
