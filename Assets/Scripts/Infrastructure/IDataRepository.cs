using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Data;

namespace Infrastructure
{
    /// <summary>
    /// ゲームの詳細な仕様に関するデータを読み出すためのリポジトリのインターフェース
    /// </summary>
    public interface IDataRepository : IRepository<DataRecord, RecordId>
    {
        public DataField<int> FindInt(DataRef dataRef);

        public DataField<string> FindString(DataRef dataRef);
    }
}
