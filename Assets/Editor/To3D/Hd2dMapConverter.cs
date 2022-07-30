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
        private Shader shad = null;
        private int mapDataIndex = -1;
        private const int CHIP_COUNT = 2500;// ���Ō��ߑł�
        private Hd2dTileInfoList tileInfoList;

        private Expression.Map.MapTile.WolfRepository repository = new Expression.Map.MapTile.WolfRepository();
        private int tileId;
        private Texture2D baseTex = null;
        private Vector2 imageScrollPos = Vector2.zero;

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
            try
            {
                string json = PlayerPrefs.GetString(saveKey);
                tileInfoList = JsonUtility.FromJson<Hd2dTileInfoList>(json);
                Debug.Log("Loaded tile data");
            }
            catch
            {
                tileInfoList = new Hd2dTileInfoList(CHIP_COUNT);
                for (int i = 0; i < CHIP_COUNT; i++)
                {
                    tileInfoList[i] = new Hd2dTileInfo(Vector3.zero, MapBlockType.Cube);
                }
                Debug.Log("Initialized tile info list");
            }
        }

        private void OnDisable()
        {
            string json = JsonUtility.ToJson(tileInfoList);
            PlayerPrefs.SetString(saveKey, json);
            Debug.Log("Saved tile data");
        }

        private void OnGUI()
        {
            // GUI
            ShowMapFilePullDown();

            shad = EditorGUILayout.ObjectField("Shader", shad, typeof(Shader), false) as Shader;

            ShowMapTiles();
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
                    DI.DependencyInjector.It().Hd2dMapDataRepository.Find(id);
                    //WolfHd2dMapFactory creator = new WolfHd2dMapFactory(id, tileInfoList, shad);
                    //creator.Create(filePaths[curIndex]);
                }
            }
        }

        private void ShowMapTiles()
        {
            float rate = 2;
            EditorGUILayout.LabelField("�}�E�X�̈ʒu", Event.current.mousePosition.ToString());
            EditorGUILayout.LabelField("�摜��̈ʒu", chipOffset.ToString());
            EditorGUILayout.LabelField("�X�N���[���o�[�̈ʒu", imageScrollPos.ToString());

            //wantsMouseMove���g�O���Ő؂�ւ�����悤��
            wantsMouseMove = EditorGUILayout.Toggle("wantsMouseMove", wantsMouseMove);

            //�}�E�X����������ĕ`��(wantsMouseMove���L���łȂ���OnGUI���̂��Ă΂�Ȃ��̖��Ӗ�)
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
                    // �y�b��z�I�[�g�`�b�v�̕���ǉ����Ă���
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
            //�摜�\��
            using (var scrollView = new EditorGUILayout.ScrollViewScope(imageScrollPos))
            {
                imageScrollPos = scrollView.scrollPosition;
                EditorGUIUtility.SetIconSize(new Vector2(baseTex.width, baseTex.height)*rate);
                GUILayout.Button(baseTex, GUIStyle.none);
                EditorGUIUtility.SetIconSize(Vector2.one);

            }

            EditorGUILayout.BeginVertical();

            tileInfoList[selectedChipIndex].type
                = (MapBlockType)EditorGUILayout.EnumPopup($"Type {selectedChipIndex}", tileInfoList[selectedChipIndex].type);
            tileInfoList[selectedChipIndex].offset
                = EditorGUILayout.Vector3Field($"Tile {selectedChipIndex}", tileInfoList[selectedChipIndex].offset);

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
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
