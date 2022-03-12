using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Expression;
using Expression.Map;

namespace WolfConverter
{
    // なぜかEditorフォルダにおいても実機ビルド対象になるのでifdefで切る
    // Assembly Infoを一括管理していたのが原因
#if UNITY_EDITOR
    public class MapConverter : EditorWindow
    {
        private int mapDataIndex = -1;

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

        [MenuItem("Window/WolfConverter/MapConverter")]
        static void ShowMapConverter()
        {
            GetWindow(typeof(MapConverter));
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

                    MapData data = WolfDependencyInjector.It().MapDataRepository.Find(id);
                    Texture2D texture = RenderMapTexture(data);
                    mapInfo = new MapInfo(data.Width, data.Height, texture);
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

        // underTexture, upperTexture, eventTextureを描画
        private Texture2D RenderMapTexture(MapData mapData)
        {
            int textureWidth = mapData.UnderTexture.width;
            int textureHeight = mapData.UnderTexture.height;
            Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
            for (int i = 0; i < textureHeight; i++)
            {
                for (int j = 0; j < textureWidth; j++)
                {
                    Color c = mapData.UnderTexture.GetPixel(j, i);
                    if (c.a > 0.9f)
                    {
                        texture.SetPixel(j, i, c);
                    }

                    c = mapData.UpperTexture.GetPixel(j, i);
                    if (c.a > 0.9f)
                    {
                        texture.SetPixel(j, i, c);
                    }
                }
            }

            int pixelPerGrid = textureWidth / mapData.Width;
            for (int i = 0; i < mapData.EventDataArray.Length; i++)
            {
                Expression.Map.MapEvent.EventData eventData = mapData.EventDataArray[i];
                if (eventData.PageData.Length == 0)
                {
                    continue;
                }

                Texture2D eventTexture = eventData.PageData[0].GetCurrentTexture();
                if (eventTexture == null)
                {
                    continue;
                }

                int x = eventData.PosX;
                int y = eventData.PosY;
                for (int j = 0; j < pixelPerGrid; j++)
                {
                    for (int k = 0; k < pixelPerGrid; k++)
                    {
                        Color c = eventTexture.GetPixel(j, k);
                        if (c.a > 0.9f)
                        {
                            texture.SetPixel(x * pixelPerGrid + j, textureHeight - (y + 1) * pixelPerGrid + k, c);
                        }
                    }
                }
            }

            texture.Apply();

            return texture;
        }
    }
#endif
}
