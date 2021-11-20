
namespace Expression
{
    public class WolfDependencyInjector
    {
        private static WolfDependencyInjector instance;

        public Map.IMapDataRepository MapDataRepository { get; private set; }

        private WolfDependencyInjector(Map.IMapDataRepository mapDataRepository)
        {
            MapDataRepository = mapDataRepository;
        }

        public static WolfDependencyInjector It()
        {
            if (instance == null)
            {
                instance = new WolfDependencyInjector(new Map.WolfMapDataRepository());
            }

            return instance;
        }
    }
}