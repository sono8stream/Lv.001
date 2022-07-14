using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Expression;
using Expression.Map;

namespace Hd2d
{
    // �y�b��zAssembly Info��؂蕪����ifdef���폜
#if UNITY_EDITOR
    public class Hd2dMapConverter : EditorWindow
    {
        private Material mat = null;
        private int mapDataIndex = -1;
        private const int CHIP_COUNT = 2500;// ���Ō��ߑł�
        private Hd2dTileInfo[] tileInfoArray = new Hd2dTileInfo[CHIP_COUNT];
        private Vector2 scrollPos = Vector2.zero;

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

            mat = EditorGUILayout.ObjectField("Material", mat, typeof(Material), false) as Material;

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scrollView.scrollPosition;

                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    tileInfoArray[i].type
                        = (MapBlockType)EditorGUILayout.EnumPopup("Type", tileInfoArray[i].type);
                    tileInfoArray[i].offset
                        = EditorGUILayout.Vector3Field($"Tile {i}", tileInfoArray[i].offset); 
                }
            }
        }

        private void ShowMapFilePullDown()
        {
            // �v���_�E�����j���[�ɓo�^���镶����z��
            string path = $"{Application.streamingAssetsPath}/Data/MapData";
            string[] filePaths = System.IO.Directory.GetFiles(path, "*.mps");
            string[] displayOptions = filePaths.Select(a => a.Replace($"{path}\\", "")).ToArray();

            // �v���_�E�����j���[�̍쐬
            var curIndex = displayOptions.Length > 0
            ? EditorGUILayout.Popup("MapData", mapDataIndex, displayOptions)
                : -1;

            // �`�F�b�N�I����
            if (EditorGUI.EndChangeCheck())
            {
                if (mapDataIndex != curIndex)
                {
                    RemoveExistingMap();
                    mapDataIndex = curIndex;
                    MapId id = new MapId(mapDataIndex);
                    WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id, tileInfoArray);
                    creator.Create(filePaths[curIndex]);
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
