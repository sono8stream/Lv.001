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
                message += Create(parts[i], context).GetString();
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

        private Common.IDataAccessor Create(string str, CommandVisitContext context)
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
                    return new Common.ConstDataAccessor(str);
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );
                return new Common.RepositoryVariableAccessor(repository, dataRef);
            }
            else if (str.StartsWith("cself["))
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
                // []内を取得
                string fieldStr = str.Substring("cself[".Length, str.Length - "cself[]".Length);
                if (!int.TryParse(fieldStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return new Common.ConstDataAccessor(str);
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return new Common.ConstDataAccessor("0");
                }

                return new Event.CommonEventVariableAccessor(
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
                    return new Common.ConstDataAccessor(str);
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
            return new Common.ConstDataAccessor(str);
        }
    }

    internal class StringBlockFactory2
    {
        private string text;
        private HashSet<string> patterns;

        public StringBlockFactory2(string text)
        {
            this.text = text;
            patterns = BuildPatterns();
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            int index = 0;
            IStringBlock block = CreateBlock(text, ref index, context);
            return block.GetMessaage();
        }

        IStringBlock CreateBlock(string text, ref int index, CommandVisitContext context)
        {
            // 再帰的に\XXX[]という文字列を構造化しながら取得する。
            // \XXX[]hoge\YYY[]というように並列で複数から構成されている場合があるので、]で区切りながら取得していく

            List<IStringBlock> blocks = new List<IStringBlock>();
            while (index < text.Length)
            {
                int specialStartI = text.IndexOf('\\', index);
                if (specialStartI == -1)
                {
                    // 特殊文字がなければそのまま返す
                    blocks.Add(new ConstStringBock(text.Substring(specialStartI)));
                }

                int blockStartI = text.IndexOf('[', specialStartI);

                if (specialStartI < 0 || blockStartI < 0)
                {
                    // 特殊文字を見つけられなかった場合、以降]がやってくるまでの文字列をそのまま返す。
                    int blockEndI = text.IndexOf(']', index);
                    if (blockEndI == -1)
                    {
                        // ]が無いので末尾まで取り出して返す
                        int currentIndex = index;
                        index = text.Length;
                        blocks.Add(new ConstStringBock(text.Substring(currentIndex)));
                    }
                    else
                    {
                        int currentIndex = index;
                        index = blockEndI + 1;
                        blocks.Add(new ConstStringBock(text.Substring(currentIndex, index - currentIndex)));
                        continue;
                    }
                }

                string functionString = text.Substring(specialStartI, blockStartI - specialStartI);
                if (patterns.Contains(functionString))
                {
                    // 特殊文字なので、内部ブロックを取得。
                    IStringBlock child = CreateBlock(text, ref blockStartI, context);
                    // 部六読み取り後に]が来なければ特殊文字とみなせない。
                    if (text[index] == ']')
                    {
                        return new SpecialStringBock(functionString, context, child);
                    }
                    else
                    {
                        // ブロックになっていない場合はそのまま足すだけ
                        blocks.Add(new ConstStringBock($"\\{functionString}[]"));
                    }
                }
            }

            if (blocks.Count == 1)
            {
                return blocks[0];
            }
            else
            {
                return new MultiStringBock(blocks);
            }
        }

        private HashSet<string> BuildPatterns()
        {
            // 随時増やす
            return new HashSet<string>
            {
                "self",
                "cself",
                "sdb",
            };
        }
    }

    internal interface IStringBlock
    {

        public string GetMessaage();
    }

    internal class MultiStringBock : IStringBlock
    {
        private List<IStringBlock> children;

        public MultiStringBock(List<IStringBlock> children) : base()
        {
            this.children = children;
        }

        public string GetMessaage()
        {
            string message = "";
            for (int i = 0; i < children.Count; i++)
            {
                message += children[i].GetMessaage();
            }

            return message;
        }
    }

    internal class ConstStringBock : IStringBlock
    {
        private string part;

        public ConstStringBock(string part) : base()
        {
            this.part = part;
        }

        public string GetMessaage()
        {
            return part;
        }
    }

    internal class SpecialStringBock : IStringBlock
    {
        private IStringBlock child;
        private string functionString;
        private CommandVisitContext context;

        public SpecialStringBock(string functionString,
            CommandVisitContext context, IStringBlock child) : base()
        {
            this.functionString = functionString;
            this.child = child;
        }

        public string GetMessaage()
        {
            string message = "";
            string childStr = child.GetMessaage();
            if (functionString.Equals("self"))
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                if (!int.TryParse(childStr, out int fieldId))
                {
                    // ID変換できない場合は定数として返す
                    message += childStr;
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                message += new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else if (functionString.Equals("cself"))
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
                if (!int.TryParse(childStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    message += childStr;
                }
                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    message += "0";
                }

                message += new Event.CommonEventVariableAccessor(
                    new Event.CommonEventId(context.CommonEventId.Value), variableId).GetString();
            }
            else if (functionString.Equals("sdb"))
            {
                // システムDBの変数呼び出し。\sdb[A:B:C]でタイプA番・データB番・項目C番を呼び出す。
                string[] vars = childStr.Split(':');
                if (vars.Length != 3)
                {
                    return childStr;
                }

                if (!(int.TryParse(vars[0], out int tableId)
                    && int.TryParse(vars[1], out int recordId)
                    && int.TryParse(vars[2], out int fieldId)))
                {
                    // ID変換できない場合は定数として返す
                    return childStr;
                }

                var repository = DI.DependencyInjector.It().SystemDataRepository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(tableId, ""),
                    new Domain.Data.RecordId(recordId, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                message += new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else
            {
                // 処理できない場合はただ返却する
                message += $"\\{functionString}[{childStr}]";
            }

            return message;
        }
    }
}
