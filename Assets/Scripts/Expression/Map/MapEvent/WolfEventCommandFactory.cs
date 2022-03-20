using System;
using Util.Wolf;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class WolfEventCommandFactory
    {
        WolfDataReader reader;
        int startOffset;

        public WolfEventCommandFactory(WolfDataReader reader, int startOffset)
        {
            this.reader = reader;
            this.startOffset = startOffset;
        }

        public EventCommandBase Create2(out int nextOffset)
        {
            int currentOffset = startOffset;
            int variableCount = reader.ReadByte(currentOffset, out currentOffset);
            int commandType = reader.ReadInt(currentOffset, true, out currentOffset);
            //Debug.Log(commandType.ToString("X8"));
            EventCommandBase command = new EventCommandBase();

            switch (commandType)
            {
                case 0x00000065:
                    command = CreateShowTextCommand(currentOffset, out currentOffset);
                    break;
                case 0x00000067:
                    CreateDebugTextCommand(currentOffset, out currentOffset);
                    break;
                case 0x00000066:
                    command = CreateChoiceForkCommand(currentOffset, out currentOffset);
                    break;
                case 0x0000006F:
                    CreateFlagForkByVariableCommand(currentOffset, out currentOffset, variableCount);
                    break;
                case 0x00000191:
                    command = CreateForkBeginCommand(currentOffset, out currentOffset);
                    break;
                case 0x000000D2:
                    CreateCallEventByIdCommand(currentOffset, out currentOffset);
                    break;
                default:
                    CreateDefaultCommand(currentOffset, out currentOffset, variableCount);
                    break;
            }

            nextOffset = currentOffset;
            startOffset = currentOffset;
            return command;
        }

        public EventCommandBase Create(out int nextOffset)
        {
            int currentOffset = startOffset;
            MetaEventCommand metaCommand = ReadCommand(currentOffset, out currentOffset);

            EventCommandBase command = new EventCommandBase();

            switch (metaCommand.NumberArgs[0])
            {
                case 0x00000065:
                    command = CreateShowTextCommand2(metaCommand.StringArgs);
                    break;
                case 0x00000067:
                    // デバッグ文。処理なし
                    break;
                case 0x00000066:
                    command = CreateChoiceForkCommand2(metaCommand);
                    break;
                case 0x0000006F:
                    CreateFlagForkByVariableCommand2(metaCommand);
                    break;
                case 0x00000191:
                    command = CreateForkBeginCommand2(metaCommand);
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

        // すべてのコマンドで共通の設定
        // ただし動作指定コマンドのみフッタの後に動作指定イベントが連続する
        private EventCommandBase CreateDefaultCommand(int offset, out int nextOffset, int numberVariableCount)
        {
            int currentOffset = offset;

            // 番号-1だけ読み取り
            for (int i = 0; i < numberVariableCount - 1; i++)
            {
                int val = reader.ReadInt(currentOffset, true, out currentOffset);
                //Debug.Log($"数値変数{i}：{val}");
            }

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);

            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            for (int i = 0; i < stringVariableCount; i++)
            {
                string text = reader.ReadString(currentOffset, out currentOffset);
                //Debug.Log($"文字列変数{i}：{text}");
            }

            int footer = reader.ReadByte(currentOffset, out currentOffset);
            // フッタの値が1の場合、動作指定コマンドなので移動ルート読み取りを行う
            if (footer == 1)
            {
                ReadEventMoveRoute(currentOffset, out currentOffset);
            }

            nextOffset = currentOffset;
            return null;
        }

        private EventCommandBase CreateShowTextCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            string text = reader.ReadString(currentOffset, out currentOffset);
            Debug.Log($"文章表示：{text}");
            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return new MessageCommand(text);
        }

        private EventCommandBase CreateShowTextCommand2(string[] stringVariables)
        {
            string text = stringVariables[0];
            Debug.Log($"文章表示：{text}");

            return new MessageCommand(text);
        }

        private EventCommandBase CreateDebugTextCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            string text = reader.ReadString(currentOffset, out currentOffset);
            Debug.Log($"デバッグ文：{text}");
            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        private EventCommandBase CreateChoiceForkCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;

            int forkParams = reader.ReadInt(currentOffset, true, out currentOffset);
            int forkCount = forkParams % (1 << 4);
            int otherForkOption = forkParams / (1 << 8) % (1 << 8);

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            string[] choiceStrings = new string[stringVariableCount];
            for (int i = 0; i < stringVariableCount; i++)
            {
                choiceStrings[i] = reader.ReadString(currentOffset, out currentOffset);
                Debug.Log($"選択肢{i}：{choiceStrings[i]}");
            }

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return new ChoiceForkCommand(indentDepth,choiceStrings);
        }

        private EventCommandBase CreateChoiceForkCommand2(MetaEventCommand metaCommand)
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

        private EventCommandBase CreateFlagForkByVariableCommand(int offset, out int nextOffset, int numberVariableCount)
        {
            int currentOffset = offset;

            int forkParams = reader.ReadInt(currentOffset, true, out currentOffset);
            int forkCount = forkParams % (1 << 4);
            int flagCount = (numberVariableCount - 2) / 3;
            Debug.Log($"条件数：{flagCount}、分岐数：{forkCount}");
            for (int i = 0; i < flagCount; i++)
            {
                int flagLeft = reader.ReadInt(currentOffset, true, out currentOffset);
                int flagRight = reader.ReadInt(currentOffset, true, out currentOffset);
                int rightAndCompareParams = reader.ReadInt(currentOffset, true, out currentOffset);
            }

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        private EventCommandBase CreateFlagForkByVariableCommand2(MetaEventCommand metaCommand)
        {
            int forkParams = metaCommand.NumberArgs[1];
            int forkCount = forkParams % (1 << 4);
            int flagCount = (metaCommand.NumberArgs.Length - 2) / 3;
            Debug.Log($"条件数：{flagCount}、分岐数：{forkCount}");
            for (int i = 0; i < flagCount; i++)
            {
                int flagLeft = metaCommand.NumberArgs[2 + 3 * i];
                int flagRight = metaCommand.NumberArgs[3 + 3 * i];
                int rightAndCompareParams = metaCommand.NumberArgs[4 + 3 * i];
            }

            return null;
        }

        /// <summary>
        /// 分岐始点を示すコマンドを生成
        /// 選択肢、条件分岐共通
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="nextOffset"></param>
        /// <returns></returns>
        private EventCommandBase CreateForkBeginCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int forkNumber = reader.ReadInt(currentOffset, true, out currentOffset);
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);

            Debug.Log($"分岐始点{forkNumber}");

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return new ForkBeginCommand(indentDepth, forkNumber);
        }

        private EventCommandBase CreateForkBeginCommand2(MetaEventCommand metaCommand)
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