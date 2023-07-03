using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG��1��DB���ڂ��������R�[�h�N���X
    /// �E�f�B�^��UI�ł́u�f�[�^�v�ɑΉ�����
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
