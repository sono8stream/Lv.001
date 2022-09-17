using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGのシステムDBファイルを読み込むためのリポジトリ
    /// </summary>
    public class WolfDatabaseLoader
    {
        public WolfDatabaseSchema[] LoadTypes(string projPath)
        {
            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(projPath);
            int offset = 0;
            int columns = reader.ReadInt(offset, true, out offset);
            WolfDatabaseSchema[] res = new WolfDatabaseSchema[columns];

            for (int i = 0; i < columns; i++)
            {
                res[i] = LoadType(reader);
            }

            return res;
        }

        private WolfDatabaseSchema LoadType(Util.Wolf.WolfDataReader reader)
        {
            throw new System.NotImplementedException();
        }

        public void LoadData(string[] typeArray,
            Dictionary<DataRef, int> intDict,
            Dictionary<DataRef, string> strDict)
        {
            throw new System.NotImplementedException();
        }
    }
}
