using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// マップ情報を読み出すためのインターフェース
    /// </summary>
    public interface IMapDataRepository : IRepository<MapData, MapId>
    {
        public int GetCount();
    }
}
