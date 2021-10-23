using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.Linq;

namespace WolfConverter
{
    public class MapConverter : EditorWindow
    {
        private Object folder;
        private int index;

        private Object imgDirectory;

        private Object dataDirectory;

        private Texture2D mapchipTexture = null;

        private Texture2D mapTexture = null;

        private TextAsset mapBinData = null;

        private byte[] dataBytes = null;

        private const int autoTileCount = 16;
        private Texture2D[] autochipTextures = new Texture2D[autoTileCount];



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

            //ShowMapChipImage();

            EditorGUILayout.BeginHorizontal();

            DrawImagePart();

            DrawBinary();

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
            string[] displayOptions = System.IO.Directory.GetFiles(path, "*.png");

            // プルダウンメニューの作成
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapChip", index, displayOptions)
                : -1;

            if (EditorGUI.EndChangeCheck())
            {
                if (index != curIndex)
                {
                    index = curIndex;
                    //string imagePath = $"file://{Application.streamingAssetsPath}/MapChip/{displayOptions[index]}";
                    LoadTexture(displayOptions[index]);
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
            string[] displayOptions = System.IO.Directory.GetFiles(path, "*.mps");
            displayOptions = displayOptions.Select(a => a.Replace($"{path}\\", "")).ToArray();

            // プルダウンメニューの作成
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapData", index, displayOptions)
                : -1;

            // チェック終了時
            if (EditorGUI.EndChangeCheck())
            {
                if (index != curIndex)
                {
                    index = curIndex;
                    string[] names = System.IO.Directory.GetFiles(path, "*.mps");
                    Debug.Log(names[0]);
                    mapBinData = Resources.Load("Data/MapData/SampleMapA") as TextAsset;
                    using (var reader = new System.IO.BinaryReader(new System.IO.FileStream(names[index], System.IO.FileMode.Open)))
                    {
                        dataBytes = reader.ReadBytes(int.MaxValue);
                        Debug.Log(dataBytes.Length);
                    }
                    ReadBinary();
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
            string path = AssetDatabase.GetAssetPath(imgDirectory);
            string[] names = System.IO.Directory.GetFiles(path, "*.png");
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

        private void ShowMapChipImage()
        {
            if (mapchipTexture == null)
            {
                return;
            }

            //画像のサイズ
            var textureWidth = (float)mapchipTexture.width;
            var textureHeight = (float)mapchipTexture.height;

            // ウィンドウサイズより大きければ縮小
            if (this.position.width < textureWidth
                    || this.position.height < textureHeight)
            {
                //縮小率を計算(縦横でより小さい方)
                float shrinkRate = Mathf.Min(
                    this.position.width / textureWidth, this.position.height / textureHeight);

                textureWidth *= shrinkRate;
                textureHeight *= shrinkRate;
            }

            //Windowの中央に表示するための位置決定
            var posX = (this.position.width - textureWidth) / 2;
            var posY = (this.position.height - textureHeight) / 2;

            //画像の表示
            EditorGUI.DrawPreviewTexture(new Rect(posX, posY, textureWidth, textureHeight), mapchipTexture);

        }

        // 画像一覧をボタン選択出来る形にして出力
        private void DrawImagePart()
        {
            if (imgDirectory != null && mapchipTexture != null)
            {
                float x = 0.0f;
                float y = 0.0f;
                float w = 50.0f;
                float h = 50.0f;
                float maxW = 300.0f;
                float maxH = 300.0f;

                string path = AssetDatabase.GetAssetPath(imgDirectory);
                string[] names = System.IO.Directory.GetFiles(path, "*.png");
                GUILayout.Button(mapchipTexture, GUILayout.MaxWidth(w), GUILayout.MaxHeight(maxH), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

            }
        }

        private void ReadBinary()
        {
            if (dataBytes == null)
            {
                return;
            }

            byte[] bytes = dataBytes;
            int width = LoadInt(bytes, 0x26, true);
            int height = LoadInt(bytes, 0x2A, true);
            int[,] mapData1 = LoadLayer(bytes, width, height, 0x32);
            int[,] mapData2 = LoadLayer(bytes, width, height, 0x32 + width * height * 4);
            int[,] mapData3 = LoadLayer(bytes, width, height, 0x32 + width * height * 4 * 2);

            Utils.MapReader reader = new Utils.MapReader();
            Texture2D layer1Texture = reader.ReadMap(mapData1, mapchipTexture, autochipTextures);
            Texture2D layer2Texture = reader.ReadMap(mapData2, mapchipTexture, autochipTextures);
            Texture2D layer3Texture = reader.ReadMap(mapData3, mapchipTexture, autochipTextures);
            mapTexture = reader.CombineTexture(layer1Texture, layer2Texture, layer3Texture);
        }

        private void DrawBinary()
        {
            if (mapBinData == null || mapTexture == null)
            {
                return;
            }

            byte[] bytes = mapBinData.bytes;
            int width = LoadInt(bytes, 0x26, true);
            int height = LoadInt(bytes, 0x2A, true);

            int[,] mapData1 = LoadLayer(bytes, width, height, 0x32);

            string data = "";
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    data += $"{mapData1[i, j]},";
                }
                data += "\n";
            }

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label($"Width: {width.ToString()}", GUILayout.Width(110));
            GUILayout.Label($"Height: {height.ToString()}", GUILayout.Width(110));
            GUILayout.Label($"Data size: {bytes.Length.ToString()} bytes");

            EditorGUILayout.EndHorizontal();

            //data = GUILayout.TextArea(data);
            GUILayout.Button(mapTexture, GUILayout.MaxWidth(mapTexture.width), GUILayout.MaxHeight(mapTexture.height), GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));

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
    }

}

