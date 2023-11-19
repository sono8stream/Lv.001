using Domain.Data;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのDBを読み出すためのリポジトリ実装クラス
    /// 【暫定】Get/Setのロジックがアクセッサ側にも書かれているので、ロジックを集約させる。
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
            // 数値Dictに要素が無ければ何も取り出さない。nullハンドリングはアクセッサ側で処理させる。
            if (intDict.ContainsKey(dataRef))
            {
                int val = intDict[dataRef];
                return new DataField<int>(dataRef.FieldId, val);
            }
            else
            {
                return null;
            }
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
            // 文字列Dictに要素が無ければ何も取り出さない。nullハンドリングはアクセッサ側で処理させる。
            if (stringDict.ContainsKey(dataRef))
            {
                return new DataField<string>(dataRef.FieldId, stringDict[dataRef].ToString());
            }
            else
            {
                return null;
            }
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
