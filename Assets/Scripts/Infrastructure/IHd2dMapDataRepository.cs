using Expression.Map;

namespace Infrastructure
{
    /// <summary>
    /// HD2Dマップ情報を読み出すためのインターフェース
    /// </summary>
    public interface IHd2dMapDataRepository:IRepository<Hd2dMapData,MapId>
    {
        public int GetCount();
    }
}
