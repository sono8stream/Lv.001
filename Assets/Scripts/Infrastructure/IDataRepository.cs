using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    /// <summary>
    /// �f�[�^��ǂݏo�����߂̃��|�W�g���̃C���^�[�t�F�[�X
    /// </summary>
    public interface IDataRepository<T, T_ID>
    {
        public T Find(T_ID id);
    }
}
