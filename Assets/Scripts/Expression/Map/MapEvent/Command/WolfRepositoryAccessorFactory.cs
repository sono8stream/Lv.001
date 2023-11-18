using System;
using System.Collections.Generic;
using Infrastructure;
using Expression.Common;

namespace Expression.Map.MapEvent.Command
{
    /// <summary>
    /// DataRefを遅延生成してデータアクセスするAccessor
    /// </summary>
    public class WolfRepositoryAccessorFactory : IDataAccessorFactory
    {
        private WolfVariableAccessorCreator tableIdCreator;
        private WolfVariableAccessorCreator recordIdCreator;
        private WolfVariableAccessorCreator fieldIdCreator;
        private IDataRepository targetRepository;

        public WolfRepositoryAccessorFactory(WolfConfig.DatabaseType databaseType,
            int tableVal, int recordVal, int fieldVal)
        {
            targetRepository = GetRepository(databaseType);

            tableIdCreator = new WolfVariableAccessorCreator(false, tableVal);
            recordIdCreator = new WolfVariableAccessorCreator(false, recordVal);
            fieldIdCreator = new WolfVariableAccessorCreator(false, fieldVal);
        }

        public int GetInt(CommandVisitContext context)
        {
            var accessor = CreateRepositoryAccessor(context);
            return accessor.GetInt();
        }

        public string GetString(CommandVisitContext context)
        {
            var accessor = CreateRepositoryAccessor(context);
            return accessor.GetString();
        }

        public void SetInt(CommandVisitContext context, int value)
        {
            var accessor = CreateRepositoryAccessor(context);
            accessor.SetInt(value);
        }

        public void SetString(CommandVisitContext context, string value)
        {
            var accessor = CreateRepositoryAccessor(context);
            accessor.SetString(value);
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            var accessor = CreateRepositoryAccessor(context);
            return accessor.TestType(targetType);
        }

        private Common.IDataAccessor CreateRepositoryAccessor(CommandVisitContext context)
        {
            var tableId = new Domain.Data.TableId(tableIdCreator.Create(context).GetInt(), "");
            var recordId = new Domain.Data.RecordId(recordIdCreator.Create(context).GetInt(), "");
            var fieldId = new Domain.Data.FieldId(fieldIdCreator.Create(context).GetInt(), "");
            var dataRef = new Domain.Data.DataRef(tableId, recordId, fieldId);
            return new Common.RepositoryVariableAccessor(targetRepository, dataRef);
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
