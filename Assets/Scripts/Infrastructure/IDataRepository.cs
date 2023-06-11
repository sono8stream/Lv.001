using Domain.Data;

namespace Infrastructure
{
    /// <summary>
    /// ゲームの詳細な仕様に関するデータを読み出すためのリポジトリのインターフェース
    /// </summary>
    public interface IDataRepository
    // 【暫定】RecordIdからDataRecordを読み出したいユースケースがないので，今後廃止
    // DataRefからDataFieldを読み出すユースケースの方がありそう
    {
        public DataField<int> FindInt(DataRef dataRef);

        public void SetInt(DataRef dataRef, int value);

        public DataField<string> FindString(DataRef dataRef);

        public void SetString(DataRef dataRef, string value);
    }
}
