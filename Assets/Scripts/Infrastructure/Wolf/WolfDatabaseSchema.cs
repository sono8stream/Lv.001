using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG��1��DB���ڂ̐����ێ�����X�L�[�}�N���X
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
