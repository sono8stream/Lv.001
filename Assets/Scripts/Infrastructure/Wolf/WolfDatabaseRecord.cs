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
