
namespace Common
{
    /// <summary>
    /// �f�[�^��ǂݏo�����߂̃��|�W�g���̃C���^�[�t�F�[�X
    /// </summary>
    public interface IRepository<T, T_ID>
    {
        public T Find(T_ID id);
    }
}
