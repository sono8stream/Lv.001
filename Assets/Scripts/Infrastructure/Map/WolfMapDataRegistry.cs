using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Infrastructure.Map
{
    public class WolfMapDataRegistry : IMapDataRegistry
    {
        private List<MapData> mapDataList;

        public WolfMapDataRegistry(string dataPath)
        {
            // 暫定：mpsをシステム変数DBから読み込めるようになるまでフォルダ全体のmpsを拾ってくる
            string mapFilePath = "";
            Debug.Log(mapFilePath);
            byte[] bytes;
            using (var fs = new System.IO.FileStream(mapFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
            }
            Debug.Log(mapFilePath);

            int tileSetId = LoadInt(bytes, 0x22, true);
            MapTile.Repository repository = new MapTile.Repository();
            MapTile.Data tileData = repository.Find(tileSetId);
            Debug.Log(tileData.SettingName);

            {
                int invalidId = tileData.BaseTileFilePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars());
                Debug.Log(tileData.BaseTileFilePath);
                Debug.Log(tileData.BaseTileFilePath[invalidId]);

                string imagePath = "Assets/Resources/Data/" + tileData.BaseTileFilePath.Replace("/", "\\");
                imagePath = "Assets/Resources/Data/MapChip/[Base]BaseChip_pipo.png";
                Debug.Log(imagePath);
                Debug.Log(tileData.BaseTileFilePath);
                Debug.Log(imagePath.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()));
                mapchipTexture = new Texture2D(1, 1);
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


            int width = LoadInt(bytes, 0x26, true);
            int height = LoadInt(bytes, 0x2A, true);
            int[,] mapData1 = LoadLayer(bytes, width, height, 0x32 + width * height * 4 * 0);
            int[,] mapData2 = LoadLayer(bytes, width, height, 0x32 + width * height * 4 * 1);
            int[,] mapData3 = LoadLayer(bytes, width, height, 0x32 + width * height * 4 * 2);

            Utils.WolfMapReader reader = new Utils.WolfMapReader();
            Texture2D layer1Texture = reader.ReadMap(mapData1, mapchipTexture, autochipTextures);
            Texture2D layer2Texture = reader.ReadMap(mapData2, mapchipTexture, autochipTextures);
            Texture2D layer3Texture = reader.ReadMap(mapData3, mapchipTexture, autochipTextures);
            Texture2D mapTexture = reader.CombineTexture(layer1Texture, layer2Texture, layer3Texture);

            //mapInfo = new MapInfo(width, height, mapTexture);
            mapInfo = new MapInfo(width, height, mapTexture);
        }

        public MapData Find(int index)
        {
            if (index >= 0 && index < mapDataList.Count)
            {
                return mapDataList[index];
            }
            else
            {
                return null;
            }
        }
    }
}
