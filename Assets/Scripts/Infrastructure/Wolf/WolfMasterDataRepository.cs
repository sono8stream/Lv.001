using Domain.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// WolfRPG�̃V�X�e���ϐ���ǂݏo�����߂̃��|�W�g��
    /// </summary>
    public class WolfMasterDataRepository : IMasterDataRepository
    {
        public DataRecord Find(RecordId id)
        {
            return null;
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
