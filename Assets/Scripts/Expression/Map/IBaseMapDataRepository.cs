using Common;

namespace Expression.Map
{
    /// <summary>
    /// HD2D�}�b�v����ǂݏo�����߂̃C���^�[�t�F�[�X
    /// </summary>
    public interface IBaseMapDataRepository : IRepository<BaseMapData, MapId>
    {
        public int GetCount();
    }
}
