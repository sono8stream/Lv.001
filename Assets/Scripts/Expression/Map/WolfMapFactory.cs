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

            Debug.Log(mapFilePath);
            Util.Wolf.WolfDataReader reader = new Util.Wolf.WolfDataReader(mapFilePath);

            int tileSetId = reader.ReadInt(0x22, true, out int tmp);
            MapTile.WolfRepository repository = new MapTile.WolfRepository();
            MapTile.TileData tileData = repository.Find(tileSetId);
            Debug.Log(tileData.SettingName);

            Texture2D mapchipTexture = new Texture2D(1, 1);
            int autoTileCount = 16;
            Texture2D[] autochipTextures = new Texture2D[autoTileCount];
            {
                int invalidId = tileData.BaseTileFilePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars());
                Debug.Log(tileData.BaseTileFilePath);
                Debug.Log(tileData.BaseTileFilePath[invalidId]);

                string imagePath = "Assets/Resources/Data/" + tileData.BaseTileFilePath.Replace("/", "\\");
                imagePath = "Assets/Resources/Data/MapChip/[Base]BaseChip_pipo.png";
                Debug.Log(imagePath);
                Debug.Log(tileData.BaseTileFilePath);
                Debug.Log(imagePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()));
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
                    Debug.Log(autochipImagePath);
                    invalidId = autochipImagePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars());
                    Debug.Log(invalidId);
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

            Util.Wolf.WolfMapReader mapReader = new Util.Wolf.WolfMapReader();
            Texture2D layer1Texture = ReadMap(mapData1, mapchipTexture, autochipTextures);
            Texture2D layer2Texture = ReadMap(mapData2, mapchipTexture, autochipTextures);
            Texture2D layer3Texture = ReadMap(mapData3, mapchipTexture, autochipTextures);
            Texture2D underMapTexture = CombineTexture(layer1Texture, layer2Texture, layer3Texture);

            Texture2D upperMapTexture = new Texture2D(underMapTexture.width, underMapTexture.height);

            int[,] movableGrid = new int[height, width];

            MapData mapData = new MapData(underMapTexture, upperMapTexture, width, height, movableGrid);

            return mapData;
        }

        public Texture2D ReadMap(int[,] mapData, Texture2D mapchipTexture, Texture2D[] autochipTextures)
        {
            int masu = 16;
            int width = mapData.GetLength(1);
            int height = mapData.GetLength(0);
            Texture2D mapTexture = new Texture2D(masu * width, masu * height, TextureFormat.RGBA32, false);//マップ初期化
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    // オートチップ判定
                    if (mapData[i, j] >= 100000)
                    {
                        int id = mapData[i, j] / 100000;
                        id--;
                        if (id > 0)
                        {
                            int leftUp = mapData[i, j] / 1000 % 10;

                            Color[] c = autochipTextures[id].GetPixels(0,
                                autochipTextures[id].height - leftUp * masu - masu / 2, masu / 2, masu / 2);
                            mapTexture.SetPixels(masu * j, mapTexture.height - masu * (i + 1) + masu / 2, masu / 2, masu / 2, c);

                            int rightUp = mapData[i, j] / 100 % 10;

                            c = autochipTextures[id].GetPixels(masu / 2,
                                autochipTextures[id].height - rightUp * masu - masu / 2, masu / 2, masu / 2);
                            mapTexture.SetPixels(masu * j + masu / 2,
                                mapTexture.height - masu * (i + 1) + masu / 2, masu / 2, masu / 2, c);

                            int leftDown = mapData[i, j] / 10 % 10;

                            c = autochipTextures[id].GetPixels(0,
                                autochipTextures[id].height - leftDown * masu - masu, masu / 2, masu / 2);
                            mapTexture.SetPixels(masu * j,
                                mapTexture.height - masu * (i + 1), masu / 2, masu / 2, c);

                            int rightDown = mapData[i, j] / 1 % 10;

                            c = autochipTextures[id].GetPixels(masu / 2,
                                autochipTextures[id].height - rightDown * masu - masu, masu / 2, masu / 2);
                            mapTexture.SetPixels(masu * j + masu / 2,
                                mapTexture.height - masu * (i + 1), masu / 2, masu / 2, c);
                        }
                    }
                    else
                    {
                        Color[] c = mapchipTexture.GetPixels(masu * (mapData[i, j] % 8),
                            mapchipTexture.height - masu * (1 + mapData[i, j] / 8), masu, masu);
                        mapTexture.SetPixels(masu * j, mapTexture.height - masu * (i + 1), masu, masu, c);
                    }
                }
            }
            mapTexture.Apply();
            return mapTexture;
        }

        public Texture2D CombineTexture(params Texture2D[] textures)
        {
            if (textures.Length == 0)
            {
                return null;
            }

            int width = textures[0].width;
            int height = textures[0].height;
            for (int i = 1; i < textures.Length; i++)
            {
                if (width != textures[i].width || height != textures[i].height)
                {
                    return null;
                }
            }

            Texture2D resTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);//マップ初期化
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    for (int k = 0; k < textures.Length; k++)
                    {
                        Color c = textures[k].GetPixel(j, i);

                        if (c.a < 0.9f)
                        {
                            continue;
                        }


                        //Debug.Log(c);
                        resTexture.SetPixel(j, i, c);
                    }

                }
            }
            resTexture.Apply();
            Debug.Log("Done combining");
            return resTexture;
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
    }

}