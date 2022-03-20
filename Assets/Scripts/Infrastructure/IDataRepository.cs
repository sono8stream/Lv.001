using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// データを読み出すためのリポジトリのインターフェース
    /// </summary>
    public interface IDataRepository<T, T_ID>
    {
        public T Find(T_ID id);
    }
}
