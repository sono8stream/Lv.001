using Infrastructure;

namespace DI
{
    public class DependencyInjector
    {
        private static DependencyInjector instance;

        public ISystemDataRepository SystemDataRepository { get; private set; }

        public IMasterDataRepository MasterDataRepository { get; private set; }

        public IPlayDataRepository PlayDataRepository { get; private set; }

        public Expression.Map.IMapDataRepository MapDataRepository { get; private set; }

        public Expression.Map.IBaseMapDataRepository BaseMapDataRepository { get; private set; }

        public IExpressionDataRepository MapEventStateRpository { get; private set; }

        public Expression.Event.ICommonEventCommandsRepository CommonEventCommandsRepository { get; private set; }

        private DependencyInjector() { }

        public static DependencyInjector It()
        {
            if (instance == null)
            {
                // 環境に応じて依存関係を注入する
                // 依存関係があるので，この順に初期化すること
                instance = new DependencyInjector();
                instance.SystemDataRepository = new WolfSystemDataRepository();
                instance.MasterDataRepository = new WolfMasterDataRepository();
                instance.PlayDataRepository = new WolfPlayDataRepository();
                instance.CommonEventCommandsRepository = new WolfCommonEventCommandsRepository();
                instance.MapDataRepository = new WolfMapDataRepository();
                instance.MapEventStateRpository = new WolfMapEventStateRepository();
                instance.BaseMapDataRepository = new WolfBaseMapDataRepository();
            }

            return instance;
        }
    }
}