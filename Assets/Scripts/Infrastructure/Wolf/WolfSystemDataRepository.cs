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
            var loader = new Infrastructure.WolfDatabaseLoader();
            var projPath = $"{Application.streamingAssetsPath}/Data/BasicData/SysDatabase.project";
            var datPath = $"{Application.streamingAssetsPath}/Data/BasicData/SysDatabase.dat";
            loader.LoadDatabase(projPath, datPath, out intDict, out stringDict);
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            int val = intDict.ContainsKey(dataRef) ? intDict[dataRef] : 0;
            return new DataField<int>(dataRef.FieldId, val);
        }

        public void SetInt(DataRef dataRef, int value)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            string val = stringDict.ContainsKey(dataRef) ? stringDict[dataRef] : "";
            if (stringDict.ContainsKey(dataRef))
            {
                val = stringDict[dataRef];
            }
            return new DataField<string>(dataRef.FieldId, val);
        }

        public void SetString(DataRef dataRef, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
