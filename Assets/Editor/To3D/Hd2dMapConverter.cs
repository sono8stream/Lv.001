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
        private Material mat = null;
        private int mapDataIndex = -1;
        private const int CHIP_COUNT = 256;// 仮で決め打ち
        private Vector3[] offsets = new Vector3[CHIP_COUNT];
        private Vector2 scrollPos = Vector2.zero;

        [MenuItem("Window/Hd2dConverter/WolfConverter")]
        static void ShowMapConverter()
        {
            GetWindow(typeof(Hd2dMapConverter));
        }

        private void OnGUI()
        {
            // GUI
            ShowMapFilePullDown();

            mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;

                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    offsets[i] = EditorGUILayout.Vector3Field("Sample", offsets[i]);
                }
            }
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
                    mapDataIndex = curIndex;
                    MapId id = new MapId(mapDataIndex);
                    Hd2dTileInfo[] tileInfoArray = offsets.Select(offset => new Hd2dTileInfo(offset)).ToArray();
                    WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id, tileInfoArray);
                    creator.Create(filePaths[curIndex]);
                }
            }
        }
    }
#endif
}
