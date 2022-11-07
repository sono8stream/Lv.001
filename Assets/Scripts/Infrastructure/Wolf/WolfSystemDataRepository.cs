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
        private Dictionary<DataRef, int> intDict;
        private Dictionary<DataRef, string> stringDict;

        public WolfSystemDataRepository()
        {
            string typePath = $"{Application.streamingAssetsPath}/Data/BasicData/SysDatabase.project";
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public void SetInt(DataRef dataRef, int value)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public void SetString(DataRef dataRef, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
