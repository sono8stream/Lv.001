
namespace DI
{
    public class DependencyInjector
    {
        private static DependencyInjector instance;

        // yb’èzMapDataRepository‚ğInfrastructure‚ÉˆÚ“®‚µ‚Ä‹¤’Ê‰»‚·‚é
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
                // ŠÂ‹«‚É‰‚¶‚ÄˆË‘¶ŠÖŒW‚ğ’“ü‚·‚é
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