using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Data;

namespace Infrastructure
{
    /// <summary>
    /// ゲームとしてのふるまい（ジャンル）を表現するセーブデータを読み出すためのリポジトリのインターフェース
    /// 例）マップイベントのセルフ変数、主人公の現在地情報
    /// </summary>
    public interface IExpressionDataRepository : IDataRepository
    {
    }
}
