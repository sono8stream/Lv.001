using System;
using UnityEngine;

namespace Expression.Map.MapEvent
{
    public class EventPageData
    {
        private Texture2D Texture { get; set; }

        private Direction Direction { get; set; }

        private bool HaveDirection { get; set; }

        public EventTriggerType TriggerType { get; private set; }

        public EventCommandBase[] CommandDataArray
        { get; private set; }

        public EventMoveData MoveData { get; private set; }

        public EventPageData(Texture2D texture, Direction initDirection, bool haveDirection,
            EventTriggerType triggerType, EventCommandBase[] commandDataArray, EventMoveData moveData)
        {
            Texture = texture;
            Direction = initDirection;
            HaveDirection = haveDirection;
            TriggerType = triggerType;
            CommandDataArray = commandDataArray;
            MoveData = moveData;
        }

        // 現在の方向に応じた画像を返す
        public Texture2D GetCurrentTexture()
        {
            Debug.Log(HaveDirection);
            if (HaveDirection)
            {
                // 【暫定】WOLF向け画像かつ8方向のみ想定
                int widthPerDirection = Texture.width / 6;
                int heightPerDirection = Texture.height / 4;
                int x = 0;
                int y = 0;
                switch (Direction)
                {
                    case Direction.Up:
                        x = widthPerDirection * 1;
                        y = heightPerDirection * 0;
                        break;
                    case Direction.UpRight:
                        x = widthPerDirection * 4;
                        y = heightPerDirection * 0;
                        break;
                    case Direction.Right:
                        x = widthPerDirection * 1;
                        y = heightPerDirection * 1;
                        break;
                    case Direction.DownRight:
                        x = widthPerDirection * 4;
                        y = heightPerDirection * 2;
                        break;
                    case Direction.Down:
                        x = widthPerDirection * 1;
                        y = heightPerDirection * 3;
                        break;
                    case Direction.DownLeft:
                        x = widthPerDirection * 4;
                        y = heightPerDirection * 3;
                        break;
                    case Direction.Left:
                        x = widthPerDirection * 1;
                        y = heightPerDirection * 2;
                        break;
                    case Direction.UpLeft:
                        x = widthPerDirection * 4;
                        y = heightPerDirection * 1;
                        break;
                    default:
                        break;
                }
                Color[] colors = Texture.GetPixels(x, y, widthPerDirection, heightPerDirection);
                Texture2D texture = new Texture2D(widthPerDirection, heightPerDirection);
                texture.SetPixels(0, 0, widthPerDirection, heightPerDirection, colors);
                texture.Apply();
                return texture;
            }
            else
            {
                return Texture;
            }
        }

        public bool CanPass()
        {
            if (Texture == null)
            {
                return true;
            }
            else
            {
                return MoveData.CanPass;
            }
        }
    }
}