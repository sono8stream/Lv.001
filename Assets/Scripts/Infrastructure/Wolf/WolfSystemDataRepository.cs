using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのシステムDBを読み出すためのリポジトリ
    /// 【暫定】WolfRPG共通の基底のデータリポジトリクラスを作り、共通化する
    /// </summary>
    public class WolfSystemDataRepository : ISystemDataRepository
    {
        public DataRecord Find(RecordId id)
        {
            return null;
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }
    }
}
