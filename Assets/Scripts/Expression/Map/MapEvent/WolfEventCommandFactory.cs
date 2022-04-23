using System;
using Util.Wolf;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class WolfEventCommandFactory
    {
        WolfDataReader reader;
        MapId mapId;
        EventId eventId;
        int startOffset;

        public WolfEventCommandFactory(WolfDataReader reader, MapId mapId,EventId eventId, int startOffset)
        {
            this.reader = reader;
            this.mapId = mapId;
            this.eventId = eventId;
            this.startOffset = startOffset;
        }

        public EventCommandBase Create(out int nextOffset)
        {
            int currentOffset = startOffset;
            MetaEventCommand metaCommand = ReadCommand(currentOffset, out currentOffset);

            EventCommandBase command = new EventCommandBase();

            switch (metaCommand.NumberArgs[0])
            {
                case 0x00000065:
                    command = CreateShowTextCommand(metaCommand.StringArgs);
                    break;
                case 0x00000067:
                    // デバッグ文。処理なし
                    break;
                case 0x00000066:
                    command = CreateChoiceForkCommand(metaCommand);
                    break;
                case 0x0000006F:
                    command = CreateFlagForkByVariableCommand(metaCommand);
                    break;
                case 0x00000191:
                    command = CreateForkBeginCommand(metaCommand);
                    break;
                case 0x000000D2:
                    CreateCallEventByIdCommand(currentOffset, out currentOffset);
                    break;
                default:
                    if (metaCommand.FooterValue == 1)
                    {
                        ReadEventMoveRoute(currentOffset, out currentOffset);
                    }
                    break;
            }

            nextOffset = currentOffset;
            startOffset = currentOffset;
            return command;
        }

        private MetaEventCommand ReadCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;

            int numberVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            int[] numberVariables = new int[numberVariableCount];
            for (int i = 0; i < numberVariableCount; i++)
            {
                numberVariables[i] = reader.ReadInt(currentOffset, true, out currentOffset);

                if (i == 0 && numberVariables[i] == 0x000000D2)
                {
                    // イベント呼び出しコマンドは特殊な配置なので切り抜け、個別処理で読み込ませる
                    nextOffset = currentOffset;
                    return new MetaEventCommand(numberVariables, null, 0, 0);
                }
            }

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);

            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            string[] stringVariables = new string[stringVariableCount];
            for (int i = 0; i < stringVariableCount; i++)
            {
                stringVariables[i] = reader.ReadString(currentOffset, out currentOffset);
            }

            int footer = reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;

            return new MetaEventCommand(numberVariables, stringVariables, indentDepth, footer);
        }

        private EventCommandBase CreateShowTextCommand(string[] stringVariables)
        {
            string text = stringVariables[0];
            Debug.Log($"文章表示：{text}");

            return new MessageCommand(text);
        }

        private EventCommandBase CreateChoiceForkCommand(MetaEventCommand metaCommand)
        {
            int forkParams = metaCommand.NumberArgs[1];
            int forkCount = forkParams % (1 << 4);
            int otherForkOption = forkParams / (1 << 8) % (1 << 8);

            for (int i = 0; i < metaCommand.StringArgs.Length; i++)
            {
                Debug.Log($"選択肢{i}：{metaCommand.StringArgs[i]}");
            }

            return new ChoiceForkCommand(metaCommand.IndentDepth, metaCommand.StringArgs);
        }

        private EventCommandBase CreateFlagForkByVariableCommand(MetaEventCommand metaCommand)
        {
            Debug.Log("変数分岐");
            int forkParams = metaCommand.NumberArgs[1];
            int forkCount = forkParams % (1 << 4);
            int flagCount = (metaCommand.NumberArgs.Length - 2) / 3;
            Debug.Log($"条件数：{flagCount}、分岐数：{forkCount}");
            ConditionInt[] conditions = new ConditionInt[flagCount];
            for (int i = 0; i < flagCount; i++)
            {
                int flagLeft = metaCommand.NumberArgs[2 + 3 * i];
                int flagRight = metaCommand.NumberArgs[3 + 3 * i];
                int rightAndCompareParams = metaCommand.NumberArgs[4 + 3 * i];
                Debug.Log($"左辺 : {flagLeft}, 右辺 : {flagRight}, 条件ID : {rightAndCompareParams}");

                conditions[i] = GenerateCondition(flagLeft, flagRight, rightAndCompareParams);
            }

            return new ForkByVariableIntCommand(metaCommand.IndentDepth, conditions);
        }

        private ConditionInt GenerateCondition(int flagLeft, int flagRight, int rightAndCompareParams)
        {
            Common.IDataAccessor<int> leftAccessor = GenerateIntAccessor(flagLeft);

            Common.IDataAccessor<int> rightAccessor;
            if ((rightAndCompareParams >> 4) == 0)
            {
                rightAccessor = new Common.ConstDataAccessor<int>(flagRight);
            }
            else
            {
                rightAccessor = GenerateIntAccessor(flagLeft);
            }

            IntOperatorType operatorType = (IntOperatorType)Enum.ToObject(typeof(IntOperatorType), rightAndCompareParams % (1 << 4));

            var condition = new ConditionInt(leftAccessor, rightAccessor, operatorType);
            return condition;
        }

        private Common.IDataAccessor<int> GenerateIntAccessor(int val)
        {
            if (val >= 1300000000)
            {
                // システムDB読み出し
            }
            else if (val >= 1100000000)
            {
                // 可変DB読み出し
            }
            else if (val >= 1000000000)
            {
                // ユーザーDB読み出し
            }
            else if (val >= 15000000)
            {
                // コモンイベントのセルフ変数呼び出し
            }
            else if (val >= 9900000)
            {
                // システムＤＢ[5:システム文字列]呼び出し
            }
            else if (val >= 9190000)
            {
                // 実行したマップイベントの情報を呼び出し
            }
            else if (val >= 9180000)
            {
                // 主人公か仲間の情報を呼び出し
            }
            else if (val >= 9100000)
            {
                // 指定したマップイベントの情報を呼び出し
            }
            else if (val >= 9000000)
            {
                // システムＤＢ[6:システム変数名]呼び出し
            }
            else if (val >= 8000000)
            {
                // 乱数呼び出し
            }
            else if (val >= 3000000)
            {
                // システムＤＢ[4:文字列変数名]呼び出し
            }
            else if (val >= 2000000)
            {
                // システムＤＢ[14:通常変数名]もしくはシステムＤＢ[15:予備変数1]～[23:予備変数9]呼び出し
            }
            else if (val >= 1600000)
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
            }
            else if (val >= 1100000)
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                var repository = DI.DependencyInjector.It().ExpressionDataRpository;
                Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                    new Domain.Data.TableId(mapId.Value),
                    new Domain.Data.RecordId(eventId.Value),
                    new Domain.Data.FieldId(val % 10)
                    );
                return new Common.RepositoryIntAccessor(repository, dataRef);
            }
            else if (val >= 1000000)
            {
                // 指定したマップイベントのセルフ変数呼び出し
            }

            // 特殊条件以外の場合、定数を取得
            return new Common.ConstDataAccessor<int>(val);
        }

        private EventCommandBase CreateForkBeginCommand(MetaEventCommand metaCommand)
        {
            int forkNumber = metaCommand.NumberArgs[1];

            Debug.Log($"分岐始点{forkNumber}");

            return new ForkBeginCommand(metaCommand.IndentDepth, forkNumber);
        }

        // イベントコマンドの取得は未済み
        private EventCommandBase CreateCallEventByIdCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int eventId = reader.ReadInt(currentOffset, true, out currentOffset);
            Debug.Log($"イベント{eventId.ToString()}呼び出し");

            int argsParam = reader.ReadInt(currentOffset, true, out currentOffset);
            int numberArgCount = argsParam % (1 << 4);
            int stringArgCount = argsParam / (1 << 4) % (1 << 4);
            int stringArgVariableFlag = argsParam / (1 << 12) % (1 << 4);
            int acceptReturnValueFlag = argsParam / (1 << 24) % (1 << 8);

            for (int i = 0; i < numberArgCount; i++)
            {
                int numberArg = reader.ReadInt(currentOffset, true, out currentOffset);
                //Debug.Log($"数値引数{i.ToString()}：{numberArg.ToString()}");
            }

            for (int i = 0; i < stringArgCount; i++)
            {
                int stringArgVariableValue = reader.ReadInt(currentOffset, true, out currentOffset);
                //Debug.Log($"文字列引数呼び出し値{i.ToString()}：{stringArgVariableValue.ToString()}");
            }

            if (acceptReturnValueFlag > 0)
            {
                int returnValueAddress = reader.ReadInt(currentOffset, true, out currentOffset);
            }

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);

            for (int i = 0; i < stringVariableCount; i++)
            {
                string text = reader.ReadString(currentOffset, out currentOffset);
                //Debug.Log($"文字データ：{text}");
            }

            // フッタはスキップ
            int ft = reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        // 【暫定】モデル定義までデータを空読み
        private void ReadEventMoveRoute(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int animationSpeed = reader.ReadByte(currentOffset, out currentOffset);
            int moveSpeed = reader.ReadByte(currentOffset, out currentOffset);
            int moveFrequency = reader.ReadByte(currentOffset, out currentOffset);
            int moveType = reader.ReadByte(currentOffset, out currentOffset);
            int optionType = reader.ReadByte(currentOffset, out currentOffset);
            int moveFlag = reader.ReadByte(currentOffset, out currentOffset);
            int commandCount = reader.ReadInt(currentOffset, true, out currentOffset);
            //Debug.Log($"移動コマンド数：{commandCount}");

            // 動作コマンド
            for (int i = 0; i < commandCount; i++)
            {
                int commandType = reader.ReadByte(currentOffset, out currentOffset);
                int variableCount = reader.ReadByte(currentOffset, out currentOffset);
                //Debug.Log($"コマンドタイプ：{commandType}、変数の数： {variableCount}");
                for (int j = 0; j < variableCount; j++)
                {
                    int variableValue = reader.ReadInt(currentOffset, true, out currentOffset);
                    //Debug.Log($"変数{j}：{variableValue}");
                }

                // 終端
                int footer1 = reader.ReadByte(currentOffset, out currentOffset);
                int footer2 = reader.ReadByte(currentOffset, out currentOffset);
                //Debug.Log($"移動コマンド　フッタ：{footer1} {footer2}");
            }

            nextOffset = currentOffset;
        }
    }
}