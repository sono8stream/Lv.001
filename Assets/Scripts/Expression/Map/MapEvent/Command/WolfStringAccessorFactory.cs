﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.Command
{
    public class WolfStringAccessorFactory : Common.IDataAccessorFactory<string>
    {
        private bool isConstValue;
        private string rawStr;

        public WolfStringAccessorFactory(bool isConstValue, string rawVal)
        {
            this.isConstValue = isConstValue;
            this.rawStr = rawVal;
        }

        public Common.IDataAccessor<string> Create(CommandVisitContext context)
        {
            // そのまま値を使用する場合
            if (isConstValue)
            {
                return new Common.ConstDataAccessor<string>(rawStr);
            }

            if (rawStr.StartsWith("\\self["))
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = rawStr.Substring("\\self[".Length, rawStr.Length - "\\self[]".Length);
                if (int.TryParse(fieldStr, out int fieldId))
                {
                    // ID変換できない場合は定数として返す
                    new Common.ConstDataAccessor<string>(rawStr);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryStringAccessor(repository, dataRef);
            }

            // 特殊条件以外の場合、定数を取得
            return new Common.ConstDataAccessor<string>(rawStr);
        }
    }
}
