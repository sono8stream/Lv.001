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

        public IBaseMapDataRepository BaseMapDataRepository { get; private set; }

        public IExpressionDataRepository ExpressionDataRpository { get; private set; }

        private DependencyInjector() { }

        public static DependencyInjector It()
        {
            if (instance == null)
            {
                // ŠÂ‹«‚É‰‚¶‚ÄˆË‘¶ŠÖŒW‚ğ’“ü‚·‚é
                // ˆË‘¶ŠÖŒW‚ª‚ ‚é‚Ì‚ÅC‚±‚Ì‡‚É‰Šú‰»
                instance = new DependencyInjector();
                instance.SystemDataRepository = new WolfSystemDataRepository();
                instance.MasterDataRepository = new WolfMasterDataRepository();
                instance.PlayDataRepository = new WolfPlayDataRepository();
                instance.MapDataRepository = new WolfMapDataRepository();
                instance.ExpressionDataRpository = new WolfExpressionDataRepository();
                instance.BaseMapDataRepository = new WolfBaseMapDataRepository();
            }

            return instance;
        }
    }
}