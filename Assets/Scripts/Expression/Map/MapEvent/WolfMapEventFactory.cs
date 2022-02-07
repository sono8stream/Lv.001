using System;
using Util.Wolf;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class WolfMapEventFactory
    {
        WolfDataReader reader;
        int startOffset;

        public WolfMapEventFactory(WolfDataReader reader, int startOffset)
        {
            this.reader = reader;
            this.startOffset = startOffset;
        }

        public EventCommand Create(out int nextOffset)
        {
            int currentOffset = startOffset;
            int variableCount = reader.ReadByte(currentOffset, out currentOffset);
            int commandType = reader.ReadInt(currentOffset, true, out currentOffset);
            //Debug.Log(commandType.ToString("X8"));
            EventCommand command = new EventCommand();

            switch (commandType)
            {
                case 0x00000065:
                    command = CreateShowTextCommand(currentOffset, out currentOffset);
                    break;
                case 0x00000067:
                    CreateDebugTextCommand(currentOffset, out currentOffset);
                    break;
                case 0x00000066:
                    CreateChoiceForkCommand(currentOffset, out currentOffset);
                    break;
                case 0x0000006F:
                    CreateFlagForkByVariableCommand(currentOffset, out currentOffset, variableCount);
                    break;
                case 0x00000191:
                    CreateForkBeginByVariableCommand(currentOffset, out currentOffset);
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

        // すべてのコマンドで共通の設定
        // ただし動作指定コマンドのみフッタの後に動作指定イベントが連続する
        private EventCommand CreateDefaultCommand(int offset, out int nextOffset, int numberVariableCount)
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

        private EventCommand CreateShowTextCommand(int offset, out int nextOffset)
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

        private EventCommand CreateDebugTextCommand(int offset, out int nextOffset)
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

        private EventCommand CreateChoiceForkCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;

            int forkParams = reader.ReadInt(currentOffset, true, out currentOffset);
            int forkCount = forkParams % (1 << 4);
            int otherForkOption = forkParams / (1 << 8) % (1 << 8);

            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            for (int i = 0; i < stringVariableCount; i++)
            {
                string text = reader.ReadString(currentOffset, out currentOffset);
                Debug.Log($"選択肢{i}：{text}");
            }

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        private EventCommand CreateFlagForkByVariableCommand(int offset, out int nextOffset, int numberVariableCount)
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

        private EventCommand CreateForkBeginByVariableCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int forkNumber = reader.ReadInt(currentOffset, true, out currentOffset);
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);

            Debug.Log($"分岐始点{forkNumber}");

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        private EventCommand CreateOperateVariableCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int forkNumber = reader.ReadInt(currentOffset, true, out currentOffset);
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);

            Debug.Log($"分岐始点{forkNumber}");

            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        // イベントコマンドの取得は未済み
        private EventCommand CreateCallEventByIdCommand(int offset, out int nextOffset)
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