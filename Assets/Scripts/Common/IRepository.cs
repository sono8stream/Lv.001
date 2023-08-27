
namespace Common
{
    /// <summary>
    /// データを読み出すためのリポジトリのインターフェース
    /// </summary>
    public interface IRepository<T, T_ID>
    {
        public T Find(T_ID id);
    }
}
