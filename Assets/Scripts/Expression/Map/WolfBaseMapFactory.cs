using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class WolfBaseMapFactory
    {
        // 【暫定】マップチップのピクセル数は16で固定とする
        //          マップ描画時など至る所で使用するので，どう使いまわすかが課題
        //          MapDataに含める?
        protected const int PIXEL_PER_GRID = 16;
        protected MapId mapId;

        public WolfBaseMapFactory(MapId mapId)
        {
            this.mapId = mapId;
        }

        protected MapEvent.EventData[] ReadMapEvents(Util.Wolf.WolfDataReader reader, Texture2D mapTexture, int offset)
        {
            List<MapEvent.EventData> list = new List<MapEvent.EventData>();
            int headerByte = reader.ReadByte(offset, out offset);
            while (headerByte != 0x66)
            {
                // ヘッダーの余り部分をスキップ
                for (int i = 0; i < 4; i++)
                {
                    reader.ReadByte(offset, out offset);
                }
                MapEvent.EventId eventId = new MapEvent.EventId(
                    reader.ReadInt(offset, true, out offset));
                Debug.Log($"イベントID：{eventId.Value}");
                string eventName = reader.ReadString(offset, out offset);
                int posX = reader.ReadInt(offset, true, out offset);
                int posY = reader.ReadInt(offset, true, out offset);
                int pageCount = reader.ReadInt(offset, true, out offset);
                Debug.Log($"ページ数：{pageCount}");
                // 00 00 00 00のスキップ
                reader.ReadInt(offset, true, out offset);

                List<MapEvent.EventPageData> eventPages = new List<MapEvent.EventPageData>();
                for (int i = 0; i < pageCount; i++)
                {
                    // イベントページの読み込み
                    eventPages.Add(ReadEventPageData(reader, mapTexture, eventId, offset, out offset));
                }

                // フッタースキップ
                reader.ReadByte(offset, out offset);

                list.Add(new MapEvent.EventData(eventId, posX, posY, eventPages.ToArray()));

                // 次の計算用にヘッダを更新
                int nextHeaderByte = reader.ReadByte(offset, out offset);
                headerByte = nextHeaderByte;
            }
            Debug.Log(list.Count);

            return list.ToArray();
        }

        private MapEvent.EventPageData ReadEventPageData(Util.Wolf.WolfDataReader reader, Texture2D mapTexture,
            MapEvent.EventId eventId, int offset, out int nextOffset)
        {
            // ヘッダースキップ
            int hh = reader.ReadByte(offset, out offset);
            int tileNo = reader.ReadInt(offset, true, out offset);
            string chipImgName = reader.ReadString(offset, out offset);
            Debug.Log(chipImgName);
            int directionVal = reader.ReadByte(offset, out offset);
            int animNo = reader.ReadByte(offset, out offset);
            int charaAlpha = reader.ReadByte(offset, out offset);
            int showType = reader.ReadByte(offset, out offset);// 通常/加算/減算/乗算
            int triggerTypeVal = reader.ReadByte(offset, out offset);
            MapEvent.EventTriggerType triggerType = ConvertTriggerValueToTriggerType(triggerTypeVal);

            int triggerFlagOpr1 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr2 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr3 = reader.ReadByte(offset, out offset);
            int triggerFlagOpr4 = reader.ReadByte(offset, out offset);
            int triggerFlagLeft1 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft2 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft3 = reader.ReadInt(offset, true, out offset);
            int triggerFlagLeft4 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight1 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight2 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight3 = reader.ReadInt(offset, true, out offset);
            int triggerFlagRight4 = reader.ReadInt(offset, true, out offset);

            MapEvent.EventMoveData moveData = ReadEventMoveRoute(reader, offset, out offset);

            int eventCommandCount = reader.ReadInt(offset, true, out offset);
            Debug.Log($"イベントコマンド数：{eventCommandCount}");
            // デバッグここまでOK
            MapEvent.EventCommandBase[] commands = ReadEventCommands(reader, eventCommandCount, offset, out offset);

            // イベントコマンドフッタースキップ
            reader.ReadInt(offset, true, out offset);

            int shadowNo = reader.ReadByte(offset, out offset);
            int rangeExtendX = reader.ReadByte(offset, out offset);
            int rangeExtendY = reader.ReadByte(offset, out offset);

            // フッタースキップ
            int ff = reader.ReadByte(offset, out offset);

            nextOffset = offset;
            Direction direction = ConvertDirectionValueToDirection(directionVal);

            Texture2D texture = null;
            bool haveDirection = false;
            if (tileNo == -1)
            {
                if (string.IsNullOrEmpty(chipImgName))
                {
                    haveDirection = false;
                }
                else
                {
                    // キャラチップから画像を取得する
                    // 【暫定】読み込めなかった場合のエラー処理を追加
                    texture = new Texture2D(1, 1);
                    string imagePath = $"{Application.streamingAssetsPath}/Data/" + chipImgName;
                    byte[] charaTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
                    texture.LoadImage(charaTexBytes);
                    texture.Apply();

                    haveDirection = true;
                }
            }
            else
            {
                // マップタイルから画像を取得する
                texture = new Texture2D(PIXEL_PER_GRID, PIXEL_PER_GRID);
                Color[] c = mapTexture.GetPixels(PIXEL_PER_GRID * (tileNo % 8),
                    mapTexture.height - PIXEL_PER_GRID * (tileNo / 8 + 1), PIXEL_PER_GRID, PIXEL_PER_GRID);
                texture.SetPixels(0, 0, PIXEL_PER_GRID, PIXEL_PER_GRID, c);
                texture.Apply();
                haveDirection = false;
            }
            Debug.Log(haveDirection);

            return new MapEvent.EventPageData(texture, direction, haveDirection, triggerType, commands, moveData);
        }

        private Direction ConvertDirectionValueToDirection(int directionVal)
        {
            switch (directionVal)
            {
                case 1:
                    return Direction.DownLeft;
                case 2:
                    return Direction.Down;
                case 3:
                    return Direction.DownRight;
                case 4:
                    return Direction.Left;
                case 5:
                    return Direction.Down;
                case 6:
                    return Direction.Right;
                case 7:
                    return Direction.UpLeft;
                case 8:
                    return Direction.Up;
                case 9:
                    return Direction.UpRight;
                default:
                    return Direction.Down;
            }
        }

        private MapEvent.EventTriggerType ConvertTriggerValueToTriggerType(int triggerVal)
        {
            switch (triggerVal)
            {
                case 0:
                    return MapEvent.EventTriggerType.OnCheck;
                case 1:
                    return MapEvent.EventTriggerType.Auto;
                case 2:
                    return MapEvent.EventTriggerType.Parallel;
                case 3:
                    return MapEvent.EventTriggerType.OnPlayerContact;
                case 4:
                    return MapEvent.EventTriggerType.OnEventContact;
                default:
                    throw new System.NotImplementedException();
            }
        }

        // 【暫定】モデル定義までデータを空読み
        private MapEvent.EventMoveData ReadEventMoveRoute(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset)
        {
            int animationSpeed = reader.ReadByte(offset, out offset);
            int moveSpeed = reader.ReadByte(offset, out offset);
            int moveFrequency = reader.ReadByte(offset, out offset);
            int moveType = reader.ReadByte(offset, out offset);
            int optionType = reader.ReadByte(offset, out offset);
            bool canPass = (optionType & 8) > 0;
            int moveFlag = reader.ReadByte(offset, out offset);
            int commandCount = reader.ReadInt(offset, true, out offset);
            Debug.Log($"移動コマンド数：{commandCount}");

            // 動作コマンド
            for (int i = 0; i < commandCount; i++)
            {
                int commandType = reader.ReadByte(offset, out offset);
                int variableCount = reader.ReadByte(offset, out offset);
                Debug.Log($"コマンドタイプ：{commandType}、変数の数： {variableCount}");
                for (int j = 0; j < variableCount; j++)
                {
                    int variableValue = reader.ReadInt(offset, true, out offset);
                    Debug.Log($"変数{j}：{variableValue}");
                }

                // 終端
                int footer1 = reader.ReadByte(offset, out offset);
                int footer2 = reader.ReadByte(offset, out offset);
                Debug.Log($"移動コマンド　フッタ：{footer1} {footer2}");
            }

            nextOffset = offset;
            return new MapEvent.EventMoveData(canPass);
        }

        // 【暫定】詳細定義していないコマンドは空読み
        private MapEvent.EventCommandBase[] ReadEventCommands(Util.Wolf.WolfDataReader reader,
            int eventCommandCount, int offset, out int nextOffset)
        {
            int currentOffset = offset;
            MapEvent.WolfEventCommandFactory factory = new MapEvent.WolfEventCommandFactory(reader, currentOffset);
            List<MapEvent.EventCommandBase> commands = new List<MapEvent.EventCommandBase>();
            for (int i = 0; i < eventCommandCount; i++)
            {
                // 一つ一つのコマンドを読み取る
                commands.Add(factory.Create(out currentOffset));
            }
            nextOffset = currentOffset;

            return commands.ToArray();
        }
    }

}