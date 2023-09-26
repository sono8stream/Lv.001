using Expression.Common;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.Command
{
    public class WolfStringAccessorFactory : IDataAccessorFactory
    {
        private bool isConstValue;
        private string rawStr;

        public WolfStringAccessorFactory(bool isConstValue, string rawVal)
        {
            this.isConstValue = isConstValue;
            this.rawStr = rawVal;
        }

        public int GetInt(CommandVisitContext context)
        {
            return 0;
        }

        public string GetString(CommandVisitContext context)
        {
            var accessor = Create(context);
            return accessor.GetString();
        }

        public void SetInt(CommandVisitContext context, int value)
        {
            // 何もしない
        }

        public void SetString(CommandVisitContext context, string value)
        {
            var accessor = Create(context);
            accessor.SetString(value);
        }

        public bool TestType(CommandVisitContext context, VariableType targetType)
        {
            var accessor = Create(context);
            return accessor.TestType(targetType);
        }

        private Common.IDataAccessor Create(CommandVisitContext context)
        {
            // そのまま値を使用する場合
            if (isConstValue)
            {
                return new Common.ConstDataAccessor(rawStr);
            }

            if (rawStr.StartsWith("\\self["))
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = rawStr.Substring("\\self[".Length, rawStr.Length - "\\self[]".Length);
                if (int.TryParse(fieldStr, out int fieldId))
                {
                    // ID変換できない場合は定数として返す
                    new Common.ConstDataAccessor(rawStr);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryVariableAccessor(repository, dataRef);
            }
            else if (rawStr.StartsWith("\\cself["))
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = rawStr.Substring("\\self[".Length, rawStr.Length - "\\self[]".Length);
                if (int.TryParse(fieldStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor(rawStr);
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return new Common.ConstDataAccessor("0");
                }

                return new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }
            else if (rawStr.StartsWith("\\sdb["))
            {
                // システムDBの変数呼び出し
                // []内を取得
                string fieldStr = rawStr.Substring("\\self[".Length, rawStr.Length - "\\self[]".Length);
                if (int.TryParse(fieldStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor(rawStr);
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return new Common.ConstDataAccessor("0");
                }

                return new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }

            // 特殊条件以外の場合、定数を取得
            return new Common.ConstDataAccessor(rawStr);
        }
    }
}
