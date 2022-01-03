using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class WolfMapCreator
    {
        public MapData Create(string mapFilePath)
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
                string imagePath = "Assets/Resources/Data/" + tileData.BaseTileFilePath.Replace("/", "\\");
                imagePath = "Assets/Resources/Data/MapChip/[Base]BaseChip_pipo.png";
                using (var fs = new System.IO.FileStream(imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    byte[] texBytes = new byte[fs.Length];
                    fs.Read(texBytes, 0, texBytes.Length);
                    mapchipTexture.LoadImage(texBytes);
                    mapchipTexture.Apply();
                }

                for (int i = 1; i < autoTileCount; i++)
                {
                    string autochipImagePath = "Assets/Resources/Data/" + tileData.AutoTileFilePaths[i - 1];
                    autochipTextures[i] = new Texture2D(1, 1);
                    using (var fs = new System.IO.FileStream(autochipImagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        byte[] texBytes = new byte[fs.Length];
                        fs.Read(texBytes, 0, texBytes.Length);
                        autochipTextures[i].LoadImage(texBytes);
                        autochipTextures[i].Apply();
                    }
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

            MapEvent.EventData[] events = ReadMapEvents(reader, 0x32 + width * height * 4 * 3);

            return CombineMapData(mapDataX1, mapDataX2, mapDataX3);
        }

        private MapData ReadMap(int[,] mapData, Texture2D mapchipTexture, Texture2D[] autochipTextures, MapTile.TileData tileData)
        {
            // 【暫定】マップチップのピクセル数は16で固定とする　
            int masu = 16;
            int width = mapData.GetLength(1);
            int height = mapData.GetLength(0);
            Texture2D underTexture = new Texture2D(masu * width, masu * height, TextureFormat.RGBA32, false);//マップ初期化
            Texture2D upperTexture = new Texture2D(masu * width, masu * height, TextureFormat.RGBA32, false);//マップ初期化
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
                            autochipTextures[id].height - leftUp * masu - masu / 2, masu / 2, masu / 2);
                        targetTexture.SetPixels(masu * j, targetTexture.height - masu * (i + 1) + masu / 2, masu / 2, masu / 2, c);

                        int rightUp = mapData[i, j] / 100 % 10;

                        c = autochipTextures[id].GetPixels(masu / 2,
                            autochipTextures[id].height - rightUp * masu - masu / 2, masu / 2, masu / 2);
                        targetTexture.SetPixels(masu * j + masu / 2,
                            targetTexture.height - masu * (i + 1) + masu / 2, masu / 2, masu / 2, c);

                        int leftDown = mapData[i, j] / 10 % 10;

                        c = autochipTextures[id].GetPixels(0,
                            autochipTextures[id].height - leftDown * masu - masu, masu / 2, masu / 2);
                        targetTexture.SetPixels(masu * j,
                            targetTexture.height - masu * (i + 1), masu / 2, masu / 2, c);

                        int rightDown = mapData[i, j] / 1 % 10;

                        c = autochipTextures[id].GetPixels(masu / 2,
                            autochipTextures[id].height - rightDown * masu - masu, masu / 2, masu / 2);
                        targetTexture.SetPixels(masu * j + masu / 2,
                            targetTexture.height - masu * (i + 1), masu / 2, masu / 2, c);
                    }
                    else
                    {
                        MapTile.UnitTile tile = tileData.UnitTileConfigs[mapData[i, j] + 16];
                        movableGrid[i, j] = GetTileInfoFrom(tile);

                        Texture2D targetTexture = tile.MovableTypeValue == MapTile.MovableType.AlwaysUpper ? upperTexture : underTexture;

                        Color[] c = mapchipTexture.GetPixels(masu * (mapData[i, j] % 8),
                            mapchipTexture.height - masu * (1 + mapData[i, j] / 8), masu, masu);
                        targetTexture.SetPixels(masu * j, targetTexture.height - masu * (i + 1), masu, masu, c);
                    }
                }
            }
            upperTexture.Apply();
            underTexture.Apply();

            return new MapData(underTexture, upperTexture, width, height, movableGrid);
        }

        private MapData CombineMapData(params MapData[] mapDataArray)
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

            MapData data = new MapData(underTexture, upperTexture, mapDataArray[0].Width, mapDataArray[0].Height, movableGrid);
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

        private MapEvent.EventData[] ReadMapEvents(Util.Wolf.WolfDataReader reader, int offset)
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
                int eventId = reader.ReadInt(offset, true, out offset);
                Debug.Log(eventId);
                string eventName = reader.ReadString(offset, out offset);
                int posX = reader.ReadInt(offset, true, out offset);
                int posY = reader.ReadInt(offset, true, out offset);
                int pageCount = reader.ReadInt(offset, true, out offset);
                Debug.Log(pageCount);
                // 00 00 00 00のスキップ
                reader.ReadInt(offset, true, out offset);

                List<MapEvent.EventPageData> eventPages = new List<MapEvent.EventPageData>();
                for (int i = 0; i < pageCount; i++)
                {
                    // イベントページの読み込み

                    // ヘッダースキップ
                    int hh = reader.ReadByte(offset, out offset);
                    int tileNo = reader.ReadInt(offset, true, out offset);
                    string chipImgName = reader.ReadString(offset, out offset);
                    Debug.Log(chipImgName);
                    int direction = reader.ReadByte(offset, out offset);
                    int animNo = reader.ReadByte(offset, out offset);
                    int charaAlpha = reader.ReadByte(offset, out offset);
                    int showType = reader.ReadByte(offset, out offset);
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
                    Debug.Log(eventCommandCount);
                    // デバッグここまでOK
                    ReadEventCommands(reader, eventCommandCount, offset, out offset);

                    // イベントコマンドフッタースキップ　暫定でイベントコマンド読み取りに含める
                    //reader.ReadInt(offset, true, out offset);

                    int shadowNo = reader.ReadByte(offset, out offset);
                    Debug.Log(shadowNo);// ここから不適
                    int rangeExtendX = reader.ReadByte(offset, out offset);
                    int rangeExtendY = reader.ReadByte(offset, out offset);

                    // フッタースキップ
                    int ff = reader.ReadByte(offset, out offset);
                    Debug.Log(ff);
                }

                // フッタースキップ
                reader.ReadByte(offset, out offset);

                // 次の計算用にヘッダを更新
                int nextHeaderByte = reader.ReadByte(offset, out offset);
                headerByte = nextHeaderByte;
            }
            Debug.Log(list.Count);

            return list.ToArray();
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

            // 動作コマンド
            for (int i = 0; i < commandCount; i++)
            {
                int commandType = reader.ReadByte(offset, out offset);
                int variableCount = reader.ReadByte(offset, out offset);
                int variableValue = reader.ReadByte(offset, out offset);

                // 終端
                int footer1 = reader.ReadByte(offset, out offset);
                int footer2 = reader.ReadByte(offset, out offset);
            }

            nextOffset = offset;
        }

        // 【暫定】詳細定義まで空読み
        private void ReadEventCommands(Util.Wolf.WolfDataReader reader, int eventCommandCount, int offset, out int nextOffset)
        {
            int currentOffset = offset;
            MapEvent.WolfMapEventFactory factory = new MapEvent.WolfMapEventFactory(reader, currentOffset);
            for (int i = 0; i < eventCommandCount; i++)
            {
                // 一つ一つのコマンドを読み取る
                factory.Create2(out currentOffset);
            }
            nextOffset = currentOffset;
            // factory.Create(out nextOffset);
        }
    }

}