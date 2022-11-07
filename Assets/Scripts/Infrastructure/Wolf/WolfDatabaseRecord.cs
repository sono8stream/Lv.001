using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGの1つのDB項目を示すレコードクラス
    /// ウディタのUIでは「データ」に対応する
    /// </summary>
    public class WolfDatabaseRecord
    {
        public string Name { get; private set; }

        public int[] IntData { get; private set; }

        public string[] StringData { get; private set; }

        public WolfDatabaseRecord(string name, int[] intData, string[] stringData)
        {
            Name = name;
            IntData = intData;
            StringData = stringData;
        }
    }
}
