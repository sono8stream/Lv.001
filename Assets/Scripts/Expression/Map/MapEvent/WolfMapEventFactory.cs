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

        public EventData Create(out int nextOffset)
        {
            int offset = startOffset;
            const int footerLength = 4;
            int[] footer = new int[footerLength] { 0x03, 0x00, 0x00, 0x00 };
            // フッターまで空読み
            // これで乗り切れると乗り切れないときがある
            // Dungeon.mpsでイベントID1が乗り切れなかった
            while (true)
            {
                if (footer[0] != reader.ReadByte(offset, out offset))
                {
                    continue;
                }

                Debug.Log(reader.ReadByte(offset - 1, out offset));
                bool isFooter = true;
                int tmpNextOffset = offset;
                for (int i = 1; i < footerLength; i++)
                {
                    if (footer[i] != reader.ReadByte(tmpNextOffset, out tmpNextOffset))
                    {
                        isFooter = false;
                        break;
                    }
                }

                if (isFooter)
                {
                    offset = tmpNextOffset;
                    break;
                }
                else
                {
                    continue;
                }
            }

            nextOffset = offset;
            return null;
        }

        public EventData Create2(out int nextOffset)
        {
            int currentOffset = startOffset;
            int variableCount = reader.ReadByte(currentOffset, out currentOffset);
            int commandType = reader.ReadInt(currentOffset, true, out currentOffset);
            Debug.Log(commandType.ToString("X8"));

            switch (commandType)
            {
                case 0x00000065:
                    CreateShowTextCommand(currentOffset, out currentOffset);
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
                case 0x00000079:
                    // 変数操作
                    break;
                case 0x00000191:
                    CreateForkBeginByVariableCommand(currentOffset, out currentOffset);
                    break;
                case 0x000000D2:
                    CreateCallEventByIdCommand(currentOffset, out currentOffset);
                    break;
                default:
                    break;
            }

            nextOffset = currentOffset;
            startOffset = currentOffset;
            return null;
        }

        private EventData CreateShowTextCommand(int offset, out int nextOffset)
        {
            int currentOffset = offset;
            int indentDepth = reader.ReadByte(currentOffset, out currentOffset);
            int stringVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            string text = reader.ReadString(currentOffset, out currentOffset);
            Debug.Log($"文章表示：{text}");
            // フッタはスキップ
            reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }

        private EventData CreateDebugTextCommand(int offset, out int nextOffset)
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

        private EventData CreateChoiceForkCommand(int offset, out int nextOffset)
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

        private EventData CreateFlagForkByVariableCommand(int offset, out int nextOffset, int numberVariableCount)
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

        private EventData CreateForkBeginByVariableCommand(int offset, out int nextOffset)
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
        private EventData CreateCallEventByIdCommand(int offset, out int nextOffset)
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
                Debug.Log($"数値引数{i.ToString()}：{numberArg.ToString()}");
            }

            for (int i = 0; i < stringArgCount; i++)
            {
                int stringArgVariableValue = reader.ReadInt(currentOffset, true, out currentOffset);
                Debug.Log($"文字列引数呼び出し値{i.ToString()}：{stringArgVariableValue.ToString()}");
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
                Debug.Log($"文字データ：{text}");
            }

            // フッタはスキップ
            int ft = reader.ReadByte(currentOffset, out currentOffset);

            nextOffset = currentOffset;
            return null;
        }
    }
}