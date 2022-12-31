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
                PicturePosPattern posPattern= GetPosPattern(metaCommand.NumberArgs[2]);
                return new ShowPictureCommand(imagePath, posPattern, 0, 0);
            }
            else
            {
                return new EventCommandBase();
            }
        }

        private PicturePosPattern GetPosPattern(int posPattern)
        {
            switch (posPattern)
            {
                case 0x00:
                    // ����
                    // �ړ���00�����A����͕K�v�ɂȂ��������
                    // Ver3�ł͒�����E�����ǉ����ꂽ���A������K�v�ɂȂ�����ǉ�
                    return PicturePosPattern.LeftTop;
                case 0x10:
                    return PicturePosPattern.Center;
                case 0x20:
                    return PicturePosPattern.LeftBottom;
                case 0x30:
                    return PicturePosPattern.RightTop;
                case 0x40:
                    return PicturePosPattern.RightBottom;
                default:
                    return PicturePosPattern.LeftTop;
            }
        }
    }
}