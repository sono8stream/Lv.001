using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// HD2Dマップ情報を読み出すためのインターフェース
    /// </summary>
    public interface IBaseMapDataRepository : IRepository<BaseMapData, MapId>
    {
        public int GetCount();
    }
}
