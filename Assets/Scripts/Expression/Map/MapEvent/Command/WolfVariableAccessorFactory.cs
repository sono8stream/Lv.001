using Expression.Common;
using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfVariableAccessorFactory : Common.IDataAccessorFactory
    {
        private WolfIntAccessorCreator creator;

        public WolfVariableAccessorFactory(bool isConstValue, int rawVal)
        {
            creator = new WolfIntAccessorCreator(isConstValue, rawVal);
        }

        public int Get(CommandVisitContext context)
        {
            return creator.Create(context).Get();
        }

        public int GetInt(CommandVisitContext context)
        {
            return creator.Create(context).Get();
        }

        public string GetString(CommandVisitContext context)
        {
            return creator.Create(context).Get().ToString();
        }

        public void Set(CommandVisitContext context, int value)
        {
            creator.Create(context).Set(value);
        }

        public void SetInt(CommandVisitContext context, int value)
        {
            creator.Create(context).Set(value);
        }

        public void SetString(CommandVisitContext context, string value)
        {
            // 格納先が数値とは限らない。要修正
            if (int.TryParse(value, out int val))
            {
                creator.Create(context).Set(val);
            }
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            return creator.Create(context).TestType(targetType);
        }
    }
}
