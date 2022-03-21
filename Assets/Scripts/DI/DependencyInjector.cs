using Infrastructure;

namespace DI
{
    public class DependencyInjector
    {
        private static DependencyInjector instance;

        public ISystemDataRepository SystemDataRepository { get; private set; }

        public IMasterDataRepository MasterDataRepository { get; private set; }

        public IPlayDataRepository PlayDataRepository { get; private set; }

        public IMapDataRepository MapDataRepository { get; private set; }

        public IExpressionDataRepository ExpressionDataRpository { get; private set; }

        private DependencyInjector(IMapDataRepository mapDataRepository,
            ISystemDataRepository systemDataRepository,
            IMasterDataRepository masterDataRepository,
            IPlayDataRepository playDataRepository,
            IExpressionDataRepository expressionDataRepository)
        {
            MapDataRepository = mapDataRepository;
            SystemDataRepository = systemDataRepository;
            MasterDataRepository = masterDataRepository;
            PlayDataRepository = playDataRepository;
            ExpressionDataRpository = expressionDataRepository;
        }

        public static DependencyInjector It()
        {
            if (instance == null)
            {
                // ä¬ã´Ç…âûÇ∂ÇƒàÀë∂ä÷åWÇíçì¸Ç∑ÇÈ
                instance = new DependencyInjector(
                    new WolfMapDataRepository(),
                    new WolfSystemDataRepository(),
                    new WolfMasterDataRepository(),
                    new WolfPlayDataRepository(),
                    new WolfExpressionDataRepository()
                    );
            }

            return instance;
        }
    }
}