using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfStringFactory : IStringFactory
    {
        private string[] parts;

        public WolfStringFactory(string text)
        {
            parts = CreateParts(text);
        }

        string[] CreateParts(string text)
        {
            var matches = Regex.Matches(text, BuildPattern());
            return Regex.Split(text, BuildPattern());
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            string message = "";
            for (int i = 0; i < parts.Length; i++)
            {
                message += Create(parts[i], context).Get();
            }

            return message;
        }

        private string BuildPattern()
        {
            // 特殊文字を取り出す
            // 【暫定】一部の特殊文字にしか対応していないので、随時修正
            string[] blocks = new string[] {
                @"self\[\d+]",// マップイベントのセルフ変数
                @"cself\[\d+]",// コモンイベントのセルフ変数
                @"udb\[\d+:\d+:\d+]",// マスタデータベース
                @"cdb\[\d+:\d+:\d+]",// 可変データベース
                @"sdb\[\d+:\d+:\d+]",// システムデータベース
            };
            return @"\\(" + string.Join("|", blocks) + ")";
        }

        private Common.IDataAccessor<string> Create(string str, CommandVisitContext context)
        {
            // そのまま値を使用する場合

            if (str.StartsWith("self["))
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = str.Substring("self[".Length, str.Length - "self[]".Length);
                if (!int.TryParse(fieldStr, out int fieldId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor<string>(str);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryStringAccessor(repository, dataRef);
            }
            else if (str.StartsWith("cself["))
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = str.Substring("cself[".Length, str.Length - "cself[]".Length);
                if (!int.TryParse(fieldStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor<string>(str);
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return new Common.ConstDataAccessor<string>("0");
                }

                return new Event.CommonEventStringAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }
            else if (str.StartsWith("sdb["))
            {
                // システムDBの変数呼び出し
                // []内を取得
                string fieldStr = str.Substring("sdb[".Length, str.Length - "sdb[]".Length);
                if (int.TryParse(fieldStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor<string>(str);
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return new Common.ConstDataAccessor<string>("0");
                }

                return new Event.CommonEventStringAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId);
            }

            // 特殊条件以外の場合、定数を取得
            return new Common.ConstDataAccessor<string>(str);
        }
    }
}
