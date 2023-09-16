using System;
using System.Collections.Generic;
using Infrastructure;

namespace Expression.Map.MapEvent.Command
{
    /// <summary>
    /// DataRefを遅延生成してデータアクセスするAccessor
    /// </summary>
    public class WolfIntRepositoryAccessorFactory : Common.IDataAccessorFactory<int>
    {
        private WolfIntAccessorCreator tableIdCreator;
        private WolfIntAccessorCreator recordIdCreator;
        private WolfIntAccessorCreator fieldIdCreator;
        private IDataRepository targetRepository;

        public WolfIntRepositoryAccessorFactory(WolfConfig.DatabaseType databaseType,
            int tableVal, int recordVal, int fieldVal)
        {
            targetRepository = GetRepository(databaseType);

            tableIdCreator = new WolfIntAccessorCreator(false, tableVal);
            recordIdCreator = new WolfIntAccessorCreator(false, recordVal);
            fieldIdCreator = new WolfIntAccessorCreator(false, fieldVal);
        }

        public int Get(CommandVisitContext context)
        {
            var accessor = Create(context);
            return accessor.Get();
        }

        public void Set(CommandVisitContext context, int value)
        {
            var accessor = Create(context);
            accessor.Set(value);
        }

        private Common.IDataAccessor<int> Create(CommandVisitContext context)
        {
            var tableId = new Domain.Data.TableId(tableIdCreator.Create(context).Get(), "");
            var recordId = new Domain.Data.RecordId(recordIdCreator.Create(context).Get(), "");
            var fieldId = new Domain.Data.FieldId(fieldIdCreator.Create(context).Get(), "");
            var dataRef = new Domain.Data.DataRef(tableId, recordId, fieldId);
            return new Common.RepositoryIntAccessor(targetRepository, dataRef);
        }

        private IDataRepository GetRepository(WolfConfig.DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case WolfConfig.DatabaseType.User:
                    return DI.DependencyInjector.It().MasterDataRepository;

                case WolfConfig.DatabaseType.Changable:
                    return DI.DependencyInjector.It().PlayDataRepository;

                case WolfConfig.DatabaseType.System:
                    return DI.DependencyInjector.It().SystemDataRepository;
                default:
                    throw new Exception("未定義のDBタイプが指定された");
            }
        }
    }
}
