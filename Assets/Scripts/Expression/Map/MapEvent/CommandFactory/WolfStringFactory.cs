using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfStringFactory : IStringFactory
    {
        private string[] parts;
        private StringBlockFactory2 blockFactory;

        public WolfStringFactory(string text)
        {
            parts = CreateParts(text);
            blockFactory = new StringBlockFactory2(text);
        }

        string[] CreateParts(string text)
        {
            var matches = Regex.Matches(text, BuildPattern());
            return Regex.Split(text, BuildPattern());
        }

        public string GenerateMessage(CommandVisitContext context)
        {
            return blockFactory.GenerateMessage(context);

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

            // 再帰的に\XXX[]という文字列を構造化しながら取得する。
            // \XXX[]hoge\YYY[]というように並列で複数から構成されている場合があるので、]で区切りながら取得していく
            IStringBlock block = CreateBlock(text, ref index, 0, context);
            return block.GetMessaage();
        }

        IStringBlock CreateBlock(string text, ref int index, int depth, CommandVisitContext context)
        {
            List<IStringBlock> blocks = new List<IStringBlock>();
            string tmp = "";
            bool isSpecial = false;

            for (; index < text.Length; index++)
            {
                // 前から順に読んでいく
                if (text[index] == '\\')
                {
                    isSpecial = true;
                    // 特殊文字が始まるので、一旦ここまでの文字列を塊で詰めておく
                    blocks.Add(new ConstStringBlock(tmp));
                    tmp = "";
                }
                else if (text[index] == '[')
                {
                    // 特殊文字の終わりなので、入れ子判定。
                    if (isSpecial)
                    {
                        if (patterns.Contains(tmp))
                        {
                            // 特殊文字なので、内部ブロックを取得。
                            index++;
                            IStringBlock child = CreateBlock(text, ref index, depth + 1, context);
                            // ブロック読み取り後の位置に]がいなければ特殊文字とみなせない。
                            if (index < text.Length && text[index] == ']')
                            {
                                blocks.Add(new SpecialStringBlock(tmp, context, child));
                            }
                            else
                            {
                                // 特殊文字のブロックになっていない場合は通常の文字列として足すだけ
                                List<IStringBlock> blocksTmp = new List<IStringBlock>();
                                blocksTmp.Add(new ConstStringBlock($"\\{tmp}["));
                                blocksTmp.Add(child);
                                if (index < text.Length)
                                {
                                    blocksTmp.Add(new ConstStringBlock($"{text[index]}"));
                                }
                                blocks.Add(new MultiStringBlock(blocksTmp));
                            }
                        }
                        else
                        {
                            // 解釈可能な特殊文字ではないが、入れ子構造にはなっているのでハンドリングしておく。
                            index++;
                            List<IStringBlock> blocksTmp = new List<IStringBlock>();
                            blocksTmp.Add(new ConstStringBlock($"\\{tmp}["));
                            IStringBlock child = CreateBlock(text, ref index, depth + 1, context);
                            blocksTmp.Add(child);
                            if (index < text.Length)
                            {
                                blocksTmp.Add(new ConstStringBlock($"{text[index]}"));
                            }
                            blocks.Add(new MultiStringBlock(blocksTmp));
                        }
                        tmp = "";
                        isSpecial = false;
                    }
                    else
                    {
                        // 特殊文字でない括弧は他の文字と同様に処理。
                        tmp += text[index];
                    }
                }
                else if (text[index] == ']')
                {
                    if (depth > 0)
                    {
                        // ネストしているなら上位からの呼び出しで括弧を閉じる動作なので、上位に返す
                        if (!string.IsNullOrEmpty(tmp))
                        {
                            blocks.Add(new ConstStringBlock(tmp));
                        }
                        return new MultiStringBlock(blocks);
                    }
                    else
                    {
                        // ネストしていない状態で括弧を閉じる動作は特殊文字ではないはずなので、通常通り処理。
                        tmp += text[index];
                    }
                }
                else
                {
                    tmp += text[index];
                }
            }

            if (!string.IsNullOrEmpty(tmp))
            {
                blocks.Add(new ConstStringBlock(tmp));
            }

            return new MultiStringBlock(blocks);
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

    internal class MultiStringBlock : IStringBlock
    {
        private List<IStringBlock> children;

        public MultiStringBlock(List<IStringBlock> children) : base()
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

    internal class ConstStringBlock : IStringBlock
    {
        private string part;

        public ConstStringBlock(string part) : base()
        {
            this.part = part;
        }

        public string GetMessaage()
        {
            return part;
        }
    }

    internal class SpecialStringBlock : IStringBlock
    {
        private IStringBlock child;
        private string functionString;
        private CommandVisitContext context;

        public SpecialStringBlock(string functionString,
            CommandVisitContext context, IStringBlock child) : base()
        {
            this.functionString = functionString;
            this.child = child;
            this.context = context;
        }

        public string GetMessaage()
        {
            string childStr = child.GetMessaage();
            if (functionString.Equals("self"))
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                if (!int.TryParse(childStr, out int fieldId))
                {
                    // ID変換できない場合は定数として返す
                    return $"\\{functionString}[{childStr}]";
                }

                var repository = DI.DependencyInjector.It().MapEventStateRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(context.MapId.Value, ""),
                    new Domain.Data.RecordId(context.EventId.Value, ""),
                    new Domain.Data.FieldId(fieldId, "")
                    );

                return new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else if (functionString.Equals("cself"))
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
                if (!int.TryParse(childStr, out int variableId))
                {
                    // ID変換できない場合は定数として返す
                    return $"\\{functionString}[{childStr}]";
                }

                if (context.CommonEventId == null)
                {
                    // コモンイベントから呼び出されていない場合は0を返す
                    return "0";
                }

                return new Event.CommonEventVariableAccessor(
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

                return new Common.RepositoryVariableAccessor(repository, dataRef).GetString();
            }
            else
            {
                // 処理できない場合はただ返却する
                return $"\\{functionString}[{childStr}]";
            }
        }
    }
}
