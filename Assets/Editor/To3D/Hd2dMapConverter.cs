using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Expression;
using Expression.Map;

namespace Hd2d
{
    // 【暫定】Assembly Infoを切り分けてifdefを削除
#if UNITY_EDITOR
    public class Hd2dMapConverter : EditorWindow
    {
        private Shader shad = null;
        private int mapDataIndex = -1;
        private const int CHIP_COUNT = 2500;// 仮で決め打ち
        private Hd2dTileInfo[] tileInfoArray = new Hd2dTileInfo[CHIP_COUNT];
        private Vector2 scrollPos = Vector2.zero;

        private Expression.Map.MapTile.WolfRepository repository = new Expression.Map.MapTile.WolfRepository();
        private int tileId;
        private Texture2D baseTex = null;
        private Vector2 imageScrollPos = Vector2.zero;

        private bool isShownTileInfo = false;
        private Vector2Int chipOffset = Vector2Int.zero;
        private int selectedChipIndex = 0;

        [System.Serializable]
        class SaveData
        {
            public Hd2dTileInfo[] tileInfoArray;

            public SaveData()
            {
                tileInfoArray = new Hd2dTileInfo[0];
            }
        }

        private string saveKey = "Hd2dTileSetting";

        [MenuItem("Window/Hd2dConverter/WolfConverter")]
        static void ShowMapConverter()
        {
            GetWindow(typeof(Hd2dMapConverter));
        }

        private void OnEnable()
        {
            if (EditorPrefs.HasKey(saveKey))
            {
                string json = EditorPrefs.GetString(saveKey);
                SaveData loaded = JsonUtility.FromJson<SaveData>(json);
                if (loaded.tileInfoArray.Length == CHIP_COUNT)
                {
                    tileInfoArray = loaded.tileInfoArray;
                    Debug.Log("Loaded tile data");
                }
            }
            else
            {
                tileInfoArray = new Hd2dTileInfo[CHIP_COUNT];
                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    tileInfoArray[i] = new Hd2dTileInfo(Vector3.zero, MapBlockType.Cube);
                }
            }
        }

        private void OnDisable()
        {
            SaveData data = new SaveData();
            data.tileInfoArray = tileInfoArray;
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(saveKey, json);
            Debug.Log("Saved tile data");
        }


        private void OnGUI()
        {
            // GUI
            ShowMapFilePullDown();

            shad = EditorGUILayout.ObjectField("Shader", shad, typeof(Shader), false) as Shader;

            ShowMapTiles();

            isShownTileInfo = EditorGUILayout.BeginFoldoutHeaderGroup(isShownTileInfo, "Tile Info");

            if (isShownTileInfo)
            {
                ShowMapTileEditor();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();


        }

        private void ShowMapFilePullDown()
        {
            // プルダウンメニューに登録する文字列配列
            string path = $"{Application.streamingAssetsPath}/Data/MapData";
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
                    RemoveExistingMap();
                    mapDataIndex = curIndex;
                    MapId id = new MapId(mapDataIndex);
                    WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id, tileInfoArray, shad);
                    creator.Create(filePaths[curIndex]);
                }
            }
        }

        private void ShowMapTiles()
        {
            float rate = 2;
            EditorGUILayout.LabelField("マウスの位置", Event.current.mousePosition.ToString());
            EditorGUILayout.LabelField("画像上の位置", chipOffset.ToString());
            EditorGUILayout.LabelField("スクロールバーの位置", imageScrollPos.ToString());

            //wantsMouseMoveをトグルで切り替えられるように
            wantsMouseMove = EditorGUILayout.Toggle("wantsMouseMove", wantsMouseMove);

            //マウスが動いたら再描画(wantsMouseMoveが有効でないとOnGUI自体が呼ばれないの無意味)
            if (Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
            if (Event.current.type == EventType.MouseDown)
            {
                float xRaw = Event.current.mousePosition.x + imageScrollPos.x;
                float yRaw = Event.current.mousePosition.y + imageScrollPos.y - 140;
                int unitSize = 16;
                int chipsPerWidth = 8;
                int col = (int)(xRaw / unitSize / rate);
                int row = (int)(yRaw / unitSize / rate);
                chipOffset = new Vector2Int(col, row);

                if (0 <= chipOffset.x && chipOffset.x * unitSize < baseTex.width
                    && 0 <= chipOffset.y && chipOffset.y * unitSize < baseTex.height)
                {
                    // 【暫定】オートチップの分を追加しておく
                    selectedChipIndex = chipOffset.x + chipOffset.y * chipsPerWidth + 16;
                }
            }

            int nextTileId = EditorGUILayout.IntField("Tile ID", tileId);
            if (tileId != nextTileId || baseTex == null)
            {
                tileId = nextTileId;
                Expression.Map.MapTile.TileData tileData = repository.Find(tileId);
                string imagePath = $"{Application.streamingAssetsPath}/Data/" + tileData.BaseTileFilePath;
                byte[] baseTexBytes = Util.Common.FileLoader.LoadSync(imagePath);
                baseTex = new Texture2D(0, 0);
                baseTex.LoadImage(baseTexBytes);
                baseTex.filterMode = FilterMode.Point;
                baseTex.Apply();
            }


            EditorGUILayout.BeginHorizontal();
            //画像表示
            using (var scrollView = new EditorGUILayout.ScrollViewScope(imageScrollPos))
            {
                imageScrollPos = scrollView.scrollPosition;
                EditorGUIUtility.SetIconSize(new Vector2(baseTex.width, baseTex.height)*rate);
                GUILayout.Button(baseTex, GUIStyle.none);
                EditorGUIUtility.SetIconSize(Vector2.one);

            }

            EditorGUILayout.BeginVertical();

            tileInfoArray[selectedChipIndex].type
                = (MapBlockType)EditorGUILayout.EnumPopup($"Type {selectedChipIndex}", tileInfoArray[selectedChipIndex].type);
            tileInfoArray[selectedChipIndex].offset
                = EditorGUILayout.Vector3Field($"Tile {selectedChipIndex}", tileInfoArray[selectedChipIndex].offset);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void ShowMapTileEditor()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;

                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    tileInfoArray[i].type
                        = (MapBlockType)EditorGUILayout.EnumPopup($"Type {i}", tileInfoArray[i].type);
                    tileInfoArray[i].offset
                        = EditorGUILayout.Vector3Field($"Tile {i}", tileInfoArray[i].offset);
                }
            }
        }

        private void RemoveExistingMap()
        {
            var obj = GameObject.Find("Hd2dMap");
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }

    }
#endif
}
