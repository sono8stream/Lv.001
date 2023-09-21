using System;
using System.Collections.Generic;
using Util.Wolf;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class WolfEventCommandFactory
    {
        Dictionary<int, CommandFactory.WolfEventCommandFactoryInterface> factories;

        public WolfEventCommandFactory()
        {
            InitializeFactoryDict();
        }

        public EventCommandBase Create(WolfDataReader reader, int offset, out int nextOffset)
        {

            int currentOffset = offset;
            MetaEventCommand metaCommand = ReadCommand(reader, currentOffset, out currentOffset);

            EventCommandBase command = new EventCommandBase(metaCommand.IndentDepth);

            int commandKey = metaCommand.NumberArgs[0];
            switch (commandKey)
            {
                case 0x00000067:
                    // デバッグ文。処理なし
                    break;
                case 0x00000066:
                    command = CreateChoiceForkCommand(metaCommand);
                    break;
                case 0x0000006F:
                    command = CreateFlagForkByVariableCommand(metaCommand);
                    break;
                case 0x00000079:
                    command = CreateChangeVariableCommand(metaCommand);
                    break;
                case 0x00000082:
                    command = CreateMovePositionCommand(metaCommand);
                    break;
                case 0x00000191:
                    command = CreateForkBeginCommand(metaCommand);
                    break;
                case 0x000001F3:
                    command = CreateForkEndCommand(metaCommand);
                    break;
                default:
                    if (metaCommand.FooterValue == 1)
                    {
                        ReadEventMoveRoute(reader, currentOffset, out currentOffset);
                    }
                    break;
            }

            if (factories.ContainsKey(commandKey))
            {
                command = factories[commandKey].Create(metaCommand);
            }

            nextOffset = currentOffset;
            return command;
        }

        private MetaEventCommand ReadCommand(WolfDataReader reader, int offset, out int nextOffset)
        {
            int currentOffset = offset;

            int numberVariableCount = reader.ReadByte(currentOffset, out currentOffset);
            int[] numberVariables = new int[numberVariableCount];
            for (int i = 0; i < numberVariableCount; i++)
            {
                numberVariables[i] = reader.ReadInt(currentOffset, true, out currentOffset);
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

        private EventCommandBase CreateChoiceForkCommand(MetaEventCommand metaCommand)
        {
            int forkParams = metaCommand.NumberArgs[1];
            int forkCount = forkParams % (1 << 4);
            int otherForkOption = forkParams / (1 << 8) % (1 << 8);

            for (int i = 0; i < metaCommand.StringArgs.Length; i++)
            {
                //Debug.Log($"選択肢{i}：{metaCommand.StringArgs[i]}");
            }

            return new ChoiceForkCommand(metaCommand.IndentDepth, metaCommand.StringArgs);
        }

        private EventCommandBase CreateFlagForkByVariableCommand(MetaEventCommand metaCommand)
        {
            int forkParams = metaCommand.NumberArgs[1];
            int forkCount = forkParams % (1 << 4);
            int flagCount = (metaCommand.NumberArgs.Length - 2) / 3;
            //Debug.Log($"条件数：{flagCount}、分岐数：{forkCount}");
            ConditionInt[] conditions = new ConditionInt[flagCount];
            for (int i = 0; i < flagCount; i++)
            {
                int flagLeft = metaCommand.NumberArgs[2 + 3 * i];
                int flagRight = metaCommand.NumberArgs[3 + 3 * i];
                int rightAndCompareParams = metaCommand.NumberArgs[4 + 3 * i];
                //Debug.Log($"左辺 : {flagLeft}, 右辺 : {flagRight}, 条件ID : {rightAndCompareParams}");

                conditions[i] = GenerateCondition(flagLeft, flagRight, rightAndCompareParams);
            }

            return new ForkByVariableIntCommand(metaCommand.IndentDepth, conditions);
        }

        private ConditionInt GenerateCondition(int flagLeft, int flagRight, int rightAndCompareParams)
        {
            Common.IDataAccessorFactory leftAccessorFactory = new Command.WolfIntAccessorFactory(false, flagLeft);

            Common.IDataAccessorFactory rightAccessorFactory;
            bool isConst = (rightAndCompareParams >> 4) > 0;
            rightAccessorFactory = new Command.WolfIntAccessorFactory(isConst, flagRight);

            OperatorType operatorType = (OperatorType)Enum.ToObject(typeof(OperatorType), rightAndCompareParams % (1 << 4));

            var condition = new ConditionInt(leftAccessorFactory, rightAccessorFactory, operatorType);
            return condition;
        }

        private EventCommandBase CreateChangeVariableCommand(MetaEventCommand metaCommand)
        {
            int leftParamRef = metaCommand.NumberArgs[1];
            int rightParamRef1 = metaCommand.NumberArgs[2];
            int rightParamRef2 = metaCommand.NumberArgs[3];
            int valueCondition = metaCommand.NumberArgs[4] % 0x100;
            int operatorType = (metaCommand.NumberArgs[4] / 0x100) % 0x100;
            bool isSequential = (metaCommand.NumberArgs[4] / 0x10000) % 0x100 > 0;

            Common.IDataAccessorFactory leftAccessorFactory = new Command.WolfIntAccessorFactory(false, leftParamRef);
            Common.IDataAccessorFactory rightAccessor1Factory = new Command.WolfIntAccessorFactory(false, rightParamRef1);
            Common.IDataAccessorFactory rightAccessor2Factory = new Command.WolfIntAccessorFactory(false, rightParamRef2);
            OperatorType assignType = GetAssignOperator(operatorType % 0x10);
            OperatorType rightOperatorType = GetCalculateOperator(operatorType / 0x10);

            VariableUpdater[] updaters = new VariableUpdater[1];
            updaters[0] = new VariableUpdater(leftAccessorFactory, rightAccessor1Factory, rightAccessor2Factory,
                assignType, rightOperatorType);

            return new ChangeVariableIntCommand(metaCommand.IndentDepth, updaters);
        }

        private OperatorType GetCalculateOperator(int value)
        {
            switch (value)
            {
                case 0x0F:
                    return OperatorType.ArcTan;
                case 0x00:
                    return OperatorType.Plus;
                case 0x01:
                    return OperatorType.Minus;
                case 0x02:
                    return OperatorType.Multiply;
                case 0x03:
                    return OperatorType.Divide;
                case 0x04:
                    return OperatorType.Mod;
                case 0x05:
                    return OperatorType.And;
                case 0x06:
                    return OperatorType.Random;
                default:
                    return OperatorType.Plus;
            }
        }

        private OperatorType GetAssignOperator(int value)
        {
            switch (value)
            {
                case 0x00:
                    return OperatorType.NormalAssign;
                case 0x01:
                    return OperatorType.PlusAssign;
                case 0x02:
                    return OperatorType.MinusAssign;
                case 0x03:
                    return OperatorType.MultiplyAssign;
                case 0x04:
                    return OperatorType.DivideAssign;
                case 0x05:
                    return OperatorType.ModAssign;
                case 0x06:
                    return OperatorType.MaxAssign;
                case 0x07:
                    return OperatorType.MinAssign;
                case 0x08:
                    return OperatorType.AbsAssign;
                case 0x09:
                    return OperatorType.AngleAssign;
                case 0x0A:
                    return OperatorType.SinAssign;
                case 0x0B:
                    return OperatorType.CosAssign;
                default:
                    return OperatorType.NormalAssign;
            }
        }

        private EventCommandBase CreateMovePositionCommand(MetaEventCommand metaCommand)
        {
            //Debug.Log("場所移動");
            EventId eventId = new EventId(metaCommand.NumberArgs[1]);
            int x = metaCommand.NumberArgs[2];
            int y = metaCommand.NumberArgs[3];
            MapId mapId = new MapId(metaCommand.NumberArgs[4]);
            // 【暫定】移動の各種詳細フラグは追って実装（精密座標指定、トランジションの設定）

            return new MovePositionCommand(metaCommand.IndentDepth, eventId, x, y, mapId);
        }

        private EventCommandBase CreateForkBeginCommand(MetaEventCommand metaCommand)
        {
            int forkNumber = metaCommand.NumberArgs[1];

            //Debug.Log($"分岐始点{forkNumber}");

            return new ForkBeginCommand(metaCommand.IndentDepth, forkNumber);
        }

        private EventCommandBase CreateForkEndCommand(MetaEventCommand metaCommand)
        {
            //Debug.Log($"分岐終端");

            return new ForkEndCommand(metaCommand.IndentDepth);
        }

        // 【暫定】モデル定義までデータを空読み
        private void ReadEventMoveRoute(WolfDataReader reader, int offset, out int nextOffset)
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

        private void InitializeFactoryDict()
        {
            factories = new Dictionary<int, CommandFactory.WolfEventCommandFactoryInterface>();
            factories.Add(0x00000065, new CommandFactory.WolfShowTextCommandFactory());
            factories.Add(0x00000096, new CommandFactory.WolfPictureCommandFactory());
            factories.Add(0x000000AA, new CommandFactory.WolfLoopStartCommandFactory(true));
            factories.Add(0x000000AB, new CommandFactory.WolfLoopBreakCommandFactory());
            factories.Add(0x000000B3, new CommandFactory.WolfLoopStartCommandFactory(false));
            factories.Add(0x000000D2, new CommandFactory.WolfCallEventByIdCommandFactory());
            factories.Add(0x000000FA, new CommandFactory.WolfOperateDbCommandFactory());
            factories.Add(0x0000012C, new CommandFactory.WolfCallEventByNameCommandFactory());
            factories.Add(0x000001F2, new CommandFactory.WolfLoopEndCommandFactory());
        }
    }
}
