using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGの可変DBを読み出すためのリポジトリ
    /// </summary>
    public class WolfPlayDataRepository : IPlayDataRepository
    {
        public DataCategory Find(CategoryId id)
        {
            return null;
        }

        public DataNode<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public DataNode<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }
    }
}
