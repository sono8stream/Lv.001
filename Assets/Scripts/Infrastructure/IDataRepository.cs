using Domain.Data;

namespace Infrastructure
{
    /// <summary>
    /// �Q�[���̏ڍׂȎd�l�Ɋւ���f�[�^��ǂݏo�����߂̃��|�W�g���̃C���^�[�t�F�[�X
    /// </summary>
    public interface IDataRepository
    // �y�b��zRecordId����DataRecord��ǂݏo���������[�X�P�[�X���Ȃ��̂ŁC����p�~
    // DataRef����DataField��ǂݏo�����[�X�P�[�X�̕������肻��
    {
        public DataField<int> FindInt(DataRef dataRef);

        public void SetInt(DataRef dataRef, int value);

        public DataField<string> FindString(DataRef dataRef);

        public void SetString(DataRef dataRef, string value);
    }
}
