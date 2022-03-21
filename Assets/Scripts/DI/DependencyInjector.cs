
namespace DI
{
    public class DependencyInjector
    {
        private static DependencyInjector instance;

        // 【暫定】MapDataRepositoryをInfrastructureに移動して共通化する
        public Infrastructure.IMapDataRepository MapDataRepository { get; private set; }

        public Infrastructure.ISystemDataRepository SystemDataRepository { get; private set; }

        public Infrastructure.IMasterDataRepository MasterDataRepository { get; private set; }

        public Infrastructure.IPlayDataRepository PlayDataRepository { get; private set; }

        private DependencyInjector(Infrastructure.IMapDataRepository mapDataRepository,
            Infrastructure.ISystemDataRepository systemDataRepository,
            Infrastructure.IMasterDataRepository masterDataRepository,
            Infrastructure.IPlayDataRepository playDataRepository)
        {
            MapDataRepository = mapDataRepository;
            SystemDataRepository = systemDataRepository;
            MasterDataRepository = masterDataRepository;
            PlayDataRepository = playDataRepository;
        }

        public static DependencyInjector It()
        {
            if (instance == null)
            {
                // 環境に応じて依存関係を注入する
                instance = new DependencyInjector(
                    new Expression.Map.WolfMapDataRepository(),
                    new Infrastructure.WolfSystemDataRepository(),
                    new Infrastructure.WolfMasterDataRepository(),
                    new Infrastructure.WolfPlayDataRepository()
                    );
            }

            return instance;
        }
    }
}