using  Expression.Map.MapEvent.Command;

namespace Expression.Map.MapEvent.CommandFactory
{
    public class WolfPictureCommandFactory : WolfEventCommandFactoryInterface
    {
        public EventCommandBase Create(MetaEventCommand metaCommand)
        {
            if (metaCommand.StringArgs.Length > 0)
            {
                string imagePath = metaCommand.StringArgs[0];
                int pictureId = metaCommand.NumberArgs[2];
                PicturePivotPattern posPattern = GetPosPattern(metaCommand.NumberArgs[2]);
                int x = metaCommand.NumberArgs[8];
                int y = metaCommand.NumberArgs[9];
                float scale = metaCommand.NumberArgs[10] * 0.01f;// �g�嗦�BX/Y�ʃJ�E���g�̃P�[�X�͖�����

                return new ShowPictureCommand(pictureId, imagePath, posPattern, x, y, scale);
            }
            else
            {
                if ((metaCommand.NumberArgs[1] & 0xFF) == 0x02)// ����
                {
                    int pictureId = metaCommand.NumberArgs[2];
                    return new RemovePictureCommand(pictureId);
                }
                return new EventCommandBase();
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