using Util.Wolf;
using System;
using System.Collections.Generic;
using UnityEngine;// �y�b��z�{����Expression���C����UnityEngine�Ɉˑ����Ȃ��B�v�݌v������
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

            loader.LoadTypes(WolfConfig.DatabaseType.User, out userDbSchemas, out userDbRecords);
            loader.LoadTypes(WolfConfig.DatabaseType.Changable, out changableDbSchemas, out changableDbRecords);
            loader.LoadTypes(WolfConfig.DatabaseType.System, out systemDbSchemas, out systemDbRecords);
        }

        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            //return new EventCommandBase();

            int typeNo = metaCommand.NumberArgs[1];
            int dataNo = metaCommand.NumberArgs[2];
            int fieldNo = metaCommand.NumberArgs[3];
            int operatorType = metaCommand.NumberArgs[4] & 0x0F;
            int targetDatabase = (metaCommand.NumberArgs[4] >> 8) & 0x0F;
            int modeType = (metaCommand.NumberArgs[4] >> 12) & 0x0F;
            int nameSpecifyConfig = (metaCommand.NumberArgs[4] >> 16) & 0x0F;
            int targetVal = metaCommand.NumberArgs.Length > 5 ? metaCommand.NumberArgs[5] : 0;// ���l�������ϐ�

            // DB�̎Q�Ƃ�ϐ��Ăяo���l�ɕϊ�����
            WolfConfig.DatabaseType dbType;
            WolfDatabaseSchema[] targetSchemas;
            WolfDatabaseRecord[][] targetRecords;
            {
                var loader = new WolfDatabaseLoader();
                switch (targetDatabase)
                {
                    case 0:
                        // ��DB
                        dbType = WolfConfig.DatabaseType.Changable;
                        targetSchemas = changableDbSchemas;
                        targetRecords = changableDbRecords;
                        break;
                    case 1:
                        // �V�X�e��DB
                        dbType = WolfConfig.DatabaseType.System;
                        targetSchemas = systemDbSchemas;
                        targetRecords = systemDbRecords;
                        break;
                    case 2:
                        // ���[�UDB
                        dbType = WolfConfig.DatabaseType.User;
                        targetSchemas = userDbSchemas;
                        targetRecords = userDbRecords;
                        break;
                    default:
                        throw new Exception("�s����DB�^�C�v���w�肳�ꂽ");
                }
            }

            // �ݒ莞�ɕ�����ŃL�[���w�肵�Ă����ꍇ�A���l�Ƃ��ĕϊ�����Ă��Ȃ���Ԃ̉\��������B�i�����炭�ADB�\���ύX�ȂǂɒǏ]�ł��Ă��Ȃ��j
            // ���̂��߁A�f�[�^�\�������[�h���ĕ�����Ō�������K�v������B
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

            OperatorType assignType = GetAssignOperator(operatorType);
            Common.IDataAccessorFactory<int> targetAccessorFactory = new Command.WolfIntAccessorFactory(false, targetVal);
            Common.IDataAccessorFactory<int> databaseAccessorFactory
                = new Command.WolfIntRepositoryAccessorFactory(dbType, typeNo, dataNo, fieldNo);
            // �E�ӑ�񍀂͌Œ�B�������Ȃ�
            OperatorType rightOperatorType = OperatorType.Plus;
            Common.IDataAccessorFactory<int> rightAccessorFactory = new Command.WolfIntAccessorFactory(true, 0);

            if (modeType == 0)
            {
                // DB�ɑ��
                UpdaterInt[] updaters = new UpdaterInt[1];
                updaters[0] = new UpdaterInt(databaseAccessorFactory, targetAccessorFactory, rightAccessorFactory,
                    assignType, rightOperatorType);

                return new ChangeVariableIntCommand(updaters);
            }
            else
            {
                // �ϐ��ɑ��

                UpdaterInt[] updaters = new UpdaterInt[1];
                updaters[0] = new UpdaterInt(targetAccessorFactory, databaseAccessorFactory, rightAccessorFactory,
                    assignType, rightOperatorType);

                return new ChangeVariableIntCommand(updaters);
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
                default:
                    return OperatorType.NormalAssign;
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
