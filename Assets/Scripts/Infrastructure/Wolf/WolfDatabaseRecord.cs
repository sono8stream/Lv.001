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

        public int[] IntData { get; set; }

        public string[] StringData { get; set; }

        public WolfDatabaseRecord(string name)
        {
            Name = name;
            IntData = null;
            StringData = null;
        }
    }
}
