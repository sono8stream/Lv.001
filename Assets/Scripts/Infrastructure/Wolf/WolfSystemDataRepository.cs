using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG�̃V�X�e��DB��ǂݏo�����߂̃��|�W�g��
    /// �y�b��zWolfRPG���ʂ̊��̃f�[�^���|�W�g���N���X�����A���ʉ�����
    /// </summary>
    public class WolfSystemDataRepository : ISystemDataRepository
    {
        private Dictionary<DataRef, int> intDict;
        private Dictionary<DataRef, string> stringDict;

        public WolfSystemDataRepository()
        {
            string typePath = $"{Application.streamingAssetsPath}/Data/BasicData/SysDatabase.project";
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public void SetInt(DataRef dataRef, int value)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public void SetString(DataRef dataRef, string value)
        {
            throw new System.NotImplementedException();
        }
    }
}
