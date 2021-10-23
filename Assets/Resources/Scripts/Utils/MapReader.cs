using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class MapReader
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
                    if (mapData[i, j] >= 100000)
                    {
                        int id = mapData[i, j] / 100000;
                        id--;
                        if (id > 0)
                        {
                            Color[] c = autochipTextures[id].GetPixels(0, 0, masu, masu);
                            mapTexture.SetPixels(masu * j, mapTexture.height - masu * (i + 1), masu, masu, c);
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