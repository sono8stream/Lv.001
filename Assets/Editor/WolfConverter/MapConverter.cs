using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace WolfConverter
{
    public class MapConverter : EditorWindow
    {
        private UnityEngine.Object folder;
        private int mapchipIndex;

        private int mapDataIndex;

        private UnityEngine.Object imgDirectory;

        private UnityEngine.Object dataDirectory;

        private Texture2D mapchipTexture = null;

        private const int autoTileCount = 16;
        private Texture2D[] autochipTextures = new Texture2D[autoTileCount];

        class MapInfo
        {
            public int width;
            public int height;
            public Texture2D mapTexture;

            public MapInfo(int width, int height, Texture2D mapTexture)
            {
                this.width = width;
                this.height = height;
                this.mapTexture = mapTexture;
            }
        }

        private MapInfo mapInfo;

        [UnityEditor.MenuItem("Window/WolfConverter/MapConverter")]
        static void ShowMapConverter()
        {
            EditorWindow.GetWindow(typeof(MapConverter));
        }

        private void OnGUI()
        {
            // GUI
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mapchip Directory : ", GUILayout.Width(110));
            imgDirectory = EditorGUILayout.ObjectField(imgDirectory, typeof(UnityEngine.Object), true);
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            GUILayout.Label("MapData Directory : ", GUILayout.Width(110));
            dataDirectory = EditorGUILayout.ObjectField(dataDirectory, typeof(UnityEngine.Object), true);
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();

            ShowMapChipPullDown();

            EditorGUILayout.Space();

            ShowMapFilePullDown();

            EditorGUILayout.BeginHorizontal();

            DrawMapChip();

            ShowBinaryData();

            EditorGUILayout.EndHorizontal();
        }

        private void ShowMapChipPullDown()
        {
            if (imgDirectory == null)
            {
                return;
            }

            // プルダウンメニューに登録する文字列配列
            string path = AssetDatabase.GetAssetPath(imgDirectory);
            string[] paths = System.IO.Directory.GetFiles(path, "*.png");
            string[] displayOptions = paths.Select(a => a.Replace($"{path}\\", "")).ToArray();

            // プルダウンメニューの作成
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapChip", mapchipIndex, displayOptions)
                : -1;

            if (EditorGUI.EndChangeCheck())
            {
                if (mapchipIndex != curIndex)
                {
                    mapchipIndex = curIndex;
                    //string imagePath = $"file://{Application.streamingAssetsPath}/MapChip/{displayOptions[index]}";
                    LoadTexture(paths[mapchipIndex]);
                }
            }
        }

        private void ShowMapFilePullDown()
        {
            if (dataDirectory == null)
            {
                return;
            }

            // プルダウンメニューに登録する文字列配列
            string path = AssetDatabase.GetAssetPath(dataDirectory);
            string[] filePaths = System.IO.Directory.GetFiles(path, "*.mps");
            string[] displayOptions = filePaths.Select(a => a.Replace($"{path}\\", "")).ToArray();

            // プルダウンメニューの作成
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapData", mapDataIndex, displayOptions)
                : -1;

            // チェック終了時
            if (EditorGUI.EndChangeCheck())
            {
                if (mapDataIndex != curIndex)
                {
                    mapDataIndex = curIndex;
                    ReadBinary(filePaths[mapDataIndex]);
                }
            }
        }

        private string[] GetStreamingAssetFiles(string dir, string pattern, bool isFullPath = true)
        {
            // 最初と末尾に/追加
            if (dir.IndexOf("/") != 0)
            {
                dir = "/" + dir;
            }
            if (dir.LastIndexOf("/") != dir.Length - 1)
            {
                dir = dir + "/";
            }

            string basePath = Application.streamingAssetsPath + dir;
            string[] files = System.IO.Directory.GetFiles(basePath, pattern);

            if (isFullPath && -1 == files[0].IndexOf(basePath))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = basePath + files[i];
                }
            }
            else if (!isFullPath && -1 < files[0].IndexOf(basePath))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    files[i] = files[i].Replace(basePath, "");
                }
            }

            return files;
        }

        private void LoadTexture(string imageName)
        {
            byte[] texBytes;
            using (var fs = new System.IO.FileStream(imageName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                texBytes = new byte[fs.Length];
                fs.Read(texBytes, 0, texBytes.Length);
            }
            Texture2D texture2D = new Texture2D(1, 1);
            texture2D.LoadImage(texBytes);
            Debug.Log(texture2D.height);
            texture2D.Apply();
            Rect rect = GUILayoutUtility.GetLastRect();
            EditorGUI.DrawPreviewTexture(new Rect(rect.x, rect.y + rect.height + 10, texture2D.width, texture2D.height), texture2D);

            string path = AssetDatabase.GetAssetPath(imgDirectory);
            string[] names = System.IO.Directory.GetFiles(path, "*.png");
            Debug.Log(names[names.Length - 2]);
            mapchipTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(names[names.Length - 2], typeof(Texture2D));

            string[] autochipPaths = new string[autoTileCount]{
                "",
                "Data/MapChip/[A]Grass1-Dirt1_pipo",
                "Data/MapChip/[A]Grass1-Grass2_pipo",
                "Data/MapChip/[A]Grass1-Grass3_pipo",
                "Data/MapChip/[A]Water2_pipo",
                "Data/MapChip/[A]Water5_pipo",
                "Data/MapChip/[A]Water7_pipo",
                "Data/MapChip/[A]WaterFall2_pipo",
                "Data/MapChip/[A]Flower_pipo",
                "Data/MapChip/[A]Snow_pipo",
                "Data/MapChip/[A]Snow_Grass4_pipo",
                "Data/MapChip/[A]Snow_Dirt2_pipo",
                "Data/MapChip/[A]Ice2_pipo",
                "Data/MapChip/[A]Dirt1-Dirt2_pipo",
                "Data/MapChip/[A]Wall-Up1_pipo",
                "Data/MapChip/[A]Wall-Up2_pipo"
            };

            for (int i = 1; i < autoTileCount; i++)
            {
                autochipTextures[i] = Resources.Load(autochipPaths[i]) as Texture2D;
            }

            /*
            string imagePath = $"{Application.streamingAssetsPath}/MapChip";
            //string path = AssetDatabase.GetAssetPath(imagePath);
            //Debug.Log(path);
            mapchipTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageName, typeof(Texture2D));
            Debug.Log(mapchipTexture == null);
            /*
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
            {
                www.SendWebRequest();
                while (!www.isDone && !www.isNetworkError && !www.isHttpError) { }
                Debug.Log(www.downloadHandler.text);
                if (!www.isNetworkError && !www.isHttpError)
                {
                    Debug.Log("Downloaded");
                    mapchipTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                }
            };
            */
        }

        // 画像一覧をボタン選択出来る形にして出力
        private void DrawMapChip()
        {
            if (imgDirectory != null && mapchipTexture != null)
            {
                float maxW = 100.0f;
                float maxH = 500.0f;

                string path = AssetDatabase.GetAssetPath(imgDirectory);
                string[] names = System.IO.Directory.GetFiles(path, "*.png");
                GUILayout.Button(mapchipTexture, GUILayout.MaxWidth(maxW), GUILayout.MaxHeight(maxH),
                GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            }
        }

        private void ReadBinary(string mapFilePath)
        {
            byte[] bytes;
            using (var fs = new System.IO.FileStream(mapFilePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                bytes = new byte[fs.Length];
                fs.Read(bytes, 0, bytes.Length);
            }
            Debug.Log(mapFilePath);

            int tileSetId = LoadInt(bytes, 0x22, true);
            MapTile.Loader loader = new MapTile.Loader();
            MapTile.Data[] tileData = loader.LoadAllMapTilesFromDataBinary();
            for (int i = 0; i < tileData.Length; i++)
            {
                Debug.Log(tileData[i].SettingName);
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

            mapInfo = new MapInfo(width, height, mapTexture);
        }

        private void ShowBinaryData()
        {
            if (mapInfo == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label($"Width: {mapInfo.width.ToString()}", GUILayout.Width(110));
            GUILayout.Label($"Height: {mapInfo.height.ToString()}", GUILayout.Width(110));

            EditorGUILayout.EndHorizontal();

            GUILayout.Button(mapInfo.mapTexture, GUILayout.MaxWidth(mapInfo.mapTexture.width),
             GUILayout.MaxHeight(mapInfo.mapTexture.height), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            EditorGUILayout.EndVertical();
        }

        private int LoadInt(byte[] bytes, int offset, bool isLittleEndian)
        {
            if (offset < 0 || offset + 4 > bytes.Length)
            {
                return 0;
            }

            int res = 0;
            if (isLittleEndian)
            {
                for (int i = 4 - 1; i >= 0; i--)
                {
                    res *= 256;
                    res += bytes[offset + i];
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    res *= 256;
                    res += bytes[offset + i];
                }
            }

            return res;
        }

        private int[,] LoadLayer(byte[] bytes, int width, int height, int offset)
        {
            int[,] mapData = new int[height, width];
            for (int j = 0; j < width; j++)
            {
                for (int i = 0; i < height; i++)
                {
                    int val = LoadInt(bytes, offset, true);
                    mapData[i, j] = val;
                    offset += 4;
                }
            }

            return mapData;
        }

        private string LoadString(byte[] bytes, int offset)
        {
            int strLength = LoadInt(bytes, offset, true);
            byte[] strBytes = new byte[strLength];
            Array.Copy(bytes, offset + 4, strBytes, 0, strLength);
            return System.Text.Encoding.GetEncoding("shift_jis").GetString(strBytes);
        }
    }
}
