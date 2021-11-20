using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Expression.Map;

namespace WolfConverter
{
    public class MapConverter : EditorWindow
    {
        private int mapDataIndex = -1;
        private Expression.DependencyInjector injector
        = new Expression.DependencyInjector(new WolfMapDataRepository());

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

            ShowMapFilePullDown();

            EditorGUILayout.BeginHorizontal();

            ShowBinaryData();

            EditorGUILayout.EndHorizontal();
        }

        private void ShowMapFilePullDown()
        {
            // プルダウンメニューに登録する文字列配列
            string path = "Assets/Resources/Data/MapData";
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

                    MapData data = injector.MapDataRepository.Find(id);
                    mapInfo = new MapInfo(data.Width, data.Height, data.UnderTexture);
                }
            }
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
    }
}
