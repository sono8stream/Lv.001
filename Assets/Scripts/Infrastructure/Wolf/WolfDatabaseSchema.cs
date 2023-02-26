using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGの1つのDB項目の制約を保持するスキーマクラス
    /// ウディタのUIでは「タイプの設定」に対応する
    /// </summary>
    public class WolfDatabaseSchema
    {
        public string Name { get; private set; }

        public WolfDatabaseColumn[] Columns { get; private set; }

        public WolfDatabaseSchema(string name, WolfDatabaseColumn[] columns)
        {
            Name = name;
            Columns = columns;
        }

        public void AddColumn()
        {

        }
    }
}
