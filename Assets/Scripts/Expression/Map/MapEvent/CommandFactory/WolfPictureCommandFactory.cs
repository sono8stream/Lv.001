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
                PicturePivotPattern posPattern = GetPosPattern(metaCommand.NumberArgs[2]);
                int x = metaCommand.NumberArgs[8];
                int y = metaCommand.NumberArgs[9];

                return new ShowPictureCommand(imagePath, posPattern, x, y);
            }
            else
            {
                return new EventCommandBase();
            }
        }

        private PicturePivotPattern GetPosPattern(int posPattern)
        {
            switch (posPattern)
            {
                case 0x00:
                    // 左上
                    // 移動も00だが、これは必要になったら実装
                    // Ver3では中央上・下も追加されたが、これも必要になったら追加
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