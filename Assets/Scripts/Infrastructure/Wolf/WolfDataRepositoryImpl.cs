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

        public WolfDataRepositoryImpl(WolfConfig.DatabaseType dbType)
        {
            var loader = new Infrastructure.WolfDatabaseLoader();
            loader.LoadDatabase(dbType, out intDict, out stringDict);
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            int val = intDict.ContainsKey(dataRef) ? intDict[dataRef] : 0;
            return new DataField<int>(dataRef.FieldId, val);
        }

        public void SetInt(DataRef dataRef, int value)
        {
            Debug.Log($"Set {value} to Table: {dataRef.TableId.Value}, Record: {dataRef.RecordId.Value}, Filed: {dataRef.FieldId.Value}");
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
