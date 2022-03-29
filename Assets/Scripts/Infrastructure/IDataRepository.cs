using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Data;

namespace Infrastructure
{
    /// <summary>
    /// �Q�[���̏ڍׂȎd�l�Ɋւ���f�[�^��ǂݏo�����߂̃��|�W�g���̃C���^�[�t�F�[�X
    /// </summary>
    public interface IDataRepository : IRepository<DataRecord, RecordId>
    {
        public DataField<int> FindInt(DataRef dataRef);

        public DataField<string> FindString(DataRef dataRef);
    }
}
