using Domain.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのDBを読み出すためのリポジトリ実装クラス
    /// </summary>
    public class WolfDataRepositoryImpl
    {
        private Dictionary<DataRef, int> intDict;
        private Dictionary<DataRef, string> stringDict;

        public WolfDataRepositoryImpl(string databaseName)
        {
            var loader = new Infrastructure.WolfDatabaseLoader();
            var projPath = $"{Application.streamingAssetsPath}/Data/BasicData/{databaseName}.project";
            var datPath = $"{Application.streamingAssetsPath}/Data/BasicData/{databaseName}.dat";
            loader.LoadDatabase(projPath, datPath, out intDict, out stringDict);
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            int val = intDict.ContainsKey(dataRef) ? intDict[dataRef] : 0;
            return new DataField<int>(dataRef.FieldId, val);
        }

        public void SetInt(DataRef dataRef, int value)
        {
            if (intDict.ContainsKey(dataRef))
            {
                intDict[dataRef] = value;
            }
            else if (stringDict.ContainsKey(dataRef))
            {
                stringDict[dataRef] = value.ToString();
            }
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            // 数値を文字列として取り出したいケースがある。そういった場合は文字列変換して返す
            string val = "";
            if (intDict.ContainsKey(dataRef))
            {
                val = intDict[dataRef].ToString();
            }
            else if (stringDict.ContainsKey(dataRef))
            {
                val = stringDict[dataRef];
            }
            return new DataField<string>(dataRef.FieldId, val);
        }

        public void SetString(DataRef dataRef, string value)
        {
            if (stringDict.ContainsKey(dataRef))
            {
                stringDict[dataRef] = value;
            }
        }
    }
}
