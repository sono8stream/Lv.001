using UnityEngine;

namespace Infrastructure.Map.Util
{
    public class WolfMapReader
    {
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
    }

}