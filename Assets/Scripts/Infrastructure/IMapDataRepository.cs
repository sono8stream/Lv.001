using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// �}�b�v����ǂݏo�����߂̃C���^�[�t�F�[�X
    /// </summary>
    public interface IMapDataRepository : IRepository<MapData, MapId>
    {
        public int GetCount();
    }
}
