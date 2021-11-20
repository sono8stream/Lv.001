
namespace Expression
{
    public class DependencyInjector
    {

        public Map.IMapDataRepository MapDataRepository { get; private set; }

        public DependencyInjector(Map.IMapDataRepository mapDataRepository)
        {
            MapDataRepository = mapDataRepository;
        }
    }
}