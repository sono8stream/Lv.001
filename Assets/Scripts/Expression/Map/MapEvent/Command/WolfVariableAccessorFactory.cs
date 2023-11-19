using Expression.Common;
using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfVariableAccessorFactory : Common.IDataAccessorFactory
    {
        private WolfVariableAccessorCreator creator;

        public WolfVariableAccessorFactory(bool isConstValue, int rawVal)
        {
            creator = new WolfVariableAccessorCreator(isConstValue, rawVal);
        }

        public int GetInt(CommandVisitContext context)
        {
            // 【暫定】VariableAccessorCreator内でRepositoryAccessorFactoryを直接呼べるよう、処理を集約させたい。
            // 最終的にはCreator内でFactory生成とAccessor取得を集約させたい。
            return creator.Create(context).GetInt();
        }

        public string GetString(CommandVisitContext context)
        {
            return creator.Create(context).GetString();
        }

        public void SetInt(CommandVisitContext context, int value)
        {
            creator.Create(context).SetInt(value);
        }

        public void SetString(CommandVisitContext context, string value)
        {
            creator.Create(context).SetString(value);
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            return creator.Create(context).TestType(targetType);
        }
    }
}
