using System.Collections.Generic;
using UnityEngine;

namespace Expression.Map
{
    public class WolfMapFactory : WolfBaseMapFactory
    {
        public WolfMapFactory(MapId mapId) : base(mapId)
        {
        }

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

            return new MapData(mapId, underTexture, upperTexture, width, height, movableGrid, null);
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
    }

}