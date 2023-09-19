using Util.Wolf;
using System;
using System.Collections.Generic;
using UnityEngine;// 【暫定】本来はExpressionレイヤはUnityEngineに依存しない。要設計見直し
using Infrastructure;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfOperateDbCommandFactory : WolfEventCommandFactoryInterface
    {
        private WolfDatabaseSchema[] userDbSchemas;
        private WolfDatabaseSchema[] changableDbSchemas;
        private WolfDatabaseSchema[] systemDbSchemas;

        private WolfDatabaseRecord[][] userDbRecords;
        private WolfDatabaseRecord[][] changableDbRecords;
        private WolfDatabaseRecord[][] systemDbRecords;

        public WolfOperateDbCommandFactory()
        {
            WolfDatabaseLoader loader = new WolfDatabaseLoader();

            loader.LoadDatabaseRaw(WolfConfig.DatabaseType.User, out userDbSchemas, out userDbRecords);
            loader.LoadDatabaseRaw(WolfConfig.DatabaseType.Changable, out changableDbSchemas, out changableDbRecords);
            loader.LoadDatabaseRaw(WolfConfig.DatabaseType.System, out systemDbSchemas, out systemDbRecords);
        }

        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            int typeNo = metaCommand.NumberArgs[1];
            int dataNo = metaCommand.NumberArgs[2];
            int fieldNo = metaCommand.NumberArgs[3];
            int operatorType = metaCommand.NumberArgs[4] & 0xF0;
            int targetDatabase = (metaCommand.NumberArgs[4] >> 8) & 0x0F;
            int modeType = (metaCommand.NumberArgs[4] >> 12) & 0x0F;
            int nameSpecifyConfig = (metaCommand.NumberArgs[4] >> 16) & 0x0F;
            int targetVal = metaCommand.NumberArgs.Length > 5 ? metaCommand.NumberArgs[5] : 0;// 数値か代入先変数

            // DBの参照を変数呼び出し値に変換する
            WolfConfig.DatabaseType dbType;
            WolfDatabaseSchema[] targetSchemas;
            WolfDatabaseRecord[][] targetRecords;
            {
                var loader = new WolfDatabaseLoader();
                switch (targetDatabase)
                {
                    case 0:
                        // 可変DB
                        dbType = WolfConfig.DatabaseType.Changable;
                        targetSchemas = changableDbSchemas;
                        targetRecords = changableDbRecords;
                        break;
                    case 1:
                        // システムDB
                        dbType = WolfConfig.DatabaseType.System;
                        targetSchemas = systemDbSchemas;
                        targetRecords = systemDbRecords;
                        break;
                    case 2:
                        // ユーザDB
                        dbType = WolfConfig.DatabaseType.User;
                        targetSchemas = userDbSchemas;
                        targetRecords = userDbRecords;
                        break;
                    default:
                        throw new Exception("不正なDBタイプを指定された");
                }
            }

            // 設定時に文字列でキーを指定していた場合、数値として変換されていない状態の可能性がある。（おそらく、DB構成変更などに追従できていない）
            // このため、データ構成をロードして文字列で検索する必要がある。
            if ((nameSpecifyConfig & 0x01) > 0)
            {
                typeNo = Array.FindIndex(targetSchemas, schema => schema.Name == metaCommand.StringArgs[1]);
            }
            if ((nameSpecifyConfig & 0x02) > 0)
            {
                dataNo = Array.FindIndex(targetRecords[typeNo], record => record.Name == metaCommand.StringArgs[2]);
            }
            if ((nameSpecifyConfig & 0x04) > 0)
            {
                fieldNo = Array.FindIndex(targetSchemas[typeNo].Columns, field => field.Name == metaCommand.StringArgs[3]);
            }

            // 【暫定】文字列が与えられることもある。数値・文字列を意識しない実装にしたい
            OperatorType assignType = GetAssignOperator(operatorType);
            Common.IDataAccessorFactory<int> targetAccessorFactory = new Command.WolfIntAccessorFactory(false, targetVal);
            Common.IDataAccessorFactory<int> databaseAccessorFactory
                = new Command.WolfIntRepositoryAccessorFactory(dbType, typeNo, dataNo, fieldNo);
            // 右辺第二項は固定。何もしない
            OperatorType rightOperatorType = OperatorType.Plus;
            Common.IDataAccessorFactory<int> rightAccessorFactory = new Command.WolfIntAccessorFactory(true, 0);

            if (modeType == 0)
            {
                // DBに代入
                VariableUpdater[] updaters = new VariableUpdater[1];
                updaters[0] = new VariableUpdater(databaseAccessorFactory, targetAccessorFactory, rightAccessorFactory,
                    assignType, rightOperatorType);

                return new ChangeVariableIntCommand(metaCommand.IndentDepth, updaters);
            }
            else
            {
                // 変数に代入

                VariableUpdater[] updaters = new VariableUpdater[1];
                updaters[0] = new VariableUpdater(targetAccessorFactory, databaseAccessorFactory, rightAccessorFactory,
                    assignType, rightOperatorType);

                return new ChangeVariableIntCommand(metaCommand.IndentDepth, updaters);
            }
        }

        private OperatorType GetAssignOperator(int value)
        {
            switch (value)
            {
                case 0x00:
                    return OperatorType.NormalAssign;
                case 0x10:
                    return OperatorType.PlusAssign;
                case 0x20:
                    return OperatorType.MinusAssign;
                case 0x30:
                    return OperatorType.MultiplyAssign;
                case 0x40:
                    return OperatorType.DivideAssign;
                case 0x50:
                    return OperatorType.ModAssign;
                case 0x60:
                    return OperatorType.MaxAssign;
                case 0x70:
                    return OperatorType.MinAssign;
                default:
                    throw new Exception("想定外の割り当てタイプ");
            }
        }

        private OperatorType GetCalculateOperator(int value)
        {
            switch (value)
            {
                case 0xF0:
                    return OperatorType.ArcTan;
                case 0x00:
                    return OperatorType.Plus;
                case 0x10:
                    return OperatorType.Minus;
                case 0x20:
                    return OperatorType.Multiply;
                case 0x30:
                    return OperatorType.Divide;
                case 0x40:
                    return OperatorType.Mod;
                case 0x50:
                    return OperatorType.And;
                case 0x60:
                    return OperatorType.Random;
                default:
                    return OperatorType.Plus;
            }
        }
    }
}
