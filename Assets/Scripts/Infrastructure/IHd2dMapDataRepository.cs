using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// HD2D�}�b�v����ǂݏo�����߂̃C���^�[�t�F�[�X
    /// </summary>
    public interface IHd2dMapDataRepository:IRepository<Hd2dMapData,MapId>
    {
        public int GetCount();
    }
}
