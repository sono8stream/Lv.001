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
        public DataRecord Find(RecordId id)
        {
            return null;
        }

        public DataField<int> FindInt(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }

        public DataField<string> FindString(DataRef dataRef)
        {
            throw new System.NotImplementedException();
        }
    }
}
