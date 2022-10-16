using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPGの1つのDBカラムの制約を保持するクラス
    /// </summary>
    public class WolfDatabaseColumn
    {
        public string Name { get; private set; }
        public ColumnType Type { get; private set; }
        public int InitValue { get; private set; }

        public WolfDatabaseColumn(string name, ColumnType type, int initValue)
        {
            Name = name;
            Type = type;
            InitValue = initValue;
        }

        public enum ColumnType
        {
            Int,String
        }
    }
}
