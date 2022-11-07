using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class WolfBaseMapFactory
    {
        // 【暫定】マップチップのピクセル数は16で固定とする
        //          マップ描画時など至る所で使用するので，どう使いまわすかが課題
        //          MapDataに含める?
        private const int PIXEL_PER_GRID = 16;
        private MapId mapId;

        public WolfBaseMapFactory(MapId mapId)
        {
            this.mapId = mapId;
        }

        public BaseMapData Create(string mapFilePath)
        {
            // マップファイルからタイル情報を読み出し
            // タイル情報からテクスチャ読み込み
            // テクスチャとマップファイルからマップ生成

            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(mapFilePath);

            int tileSetId = reader.ReadInt(0x22, true, out int tmp);
            MapTile.WolfRepository repository = new MapTile.WolfRepository();
            MapTile.TileData tileData = repository.Find(tileSetId);

            Texture2D mapchipTexture = new Texture2D(1, 1);
            int autoTileCount = 16;
            Texture2D[] autochipTextures = new Texture2D[autoTileCount];
            {
                // 【暫定】ファイルを読み込めなかった場合のエラー処理
                string imagePath = $"{Application.streamingAssetsPath}/Data/" + tileData.BaseTileFilePath;
                byte[] baseTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
                mapchipTexture.LoadImage(baseTexBytes);
                mapchipTexture.Apply();

                for (int i = 1; i < autoTileCount; i++)
                {
                    string autochipImagePath = $"{Application.streamingAssetsPath}/Data/" + tileData.AutoTileFilePaths[i - 1];
                    autochipTextures[i] = new Texture2D(1, 1);
                    byte[] autoTexBytes = Util.Common.FileLoader.LoadSync(autochipImagePath);
                    autochipTextures[i].LoadImage(autoTexBytes);
                    autochipTextures[i].Apply();
                }
            }

            int width = reader.ReadInt(0x26, true, out tmp);
            int height = reader.ReadInt(0x2A, true, out tmp);
            int[,] mapData1 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 0);
            int[,] mapData2 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 1);
            int[,] mapData3 = ReadLayer(reader, width, height, 0x32 + width * height * 4 * 2);

            MapData mapDataX1 = ReadMap(mapData1, mapchipTexture, autochipTextures, tileData);
            MapData mapDataX2 = ReadMap(mapData2, mapchipTexture, autochipTextures, tileData);
            MapData mapDataX3 = ReadMap(mapData3, mapchipTexture, autochipTextures, tileData);

            MapEvent.EventData[] events = ReadMapEvents(reader, mapchipTexture, 0x32 + width * height * 4 * 3);

            return CombineMapData(events, mapDataX1, mapDataX2, mapDataX3);
        }

        private MapData ReadMap(int[,] mapData, Texture2D mapchipTexture, Texture2D[] autochipTextures, MapTile.TileData tileData)
        {
            // 【暫定】読み取る情報はイベントデータを含まずテクスチャとタイル情報のみなのでMapDataではなく別のモデルを返すようにする
            //          各マスの番号をグリッドで返すだけでも良さそう

            int width = mapData.GetLength(1);
            int height = mapData.GetLength(0);
            Texture2D underTexture = new Texture2D(PIXEL_PER_GRID * width, PIXEL_PER_GRID * height, TextureFormat.RGBA32, false);
            Texture2D upperTexture = new Texture2D(PIXEL_PER_GRID * width, PIXEL_PER_GRID * height, TextureFormat.RGBA32, false);
            MovableInfo[,] movableGrid = new MovableInfo[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // オートチップ判定
                    if (mapData[i, j] >= 100000)
                    {
                        int id = mapData[i, j] / 100000;
                        id--;
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[id];
                        movableGrid[i, j] = GetTileInfoFrom(tile);
                        // ID 0はテクスチャ情報無し
                        if (id == 0)
                        {
                            continue;
                        }

                        Texture2D targetTexture = tile.MovableTypeValue == MapTile.MovableType.AlwaysUpper ? upperTexture : underTexture;

                        int leftUp = mapData[i, j] / 1000 % 10;

                        Color[] c = autochipTextures[id].GetPixels(0,
                            autochipTextures[id].height - leftUp * PIXEL_PER_GRID - PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2);
                        targetTexture.SetPixels(PIXEL_PER_GRID * j, targetTexture.height - PIXEL_PER_GRID * (i + 1) + PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, c);

                        int rightUp = mapData[i, j] / 100 % 10;

                        c = autochipTextures[id].GetPixels(PIXEL_PER_GRID / 2,
                            autochipTextures[id].height - rightUp * PIXEL_PER_GRID - PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2);
                        targetTexture.SetPixels(PIXEL_PER_GRID * j + PIXEL_PER_GRID / 2,
                            targetTexture.height - PIXEL_PER_GRID * (i + 1) + PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, c);

                        int leftDown = mapData[i, j] / 10 % 10;

                        c = autochipTextures[id].GetPixels(0,
                            autochipTextures[id].height - leftDown * PIXEL_PER_GRID - PIXEL_PER_GRID, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2);
                        targetTexture.SetPixels(PIXEL_PER_GRID * j,
                            targetTexture.height - PIXEL_PER_GRID * (i + 1), PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, c);

                        int rightDown = mapData[i, j] / 1 % 10;

                        c = autochipTextures[id].GetPixels(PIXEL_PER_GRID / 2,
                            autochipTextures[id].height - rightDown * PIXEL_PER_GRID - PIXEL_PER_GRID, PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2);
                        targetTexture.SetPixels(PIXEL_PER_GRID * j + PIXEL_PER_GRID / 2,
                            targetTexture.height - PIXEL_PER_GRID * (i + 1), PIXEL_PER_GRID / 2, PIXEL_PER_GRID / 2, c);
                    }
                    else
                    {
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[mapData[i, j] + 16];
                        movableGrid[i, j] = GetTileInfoFrom(tile);

                        Texture2D targetTexture = tile.MovableTypeValue == MapTile.MovableType.AlwaysUpper ? upperTexture : underTexture;

                        Color[] c = mapchipTexture.GetPixels(PIXEL_PER_GRID * (mapData[i, j] % 8),
                            mapchipTexture.height - PIXEL_PER_GRID * (1 + mapData[i, j] / 8), PIXEL_PER_GRID, PIXEL_PER_GRID);
                        targetTexture.SetPixels(PIXEL_PER_GRID * j, targetTexture.height - PIXEL_PER_GRID * (i + 1), PIXEL_PER_GRID, PIXEL_PER_GRID, c);
                    }
                }
            }
            upperTexture.Apply();
            underTexture.Apply();

            return new MapData(mapId,underTexture, upperTexture, width, height, movableGrid, null);
        }

        private MapData CombineMapData(MapEvent.EventData[] eventDataArray, params MapData[] mapDataArray)
        {
            if (mapDataArray.Length == 0)
            {
                return null;
            }

            // サイズの違いをフィルタ
            int width = mapDataArray[0].UnderTexture.width;
            int height = mapDataArray[0].UnderTexture.height;
            for (int i = 1; i < mapDataArray.Length; i++)
            {
                if (width != mapDataArray[i].UnderTexture.width || height != mapDataArray[i].UnderTexture.height)
                {
                    return null;
                }
            }

            Texture2D underTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Texture2D upperTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < mapDataArray.Length; k++)
                    {
                        Color c = mapDataArray[k].UnderTexture.GetPixel(j, i);
                        if (c.a > 0.9f)
                        {
                            underTexture.SetPixel(j, i, c);
                        }

                        c = mapDataArray[k].UpperTexture.GetPixel(j, i);
                        if (c.a > 0.9f)
                        {
                            upperTexture.SetPixel(j, i, c);
                        }
                    }
                }
            }
            underTexture.Apply();
            upperTexture.Apply();

            MovableInfo[,] movableGrid = new MovableInfo[mapDataArray[0].Height, mapDataArray[0].Width];
            for (int i = 0; i < mapDataArray[0].Height; i++)
            {
                for (int j = 0; j < mapDataArray[0].Width; j++)
                {
                    bool movable = true;
                    for (int k = 0; k < mapDataArray.Length; k++)
                    {
                        movable &= mapDataArray[k].MovableGrid[i, j].IsMovable;
                    }
                    movableGrid[i, j] = new MovableInfo(movable);
                }
            }

            MapData data = new MapData(mapId, underTexture, upperTexture,
            mapDataArray[0].Width, mapDataArray[0].Height,
             movableGrid, eventDataArray);
            return data;
        }

        private int[,] ReadLayer(Util.Wolf.WolfDataReader reader, int width, int height, int offset)
        {
            int[,] mapData = new int[height, width];
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    int val = reader.ReadInt(offset, true, out offset);
                    mapData[i, j] = val;
                }
            }

            return mapData;
        }

        private MovableInfo GetTileInfoFrom(MapTile.UnitTile unitTile)
        {
            bool isMovable = unitTile.MovableTypeValue != MapTile.MovableType.Immovable;

            return new MovableInfo(isMovable);
        }

        private MapEvent.EventData[] ReadMapEvents(Util.Wolf.WolfDataReader reader, Texture2D mapTexture, int offset)
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
            int triggerFlagType = reader.ReadByte(offset, out offset);
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

            ReadEventMoveRoute(reader, offset, out offset);

            int eventCommandCount = reader.ReadInt(offset, true, out offset);
            Debug.Log($"イベントコマンド数：{eventCommandCount}");
            // デバッグここまでOK
            MapEvent.EventCommandBase[] commands = ReadEventCommands(reader, eventId, eventCommandCount, offset, out offset);

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

            return new MapEvent.EventPageData(texture, direction, haveDirection, commands);
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

        // 【暫定】モデル定義までデータを空読み
        private void ReadEventMoveRoute(Util.Wolf.WolfDataReader reader, int offset, out int nextOffset)
        {
            int animationSpeed = reader.ReadByte(offset, out offset);
            int moveSpeed = reader.ReadByte(offset, out offset);
            int moveFrequency = reader.ReadByte(offset, out offset);
            int moveType = reader.ReadByte(offset, out offset);
            int optionType = reader.ReadByte(offset, out offset);
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
        }

        // 【暫定】詳細定義していないコマンドは空読み
        private MapEvent.EventCommandBase[] ReadEventCommands(Util.Wolf.WolfDataReader reader,
            MapEvent.EventId eventId, int eventCommandCount, int offset, out int nextOffset)
        {
            int currentOffset = offset;
            MapEvent.WolfEventCommandFactory factory = new MapEvent.WolfEventCommandFactory(reader, mapId, eventId, currentOffset);
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