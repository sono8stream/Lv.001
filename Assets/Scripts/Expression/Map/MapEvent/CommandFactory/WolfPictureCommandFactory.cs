using  Expression.Map.MapEvent.Command;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfPictureCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            int operationType = metaCommand.NumberArgs[1] & 0x0F;
            if (operationType == 0x00)
            {
                int sourceType = (metaCommand.NumberArgs[1] >> 4) & 0x0F;
                if (sourceType == 0x00)
                {
                    string imagePath = metaCommand.StringArgs[0];


                    int pivot = (metaCommand.NumberArgs[1] >> 0x100) & 0xFF;
                    PicturePivotPattern posPattern = GetPosPattern(pivot);


                    int pictureId = metaCommand.NumberArgs[2];
                    int x = metaCommand.NumberArgs[8];
                    int y = metaCommand.NumberArgs[9];
                    float scale = metaCommand.NumberArgs[10] * 0.01f;// �g�嗦�BX/Y�ʃJ�E���g�̃P�[�X�͖�����

                    return new ShowPictureCommand(metaCommand.IndentDepth, pictureId,
                        imagePath, posPattern, x, y, scale);
                }

                return new EventCommandBase(metaCommand.IndentDepth);
            }
            else if (operationType == 0x02)
            {
                int pictureId = metaCommand.NumberArgs[2];
                return new RemovePictureCommand(metaCommand.IndentDepth, pictureId);
            }
            else
            {
                return new EventCommandBase(metaCommand.IndentDepth);
            }
        }

        private PicturePivotPattern GetPosPattern(int posPattern)
        {
            switch (posPattern)
            {
                case 0x00:
                    // ����
                    // �ړ���00�����A����͕K�v�ɂȂ��������
                    // Ver3�ł͒�����E�����ǉ����ꂽ���A������K�v�ɂȂ�����ǉ�
                    return PicturePivotPattern.LeftTop;
                case 0x10:
                    return PicturePivotPattern.Center;
                case 0x20:
                    return PicturePivotPattern.LeftBottom;
                case 0x30:
                    return PicturePivotPattern.RightTop;
                case 0x40:
                    return PicturePivotPattern.RightBottom;
                default:
                    return PicturePivotPattern.LeftTop;
            }
        }
    }
}