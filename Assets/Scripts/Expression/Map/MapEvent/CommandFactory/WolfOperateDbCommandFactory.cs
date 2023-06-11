using Util.Wolf;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;// �{����Expression���C����UnityEngine�Ɉˑ����Ȃ��B�v�݌v������

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfOperateDbCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            return new EventCommandBase();
            
            int typeNo = metaCommand.NumberArgs[1];
            int dataNo = metaCommand.NumberArgs[2];
            int fieldNo = metaCommand.NumberArgs[3];
            int valueType = metaCommand.NumberArgs[4] & 0x0F;
            int operatorType = (metaCommand.NumberArgs[4] >> 4) & 0x0F;
            int targetDatabase = (metaCommand.NumberArgs[4] >> 8) & 0x0F;
            int modeType = (metaCommand.NumberArgs[4] >> 12) & 0x0F;
            int nameSpecifyConfig = (metaCommand.NumberArgs[4] >> 16) & 0x0F;
            int targetVal = metaCommand.NumberArgs[5];// ���l�������ϐ�

            // DB�̎Q�Ƃ�ϐ��Ăяo���l�ɕϊ�����
            int databaseCallVal = 0;
            {
                switch (targetDatabase)
                {
                    case 0:
                        // ��DB
                        databaseCallVal = 1100000000;
                        break;
                    case 1:
                        // �V�X�e��DB
                        databaseCallVal = 1300000000;
                        break;
                    case 2:
                        // ���[�UDB
                        databaseCallVal = 1000000000;
                        break;
                }

                {
                    // �Q�Ƃɕ����񂪊܂܂�Ă����ꍇ�A�ϊ�����
                }
            }

            OperatorType assignType = GetAssignOperator(operatorType);
            Common.IDataAccessorFactory<int> targetAccessorFactory = new Command.WolfIntAccessorFactory(false, targetVal);
            Common.IDataAccessorFactory<int> databaseAccessorFactory = new Command.WolfIntAccessorFactory(false, databaseCallVal);
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