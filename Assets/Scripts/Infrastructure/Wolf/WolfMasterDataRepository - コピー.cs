using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのシステム変数を読み出すためのリポジトリ
    /// </summary>
    public class WolfMasterDataRepository : IMasterDataRepository
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
